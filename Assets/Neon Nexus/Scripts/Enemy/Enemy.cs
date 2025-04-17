using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int pointsValue = 50;
    public GameObject shieldPickupPrefab;
    public GameObject heartPickupPrefab;
    [Header("Drop Chances")]
    //[Range(0f, 1f)] public float shieldDropChance;
    //[Range(0f, 1f)] public float heartDropChance;
    [SerializeField] private float dropChance = 0.15f;
    [SerializeField] private float shieldChance = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
{
    /*float dropRoll = Random.value;
    
    if (dropRoll <= 0.15f) // 15% total drop chance
    {
        bool dropShield = Random.value < 0.5f; // 50/50 split
        
        if (dropShield && shieldPickupPrefab != null)
            Instantiate(shieldPickupPrefab, transform.position, Quaternion.identity);
        else if (heartPickupPrefab != null)
            Instantiate(heartPickupPrefab, transform.position, Quaternion.identity);
    }*/

    float dropRoll = Random.value;

    if (dropRoll <= dropChance)
    {
        bool dropShield = Random.value < shieldChance;

        if (dropShield && shieldPickupPrefab != null)
            Instantiate(shieldPickupPrefab, transform.position, Quaternion.identity);
        else if (heartPickupPrefab != null)
            Instantiate(heartPickupPrefab, transform.position, Quaternion.identity);
}


        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsValue);
        }
        Destroy(gameObject);
    }
}