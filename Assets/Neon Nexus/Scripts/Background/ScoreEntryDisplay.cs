using UnityEngine;
using TMPro;

public class ScoreEntryDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    
    [Header("Format")]
    public string rankFormat = "{0}.";
    public string scoreFormat = "{0}";
    
    public void SetScoreEntry(int rank, HighScoreEntry entry)
    {
        rankText.text = string.Format(rankFormat, rank);
        nameText.text = entry.playerName;
        scoreText.text = string.Format(scoreFormat, entry.score);
    }
    
    public void SetTextColor(Color color)
    {
        rankText.color = color;
        nameText.color = color;
        scoreText.color = color;
    }
}