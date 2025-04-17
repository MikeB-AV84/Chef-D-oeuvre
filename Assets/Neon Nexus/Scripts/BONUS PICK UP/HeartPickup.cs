using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 15f); // Auto-destroy after 15 seconds
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null && health.CanGainLife())
            {
                health.AddLife();
                Destroy(gameObject);
            }
        }
    }
}