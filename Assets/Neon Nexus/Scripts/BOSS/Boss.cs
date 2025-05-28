using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    public delegate void BossDefeatedEvent();
    public event BossDefeatedEvent OnBossDefeated;

    [Header("Boss Stats")]
    public int maxHealth = 50;
    private int currentHealth;
    public string bossTag = "Boss"; // Make sure prefab has this tag

    [Header("Combat")]
    public GameObject missilePrefab;
    public Transform[] missileLaunchPoints;
    public float missileSpeed = 5f;
    public float timeBetweenMissiles = 0.2f;
    private bool isFiring = false;


    [Header("Movement")]
    public float moveSpeed = 1f;
    public float rotationSpeed = 40f;
    public float rotationSmoothing = 7f; // Not used in current rotation logic

    [Header("Rewards")]
    public GameObject heartPickupPrefab;
    public GameObject shieldPickupPrefab;
    public int heartDropAmount = 4;
    public int shieldDropAmount = 1;
    public int scoreForDefeat = 500;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer; // Assign in Inspector or get in Start
    public Color hitFlashColor = Color.red;
    public float hitFlashDuration = 0.1f;
    private Color originalColor;


    private Transform player;
    

    void Start()
    {
        gameObject.tag = bossTag; // Ensure tag is set
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        
        currentHealth = maxHealth;

        // Inform the Health Bar (BossSpawner usually shows it, Boss updates it)
        // If BossHealthBarUI is directly referenced here:
        // if (BossHealthBarUI.Instance != null)
        // {
        // BossHealthBarUI.Instance.ShowHealthBar(maxHealth); // Or Spawner does this
        // }
    }

    void Update()
    {
        if (player == null) return; // No player, no action

        if (!isFiring)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            // Smoother rotation using Quaternion.RotateTowards
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // Already defeated

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth); // Ensure health doesn't go below 0

        Debug.Log($"Boss took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");

        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.UpdateHealth(currentHealth);
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(HitFlash());
        }
        
        if (currentHealth <= 0)
        {
            DefeatBoss();
        }
    }

    IEnumerator HitFlash()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        if (spriteRenderer != null) spriteRenderer.color = originalColor; // Check if still exists
    }

    void DefeatBoss()
    {
        Debug.Log("Boss Defeated!");
        // Spawn rewards
        for (int i = 0; i < heartDropAmount; i++)
        {
            if (heartPickupPrefab != null)
                Instantiate(heartPickupPrefab, transform.position + (Vector3)Random.insideUnitCircle * 2f, Quaternion.identity);
        }
        for (int i = 0; i < shieldDropAmount; i++)
        {
            if (shieldPickupPrefab != null)
                Instantiate(shieldPickupPrefab, transform.position + (Vector3)Random.insideUnitCircle * 1f, Quaternion.identity);
        }
        
        // Award score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreForDefeat);
        }
        
        OnBossDefeated?.Invoke(); // Signal defeat (BossSpawner listens to this)
        Destroy(gameObject); // Boss is defeated
    }

    // Called by an external system, e.g. BossAI or animation event
    public void StartMissileAttack()
    {
        if (!isFiring)
        {
            StartCoroutine(FireMissilesRoutine());
        }
    }

    public IEnumerator FireMissilesRoutine() // Renamed from FireMissiles to avoid confusion if used as public API
    {
        isFiring = true;
        yield return new WaitForSeconds(1f); 
        
        for (int i = 0; i < missileLaunchPoints.Length; i++)
        {
            if (missileLaunchPoints[i] != null && missilePrefab != null)
            {
                Transform launchPoint = missileLaunchPoints[i];
                GameObject missile = Instantiate(missilePrefab, launchPoint.position, launchPoint.rotation);
                
                Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = launchPoint.up * missileSpeed; // Use velocity for physics-based movement
                }
                
                yield return new WaitForSeconds(timeBetweenMissiles);
            }
        }
        
        isFiring = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            TakeDamage(1); // Player bullets deal 1 damage
            Destroy(collision.gameObject); // Destroy the bullet
        }
        // Dash damage is handled by HyperDash script detecting collision with "Boss" tag
    }
}