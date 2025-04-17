using UnityEngine;
using System.Collections; // Add this namespace for IEnumerator

public class ShieldBubbleFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitFlashDuration = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void PlayHitEffect()
    {
        // Visual flash
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashShield());
        }
        
        // Particles
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        
        // Sound
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);
    }
    
    private IEnumerator FlashShield()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
}