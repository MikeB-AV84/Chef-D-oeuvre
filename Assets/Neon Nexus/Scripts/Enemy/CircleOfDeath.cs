using UnityEngine;

public class CircleOfDeath : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform[] shootingPoints = new Transform[4]; // Top, Right, Down, Left
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private float bulletSpeed = 5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 45f; // degrees per second
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    
    [Header("Score Settings")]
    [SerializeField] private int scoreValue = 300;
    
    private float shootTimer;
    private GameManager gameManager;
    
    void Start()
    {
        currentHealth = maxHealth;
        gameManager = FindObjectOfType<GameManager>();
        
        // Validate shooting points
        if (shootingPoints.Length != 4)
        {
            Debug.LogError("CircleOfDeath needs exactly 4 shooting points!");
        }
        
        // Start shooting immediately
        shootTimer = shootInterval;
    }
    
    void Update()
    {
        RotateEnemy();
        HandleShooting();
    }
    
    private void RotateEnemy()
    {
        // Rotate the enemy continuously
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    private void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        
        if (shootTimer <= 0f)
        {
            ShootFromAllPoints();
            shootTimer = shootInterval;
        }
    }
    
    private void ShootFromAllPoints()
    {
        // Shoot from all 4 points simultaneously
        for (int i = 0; i < shootingPoints.Length; i++)
        {
            if (shootingPoints[i] != null)
            {
                ShootBullet(shootingPoints[i]);
            }
        }
    }
    
    private void ShootBullet(Transform shootPoint)
    {
        if (enemyBulletPrefab == null) return;
        
        // Create bullet at shoot point position
        GameObject bullet = Instantiate(enemyBulletPrefab, shootPoint.position, shootPoint.rotation);
        
        // Get the bullet's rigidbody and apply velocity
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            // Shoot in the direction the shoot point is facing
            bulletRb.linearVelocity = shootPoint.up * bulletSpeed;
        }
        
        // Alternative: If bullets don't have Rigidbody2D, use EnemyBullet script
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootPoint.up);
            bulletScript.SetSpeed(bulletSpeed);
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Optional: Add hit effect here
        // For example: flash red, play sound, etc.
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Add score to game manager
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }
        
        // Optional: Add death effects here
        // For example: explosion particle effect, death sound, etc.
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player bullets
        if (other.CompareTag("PlayerBullet"))
        {
            PlayerBullet bullet = other.GetComponent<PlayerBullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.GetDamage());
                bullet.DestroyBullet();
            }
        }
    }
    
    // Method to be called by GameManager when spawning
    public void Initialize(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
    }
    
    // Public getters for inspector debugging
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetShootTimer()
    {
        return shootTimer;
    }
}

// If you need a separate spawning script for GameManager integration:
[System.Serializable]
public class CircleOfDeathSpawner
{
    [SerializeField] private GameObject circleOfDeathPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private int scoreThreshold = 300;
    
    private bool hasSpawned = false;
    
    public bool ShouldSpawn(int currentScore)
    {
        return !hasSpawned && currentScore >= scoreThreshold;
    }
    
    public GameObject SpawnCircleOfDeath()
    {
        if (circleOfDeathPrefab == null) return null;
        
        GameObject spawnedEnemy = Object.Instantiate(circleOfDeathPrefab, spawnPosition, Quaternion.identity);
        CircleOfDeath circleScript = spawnedEnemy.GetComponent<CircleOfDeath>();
        
        if (circleScript != null)
        {
            circleScript.Initialize(spawnPosition);
        }
        
        hasSpawned = true;
        return spawnedEnemy;
    }
    
    public void Reset()
    {
        hasSpawned = false;
    }
}