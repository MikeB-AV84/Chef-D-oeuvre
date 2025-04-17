using UnityEngine;
using System.Collections;

public class DamageBlinker : MonoBehaviour
{
    [SerializeField] private float blinkDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Color blinkColor = Color.red;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isBlinking = false;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void TriggerBlink()
    {
        if (!isBlinking && spriteRenderer != null)
        {
            StartCoroutine(BlinkRoutine());
        }
    }
    
    private IEnumerator BlinkRoutine()
    {
        isBlinking = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < blinkDuration)
        {
            // Toggle between blink color and original color
            spriteRenderer.color = (spriteRenderer.color == originalColor) ? blinkColor : originalColor;
            
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }
        
        // Ensure we return to original color
        spriteRenderer.color = originalColor;
        isBlinking = false;
    }
}