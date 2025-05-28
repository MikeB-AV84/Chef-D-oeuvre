using UnityEngine;
using System.Collections;

public class HyperDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 50f; // This is now an effective speed due to duration
    public float dashDistance = 10f;
    public float dashDuration = 0.2f; // Made duration shorter for a quicker dash feel
    public float dashCooldown = 3f;
    
    [Header("Visual Effects")]
    public Color glowColor = new Color(0.5f, 0.8f, 1f, 0.8f);
    public float glowIntensity = 2f; // Not directly used without custom shader
    public GameObject trailPrefab; 
    
    [Header("Combat Settings")]
    public int enemyHitPoints = 50;
    public int bossDashDamage = 1; // Damage dealt to boss with a dash
    public LayerMask targetLayerMask = -1; // Combined layer mask for enemies and boss
    
    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;

    [Header("Debug")]
    public bool isDashing = false;
    private bool canDash = true;
    private Vector2 dashDirection;
    private Vector2 startPosition;
    private Vector2 targetPosition; // Use this for lerp target
    private float dashTimer;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material originalMaterial; // Store original material
    private Material glowMaterialInstance; // Instance for glow to avoid modifying shared material
    private GameObject activeTrail;
    
    private PlayerController playerController;
    private Rigidbody2D rb; // For physics-based movement if preferred, or for collision detection settings
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalMaterial = spriteRenderer.material; // Get the original material
            
            // Create an instance of a glow material if you have a specific glow shader
            // For simple color change, direct spriteRenderer.color is enough.
            // If using a material property for glow:
            // glowMaterialInstance = new Material(originalMaterial); 
            // glowMaterialInstance.SetColor("_GlowColor", glowColor); // Example property
        }
    }
    
    void Update()
    {
        HandleInput();
        
        // Dash movement is now handled in FixedUpdate if using Rigidbody, or Update if transform-based
    }

    void FixedUpdate() // Good for Rigidbody movement
    {
        if (isDashing)
        {
            UpdateDashMovement();
        }
    }
    
    void HandleInput()
    {
        if (!canDash || isDashing || (playerController != null && !playerController.enabled)) return; // Check if player controller is enabled
        
        bool dashInput = Input.GetMouseButtonDown(1) || Input.GetAxis("LeftTrigger") > 0.5f;
        
        if (dashInput)
        {
            AttemptDash();
        }
    }
    
    void AttemptDash()
    {
        Vector2 inputDirection = GetInputDirection();
        if (inputDirection == Vector2.zero)
        {
            inputDirection = transform.up; // Or character's current facing direction from PlayerController
        }
        
        StartDash(inputDirection.normalized);
    }

    Vector2 GetInputDirection()
    {
        // Assuming PlayerController might have a method for current aim/move direction
        // Or use raw input like before:
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(horizontal, vertical);
    }
    
    void StartDash(Vector2 direction)
    {
        dashDirection = direction;
        startPosition = transform.position; // Or rb.position if using Rigidbody strictly
        targetPosition = startPosition + dashDirection * dashDistance;
        
        isDashing = true;
        canDash = false;
        dashTimer = 0f;
        
        if (playerController != null) playerController.enabled = false; // Disable normal controls
        if (rb != null) rb.isKinematic = true; // Optional: make kinematic to prevent physics interference during dash

        StartCoroutine(DashEffectsAndLogic());
        
        if (dashSound != null && AudioManager.Instance != null) // Play via AudioManager if it handles 3D sounds
        {
            // For simplicity, PlayClipAtPoint is fine for one-shots
             AudioSource.PlayClipAtPoint(dashSound, transform.position);
        }
        
        // Debug.Log("HyperDash activated!");
    }
    
    IEnumerator DashEffectsAndLogic()
    {
        // Start visual effects
        if (trailPrefab != null)
        {
            activeTrail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
            activeTrail.transform.SetParent(transform, true); // World position stays, but moves with player
        }
        if (spriteRenderer != null) spriteRenderer.color = glowColor; // Apply glow color
        
        // Dash Duration Loop (moved from FixedUpdate to here to control timing precisely)
        // Movement will happen in FixedUpdate based on isDashing flag
        float currentDashTime = 0f;
        while(currentDashTime < dashDuration)
        {
            currentDashTime += Time.deltaTime; // Use normal deltaTime for timers
            // The actual movement and collision check can remain in FixedUpdate or be integrated here
            // For this example, movement is in FixedUpdate. We just wait for duration here.
            yield return null; 
        }

        EndDash();
    }

    void UpdateDashMovement() // Called from FixedUpdate
    {
        // This method assumes transform-based movement for simplicity of Lerp
        // If using Rigidbody, rb.MovePosition() is better.
        dashTimer += Time.fixedDeltaTime; // Use fixedDeltaTime here
        float dashProgress = Mathf.Clamp01(dashTimer / dashDuration);
        
        Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, dashProgress);

        // Check for targets between current position and new position
        // Use a cast from the *previous* frame's position to the new calculated position
        float segmentDistance = Vector2.Distance(transform.position, newPosition);
        if (segmentDistance > 0.01f) // Only cast if moving significantly
        {
            CheckForTargetsInPath((Vector2)transform.position, newPosition);
        }
        
        transform.position = newPosition; // Move the transform

        // If dashProgress >= 1f, the DashEffectsAndLogic coroutine will call EndDash.
    }

    void CheckForTargetsInPath(Vector2 fromPos, Vector2 toPos)
    {
        Vector2 castDirection = (toPos - fromPos).normalized;
        float castDistance = Vector2.Distance(fromPos, toPos);

        // Make capsule size relative to player or a fixed small size
        RaycastHit2D[] hits = Physics2D.CapsuleCastAll(
            fromPos, 
            GetComponent<Collider2D>() != null ? GetComponent<Collider2D>().bounds.size : Vector2.one * 0.5f, // Use collider size or default
            CapsuleDirection2D.Vertical, // Or Horizontal, depending on sprite
            Vector2.Angle(Vector2.up, castDirection), // Angle for the capsule
            castDirection, 
            castDistance, 
            targetLayerMask // Use combined layer mask
        );
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.CompareTag("Enemy"))
            {
                // Get Enemy component for proper destruction and scoring
                Enemy enemyComponent = hit.collider.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.DestroyEnemy(); // Assuming Enemy script has this method
                }
                else
                {
                    Destroy(hit.collider.gameObject); // Fallback
                    if(ScoreManager.Instance != null) ScoreManager.Instance.AddScore(enemyHitPoints);
                }
                // Debug.Log("Enemy destroyed by HyperDash!");
            }
            else if (hit.collider.CompareTag("Boss")) // Check for Boss tag
            {
                Boss bossComponent = hit.collider.GetComponent<Boss>();
                if (bossComponent != null)
                {
                    bossComponent.TakeDamage(bossDashDamage);
                    // Debug.Log($"Dashed into Boss, dealing {bossDashDamage} damage!");
                }
            }
        }
    }
    
    void EndDash()
    {
        if (!isDashing) return; // Prevent multiple calls

        isDashing = false;
        transform.position = targetPosition; // Ensure final position

        if (playerController != null) playerController.enabled = true;
        if (rb != null) rb.isKinematic = false; // Revert kinematic state if changed

        // Stop visual effects
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (activeTrail != null) Destroy(activeTrail, 1f); // Destroy trail after a delay
        
        StartCoroutine(DashCooldownRoutine());
        // Debug.Log("HyperDash completed!");
    }
        
    IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        // Debug.Log("HyperDash ready!");
    }
    
    public bool IsDashAvailable() => canDash && !isDashing;
    
    public float GetCooldownProgress() // More accurate cooldown progress
    {
        if (canDash) return 1f;
        if (isDashing) return 0f; // Or some value indicating active dash
        
        // This requires tracking when cooldown started
        // For simplicity, the current version doesn't provide fine-grained progress.
        // You would need to store `Time.time` when cooldown starts and compare.
        return 0f; // Placeholder for "on cooldown"
    }
}