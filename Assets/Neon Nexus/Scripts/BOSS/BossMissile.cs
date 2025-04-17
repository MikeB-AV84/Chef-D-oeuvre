using UnityEngine;

public class BossMissile : MonoBehaviour
{
    public float lifetime = 5f;
    public float speed = 5f;
    public float rotationSpeed = 200f;
    public float trackingDuration = 3f; // Time before missile stops tracking
    public int damage = 3;
    
    private Transform player;
    private Rigidbody2D rb;
    private float trackingTimer;
    private bool isTracking = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        trackingTimer = trackingDuration;
        Destroy(gameObject, lifetime);
        
        // Ignore collisions with enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Collider2D missileCollider = GetComponent<Collider2D>();
        
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null && missileCollider != null)
            {
                Physics2D.IgnoreCollision(missileCollider, enemyCollider);
            }
        }
    }

    void FixedUpdate()
    {
        if (isTracking && player != null)
        {
            // Update tracking timer
            trackingTimer -= Time.fixedDeltaTime;
            if (trackingTimer <= 0)
            {
                isTracking = false;
                return;
            }

            // Calculate direction to player
            Vector2 direction = (Vector2)player.position - rb.position;
            direction.Normalize();

            // Calculate rotation amount
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            // Apply rotation
            rb.angularVelocity = -rotateAmount * rotationSpeed;

            // Apply forward velocity
            rb.linearVelocity = transform.up * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerShield playerShield = collision.GetComponent<PlayerShield>();
            
            // Shield absorbs the hit if active
            if (playerShield != null && playerShield.AbsorbDamage())
            {
                Destroy(gameObject);
                return;
            }

            // Otherwise take damage
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            DamageBlinker blinker = collision.GetComponent<DamageBlinker>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                if (blinker != null) blinker.TriggerBlink();
            }
            
            Destroy(gameObject);
        }
    }
}