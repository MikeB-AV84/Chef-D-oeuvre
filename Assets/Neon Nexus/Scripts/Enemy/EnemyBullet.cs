using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float bulletLifetime = 5f;
    private GameObject enemyShooter;
    
    void Start()
    {
        Destroy(gameObject, bulletLifetime);
        
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        Collider2D bulletCollider = GetComponent<Collider2D>();
        
        foreach (GameObject enemy in allEnemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider, true);
            }
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

        // Otherwise take damage and blink
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        DamageBlinker blinker = collision.GetComponent<DamageBlinker>();
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
            if (blinker != null) blinker.TriggerBlink();
        }
        
        Destroy(gameObject);
    }
}
}