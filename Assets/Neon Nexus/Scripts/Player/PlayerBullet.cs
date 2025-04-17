using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float bulletLifetime = 5f; // Time before bullet auto-destructs
    public int enemyHitPoints = 50; // Points awarded for hitting an enemy
    [SerializeField] private AudioClip laserSound;

    void Start()
    {
        // Auto-destruct after lifetime expires
        Destroy(gameObject, bulletLifetime);
        AudioSource.PlayClipAtPoint(laserSound, transform.position);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy
        if (collision.CompareTag("Enemy"))
        {
            // Destroy the enemy
            Destroy(collision.gameObject);
            
            // Add points through ScoreManager
            ScoreManager.Instance?.AddScore(enemyHitPoints);
            
            // Destroy the bullet
            Destroy(gameObject);
        }
    }
}