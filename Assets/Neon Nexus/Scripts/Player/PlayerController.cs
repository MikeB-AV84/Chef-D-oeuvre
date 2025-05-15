using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private float originalSpeed;
    private Rigidbody2D rb;
    private Vector2 moveInput;

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
    
    // Death state
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalSpeed = moveSpeed;
        currentBoost = maxBoost;
        UpdateBoostUI();
    }

    void Update()
    {
        // Don't process inputs if player is dead
        if (isDead) return;
        
        HandleMovementInput();
        HandleRotation();
        HandleShooting();
        HandleBoostInput();
    }

    void FixedUpdate()
    {
        // Don't apply movement if player is dead
        if (isDead) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        ApplyMovement();
        ClampPosition();

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void HandleRotation()
    {
        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void HandleBoostInput()
    {
        bool boostInput = Input.GetKey(KeyCode.LeftShift) || Input.GetAxis("RightTrigger") > 0.5f;
        
        if (boostInput && currentBoost > 0)
        {
            isBoosting = true;
            moveSpeed = originalSpeed * boostMultiplier;
            currentBoost = Mathf.Max(0, currentBoost - boostDrainRate * Time.deltaTime);
        }
        else
        {
            isBoosting = false;
            moveSpeed = originalSpeed;
            currentBoost = Mathf.Min(maxBoost, currentBoost + boostRechargeRate * Time.deltaTime);
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
        
        // Base bullet velocity
        Vector2 bulletVelocity = firePoint.up * bulletSpeed;
        
        // Add player velocity if boosting
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
            boostText.text = $"BOOST: {Mathf.RoundToInt(currentBoost)}/{maxBoost}";
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
        originalSpeed = moveSpeed;
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }
    
    // New method for DeathScreenManager to call
    public void SetPlayerDead(bool dead)
    {
        isDead = dead;
        
        // If dead, stop all movement
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;
        }
    }
}