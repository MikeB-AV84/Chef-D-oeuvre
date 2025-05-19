using System.Collections.Generic;
using UnityEngine;
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
    
    void OnEnable()
    {
        UpdateScoreboard();
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