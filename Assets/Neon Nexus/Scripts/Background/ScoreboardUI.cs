using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ScoreboardUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform scoreEntryContainer;
    public GameObject scoreEntryPrefab;
    public TextMeshProUGUI noScoresText;
    
    [Header("Style")]
    public Color firstPlaceColor = Color.yellow;
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f); // Silver
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);     // Bronze
    
    [Header("Back Button (Optional)")]
    public Button backButton;
    
    [Header("Controller Settings")]
    public float selectedScale = 1.2f;
    public float scaleSpeed = 5f;
    
    private Vector3 originalBackButtonScale;
    private Button hoveredButton;
    private Button currentlySelected;
    
    void OnEnable()
    {
        UpdateScoreboard();
        
        // If back button exists, set it up for hover effects
        if (backButton != null)
        {
            originalBackButtonScale = backButton.transform.localScale;
            AddHoverEvent(backButton);
            SetSelectedButton(backButton);
        }
    }
    
    void Start()
    {
        // Store original scale if back button exists
        if (backButton != null)
        {
            originalBackButtonScale = backButton.transform.localScale;
            AddHoverEvent(backButton);
        }
    }
    
    void Update()
    {
        // Update button scaling if back button exists
        if (backButton != null)
        {
            UpdateButtonScaling();
        }
    }
    
    void AddHoverEvent(Button button)
    {
        // Get or add EventTrigger component
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Create mouse enter event
        EventTrigger.Entry enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((eventData) => OnMouseEnter(button));
        eventTrigger.triggers.Add(enterEvent);
        
        // Create mouse exit event
        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((eventData) => OnMouseExit(button));
        eventTrigger.triggers.Add(exitEvent);
    }
    
    void OnMouseEnter(Button button)
    {
        // Only respond if the button is currently active
        if (button.gameObject.activeInHierarchy)
        {
            hoveredButton = button;
            SetSelectedButton(button);
        }
    }
    
    void OnMouseExit(Button button)
    {
        // Clear hovered button when mouse leaves
        if (hoveredButton == button)
        {
            hoveredButton = null;
        }
    }
    
    void UpdateButtonScaling()
    {
        if (backButton == null) return;
        
        Vector3 targetScale = originalBackButtonScale;
        
        // If this is the selected button (either by controller or mouse), increase its scale
        if (backButton == currentlySelected)
        {
            targetScale *= selectedScale;
        }
        
        // Smoothly interpolate to target scale
        backButton.transform.localScale = Vector3.Lerp(
            backButton.transform.localScale, 
            targetScale, 
            Time.deltaTime * scaleSpeed);
    }
    
    void SetSelectedButton(Button button)
    {
        if (button != null)
        {
            currentlySelected = button;
            button.Select();
        }
    }
    
    public void UpdateScoreboard()
    {
        // Clear existing entries
        foreach (Transform child in scoreEntryContainer)
        {
            if (child.gameObject != noScoresText.gameObject)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (ScoreboardManager.Instance == null)
        {
            Debug.LogError("ScoreboardManager instance not found!");
            noScoresText.gameObject.SetActive(true);
            return;
        }
        
        List<HighScoreEntry> highScores = ScoreboardManager.Instance.GetHighScores();
        
        if (highScores.Count == 0)
        {
            noScoresText.gameObject.SetActive(true);
            return;
        }
        
        noScoresText.gameObject.SetActive(false);
        
        // Create score entries
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreEntryContainer);
            ScoreEntryDisplay entryDisplay = entryObject.GetComponent<ScoreEntryDisplay>();
            
            if (entryDisplay != null)
            {
                // Rank is i+1 (1-based)
                entryDisplay.SetScoreEntry(i + 1, highScores[i]);
                
                // Apply special colors for top 3
                if (i == 0)
                    entryDisplay.SetTextColor(firstPlaceColor);
                else if (i == 1)
                    entryDisplay.SetTextColor(secondPlaceColor);
                else if (i == 2)
                    entryDisplay.SetTextColor(thirdPlaceColor);
            }
        }
    }
}