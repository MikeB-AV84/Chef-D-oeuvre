using UnityEngine;
using UnityEngine.UI; // Required for Slider
using TMPro;         // Required for TextMeshProUGUI

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance; // Singleton

    [Header("UI Elements")]
    public GameObject healthBarHolder; // Assign the parent GameObject holding the slider and text
    public Slider healthSlider;            // Assign the Slider UI element in Inspector
    public TextMeshProUGUI bossNameText;   // Assign the TextMeshProUGUI element in Inspector

    [Header("Boss Info")]
    public string bossName = "Star Destroyer: The Whistler";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (healthBarHolder != null)
        {
            healthBarHolder.SetActive(false); // Start hidden
        }
        else
        {
            Debug.LogError("BossHealthBarUI: Health Bar Holder is not assigned!");
        }
    }

    public void ShowHealthBar(float maxHealthValue)
    {
        if (healthBarHolder == null || healthSlider == null || bossNameText == null)
        {
            Debug.LogError("BossHealthBarUI: One or more UI elements are not assigned!");
            return;
        }

        bossNameText.text = bossName;
        healthSlider.maxValue = maxHealthValue;
        healthSlider.value = maxHealthValue;
        healthBarHolder.SetActive(true);
        // Optional: Add fade-in animation here if desired
    }

    public void UpdateHealth(float currentHealthValue)
    {
        if (healthSlider == null) return;
        healthSlider.value = currentHealthValue;
    }

    public void HideHealthBar()
    {
        if (healthBarHolder == null) return;
        healthBarHolder.SetActive(false);
        // Optional: Add fade-out animation here if desired
    }
}