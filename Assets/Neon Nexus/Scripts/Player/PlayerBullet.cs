using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float bulletLifetime = 5f;
    public int enemyHitPoints = 50; // Points for regular enemies
    [SerializeField] private AudioClip laserSound;

    void Start()
    {
        Destroy(gameObject, bulletLifetime);
        if (laserSound != null)
        {
            AudioSource.PlayClipAtPoint(laserSound, transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Boss handles its own damage and bullet destruction in its OnTriggerEnter2D
        // So, we only explicitly handle non-boss enemies here.
        if (collision.CompareTag("Enemy"))
        {
            // Attempt to get an Enemy component to call a specific death method if it exists
            Enemy enemyComponent = collision.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                // Assuming the Enemy script has a method to handle its own destruction and scoring
                // For example: enemyComponent.TakeDamage(damageAmount); or enemyComponent.Die();
                // If not, default behavior:
                Destroy(collision.gameObject);
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(enemyHitPoints);
                }
            }
            else // Fallback if no Enemy component, just destroy and score
            {
                Destroy(collision.gameObject);
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(enemyHitPoints);
                }
            }
            
            Destroy(gameObject); // Destroy the bullet after hitting an enemy
        }
        // If it hits something else (like a wall or the Boss),
        // the Boss script or a generic environment collision handler would destroy the bullet.
        // If nothing else destroys it, it will self-destruct due to lifetime.
        // To ensure it's destroyed on hitting the boss if boss doesn't do it:
        // else if (collision.CompareTag("Boss")) {
        //     Destroy(gameObject); // Boss script handles damage, bullet just destroys itself
        // }
    }
}