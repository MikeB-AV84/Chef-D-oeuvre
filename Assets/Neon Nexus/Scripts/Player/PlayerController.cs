using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveActivationDistance = 1f; // The player will only move if the cursor is further than this distance
    private float originalSpeed;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCamera;

    [Header("Movement Boundaries")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    [Header("Boost System")]
    public float boostMultiplier = 3f;
    public float boostDrainRate = 15f;
    public float boostRechargeRate = 15f;
    public float maxBoost = 100f;
    public TextMeshProUGUI boostText;
    private float currentBoost;
    private bool isBoosting;
    
    [Header("Input Detection")]
    public float controllerThreshold = 0.1f;
    private bool useMouseFollow = true; // Start with mouse following enabled
    
    // Death state
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = moveSpeed;
        currentBoost = maxBoost;
        mainCamera = Camera.main;
        UpdateBoostUI();
    }

    void Update()
    {
        // --- MODIFIED CODE START ---
        // If the player is dead or the game is paused, do nothing.
        if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGamePaused()))
        {
            return;
        }
        // --- MODIFIED CODE END ---
        
        DetectInputType();
        
        if (useMouseFollow)
        {
            HandleMouseRotation();
            HandleAutomaticMovement(); 
        }
        else
        {
            HandleControllerMovement();
        }
        
        HandleShooting();
        HandleBoostInput();
    }

    void FixedUpdate()
    {
        // --- MODIFIED CODE START ---
        // If the player is dead or the game is paused, stop all movement.
        if (isDead || (GameManager.Instance != null && GameManager.Instance.IsGamePaused()))
        {
            rb.linearVelocity = Vector2.zero; // Explicitly stop any current movement
            return;
        }
        // --- MODIFIED CODE END ---
        
        ApplyMovement();
        ClampPosition();
    }

    void DetectInputType()
    {
        // Check for any controller stick movement
        bool controllerInput = Mathf.Abs(Input.GetAxis("Horizontal")) > controllerThreshold || 
                              Mathf.Abs(Input.GetAxis("Vertical")) > controllerThreshold;
        
        // Check for mouse movement
        bool mouseInput = Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
        
        if (controllerInput)
        {
            useMouseFollow = false;
        }
        else if (mouseInput)
        {
            useMouseFollow = true;
        }
    }

    /// <summary>
    /// Rotates the player to face the current mouse cursor position.
    /// </summary>
    void HandleMouseRotation()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Handles controller movement and rotation
    /// </summary>
    void HandleControllerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector2 controllerInput = new Vector2(horizontal, vertical);
        
        if (controllerInput.magnitude > controllerThreshold)
        {
            moveInput = controllerInput.normalized;
            
            // Rotate player to face movement direction
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    /// <summary>
    /// Moves the player towards the cursor if it's outside the activation distance.
    /// </summary>
    void HandleAutomaticMovement()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float distanceToCursor = Vector2.Distance(transform.position, mouseWorldPos);

        if (distanceToCursor > moveActivationDistance)
        {
            moveInput = transform.up;
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    /// <summary>
    /// Handles the shooting input.
    /// </summary>
    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1")) // Left Mouse Button
        {
            Shoot();
        }
    }

    /// <summary>
    /// Handles the boost input, activating when left shift is held and the player is moving.
    /// </summary>
    void HandleBoostInput()
    {
        bool boostInput = Input.GetKey(KeyCode.Space) || Input.GetAxis("RightTrigger") > 0.1f;
        
        if (boostInput && currentBoost > 0 && moveInput != Vector2.zero)
        {
            isBoosting = true;
            moveSpeed = originalSpeed * boostMultiplier;
            currentBoost = Mathf.Max(0, currentBoost - boostDrainRate * Time.deltaTime);
        }
        else
        {
            isBoosting = false;
            moveSpeed = originalSpeed;
            if (currentBoost < maxBoost)
            {
               currentBoost = Mathf.Min(maxBoost, currentBoost + boostRechargeRate * Time.deltaTime);
            }
        }
        
        UpdateBoostUI();
    }

    void ApplyMovement()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void ClampPosition()
    {
        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        rb.position = clampedPosition;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        
        Vector2 bulletVelocity = firePoint.up * bulletSpeed;
        
        if (isBoosting)
        {
            bulletVelocity += rb.linearVelocity;
        }
        
        rbBullet.linearVelocity = bulletVelocity;
        Destroy(bullet, 3f);
    }

    void UpdateBoostUI()
    {
        if (boostText != null)
        {
            boostText.text = $"{Mathf.RoundToInt(currentBoost)}%";
        }
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        float tempOriginalSpeed = originalSpeed;
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = tempOriginalSpeed;
    }
    
    public void SetPlayerDead(bool dead)
    {
        isDead = dead;
        
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;
        }
    }
}