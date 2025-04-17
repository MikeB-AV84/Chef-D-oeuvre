using UnityEngine;
using System.Collections;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] private float lifetime = 60f;
    [SerializeField] private float blinkDuration = 10f; // Start blinking last 10 seconds
    [SerializeField] private float blinkSpeed = 0.3f;
    
    private SpriteRenderer _sprite;
    private bool _isBlinking = false;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifetime);
        StartCoroutine(StartBlinkBeforeDestroy());
    }

    IEnumerator StartBlinkBeforeDestroy()
    {
        // Wait until X seconds before destruction
        yield return new WaitForSeconds(lifetime - blinkDuration);
        
        _isBlinking = true;
        float elapsed = 0f;
        
        // Blink until destroyed
        while (_isBlinking)
        {
            _sprite.enabled = !_sprite.enabled;
            elapsed += blinkSpeed;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    void OnDestroy()
    {
        _isBlinking = false; // Stop blinking when destroyed
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerShield playerShield = collision.GetComponent<PlayerShield>();
            if (playerShield != null && !playerShield.HasShield())
            {
                playerShield.ActivateShield();
                Destroy(gameObject);
            }
        }
    }
}