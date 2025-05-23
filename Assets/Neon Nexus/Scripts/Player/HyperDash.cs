using UnityEngine;
using System.Collections;

public class HyperDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 50f;
    public float dashDistance = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 3f;
    
    [Header("Visual Effects")]
    public Color glowColor = new Color(0.5f, 0.8f, 1f, 0.8f); // Light blue
    public float glowIntensity = 2f;
    public GameObject trailPrefab; // Assign a trail renderer prefab in inspector
    
    [Header("Combat Settings")]
    public int enemyHitPoints = 50; // Points awarded for destroying enemies during dash
    public LayerMask enemyLayerMask = -1; // Which layers to check for enemies
    
    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;

    [Header("Debug")]
    
    public bool isDashing = false;
    private bool canDash = true;
    private Vector2 dashDirection;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float dashTimer;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material originalMaterial;
    private Material glowMaterial;
    private GameObject activeTrail;
    
    // Input components
    private PlayerController playerController;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalMaterial = spriteRenderer.material;
            
            // Create glow material (you might want to create a proper glow shader)
            glowMaterial = new Material(originalMaterial);
        }
    }
    
    void Update()
    {
        HandleInput();
        
        if (isDashing)
        {
            UpdateDash();
        }
    }
    
    void HandleInput()
    {
        if (!canDash || isDashing) return;
        
        bool dashInput = false;
        
        // Mouse input (Right Click)
        if (Input.GetMouseButtonDown(1))
        {
            dashInput = true;
        }
        
        // Xbox controller input (Left Trigger)
        if (Input.GetAxis("LeftTrigger") > 0.5f)
        {
            dashInput = true;
        }
        
        if (dashInput)
        {
            StartDash();
        }
    }
    
    void StartDash()
    {
        if (!canDash) return;
        
        // Get movement direction from input or current facing direction
        Vector2 inputDirection = GetInputDirection();
        
        if (inputDirection == Vector2.zero)
        {
            // If no input, dash in the direction the player is facing
            inputDirection = transform.up; // Adjust based on your player's forward direction
        }
        
        dashDirection = inputDirection.normalized;
        startPosition = transform.position;
        targetPosition = startPosition + dashDirection * dashDistance;
        
        isDashing = true;
        canDash = false;
        dashTimer = 0f;
        
        // Disable normal movement during dash
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Start visual effects
        StartCoroutine(DashVisualEffects());
        
        // Play sound
        if (dashSound != null)
        {
            AudioSource.PlayClipAtPoint(dashSound, transform.position);
        }
        
        Debug.Log("HyperDash activated!");
    }
    
    Vector2 GetInputDirection()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(horizontal, vertical);
    }
    
    void UpdateDash()
    {
        dashTimer += Time.deltaTime;
        float dashProgress = dashTimer / dashDuration;
        
        if (dashProgress >= 1f)
        {
            // Dash completed
            transform.position = targetPosition;
            EndDash();
            return;
        }
        
        // Smooth dash movement
        Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, dashProgress);
        
        // Check for enemies between current position and new position
        CheckForEnemiesInPath(transform.position, newPosition);
        
        transform.position = newPosition;
    }
    
    void CheckForEnemiesInPath(Vector2 fromPos, Vector2 toPos)
    {
        // Use a capsule cast or multiple sphere casts to detect enemies along the path
        Vector2 direction = (toPos - fromPos).normalized;
        float distance = Vector2.Distance(fromPos, toPos);
        
        // Perform a capsule cast to detect all enemies in the dash path
        RaycastHit2D[] hits = Physics2D.CapsuleCastAll(
            fromPos, 
            new Vector2(1f, 1f), // Adjust size as needed
            CapsuleDirection2D.Horizontal, 
            0f, 
            direction, 
            distance, 
            enemyLayerMask
        );
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                DestroyEnemy(hit.collider.gameObject);
            }
        }
    }
    
    void DestroyEnemy(GameObject enemy)
    {
        // Check if the enemy has an Enemy component
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // Use the enemy's own destruction method to handle scoring and drops
            enemyComponent.DestroyEnemy();
        }
        else
        {
            // Fallback: destroy directly and add points manually
            Destroy(enemy);
            ScoreManager.Instance?.AddScore(enemyHitPoints);
        }
        
        Debug.Log("Enemy destroyed by HyperDash!");
    }
    
    void EndDash()
    {
        isDashing = false;
        
        // Re-enable normal movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Start cooldown
        StartCoroutine(DashCooldown());
        
        Debug.Log("HyperDash completed!");
    }
    
    IEnumerator DashVisualEffects()
    {
        // Create trail effect
        if (trailPrefab != null)
        {
            activeTrail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
            activeTrail.transform.SetParent(transform);
        }
        
        // Apply glow effect
        if (spriteRenderer != null)
        {
            spriteRenderer.material = glowMaterial;
            spriteRenderer.color = glowColor;
        }
        
        // Keep effects active during dash
        yield return new WaitWhile(() => isDashing);
        
        // Remove effects after dash
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            spriteRenderer.color = originalColor;
        }
        
        // Destroy trail after a short delay
        if (activeTrail != null)
        {
            Destroy(activeTrail, 1f);
        }
    }
    
    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        Debug.Log("HyperDash ready!");
    }
    
    // Public method to check if dash is available (useful for UI indicators)
    public bool IsDashAvailable()
    {
        return canDash && !isDashing;
    }
    
    // Public method to get cooldown progress (useful for UI)
    public float GetCooldownProgress()
    {
        if (canDash) return 1f;
        // You might want to track cooldown progress more precisely
        return 0f;
    }
}