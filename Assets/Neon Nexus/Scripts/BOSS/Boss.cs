using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    public delegate void BossDefeatedEvent();
    public event BossDefeatedEvent OnBossDefeated;

    public GameObject missilePrefab;
    public GameObject heartPickupPrefab;
    public GameObject shieldPickupPrefab;
    public float moveSpeed = 1f;
    public int hitsToDefeat = 30;
    
    private Transform player;
    private int currentHits;
    private bool isFiring = false;
    private SpriteRenderer spriteRenderer;
    public Transform[] missileLaunchPoints; // Assign these in Inspector to specific sprite locations
    public float missileSpeed = 5f;
    public float timeBetweenMissiles = 0.2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 40f; // Degrees per second
    public float rotationSmoothing = 7f; // Higher = smoother but slower turns

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isFiring && player != null)
        {
            // Movement remains the same
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        
        // New smoothed rotation code:
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void RegisterHit()
    {
        currentHits++;
        StartCoroutine(HitFlash());
        
        if (currentHits >= hitsToDefeat)
        {
            DefeatBoss();
        }
    }

    IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void DefeatBoss()
    {
        // Spawn rewards
        for (int i = 0; i < 4; i++)
        {
            Instantiate(heartPickupPrefab, transform.position + Random.insideUnitSphere * 2f, Quaternion.identity);
        }
        Instantiate(shieldPickupPrefab, transform.position, Quaternion.identity);
        
        OnBossDefeated?.Invoke();
        Destroy(gameObject);
    }

    public IEnumerator FireMissiles()
    {
        isFiring = true;
        yield return new WaitForSeconds(1f); // Stop before firing
        
        // Fire missiles sequentially from each launch point
        for (int i = 0; i < missileLaunchPoints.Length; i++)
        {
            if (missileLaunchPoints[i] != null)
            {
                Transform launchPoint = missileLaunchPoints[i];
                GameObject missile = Instantiate(missilePrefab, launchPoint.position, launchPoint.rotation);
                
                // Set velocity in the direction the launch point is facing
                Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = launchPoint.up * missileSpeed;
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
            Destroy(collision.gameObject);
            RegisterHit();
        }
    }
}
