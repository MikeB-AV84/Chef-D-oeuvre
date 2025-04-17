using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public GameObject shieldBubblePrefab;
    private GameObject currentShield;
    private int hitsRemaining;
    private const int MAX_SHIELD_HITS = 3;
    private DamageBlinker damageBlinker; // Reference to blinker

    void Start()
    {
        damageBlinker = GetComponent<DamageBlinker>(); // Get the blinker component
    }

    public void ActivateShield()
    {
        if (currentShield != null) return;
        
        currentShield = Instantiate(shieldBubblePrefab, transform);
        currentShield.transform.localPosition = Vector3.zero;
        hitsRemaining = MAX_SHIELD_HITS;
    }

    public bool AbsorbDamage()
    {
        if (!HasShield()) return false;
        
        hitsRemaining--;
        
        // Trigger shield hit effect
        if (currentShield != null)
        {
            currentShield.GetComponent<ShieldBubbleFX>()?.PlayHitEffect();
        }
        
        if (hitsRemaining <= 0)
        {
            RemoveShield();
        }
        return true;
    }

    public void RemoveShield()
    {
        if (currentShield != null)
        {
            Destroy(currentShield);
        }
    }

    public bool HasShield() => currentShield != null;
}