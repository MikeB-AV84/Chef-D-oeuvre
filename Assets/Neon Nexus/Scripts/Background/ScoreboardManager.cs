using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;

    public HighScoreEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}

[System.Serializable]
public class HighScoreData
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
}

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance;

    [Header("Scoreboard Settings")]
    public int maxHighScores = 5;

    private HighScoreData scoreData;
    private string saveKey = "HighScores";

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadHighScores()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            scoreData = JsonUtility.FromJson<HighScoreData>(json);
        }
        else
        {
            scoreData = new HighScoreData();
            // Optional: Add default scores for testing
            // AddHighScore("Player1", 1000);
            // AddHighScore("Player2", 800);
        }
    }

    public void SaveHighScores()
    {
        string json = JsonUtility.ToJson(scoreData);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public bool AddHighScore(string playerName, int score)
    {
        // Create new entry
        HighScoreEntry newEntry = new HighScoreEntry(playerName, score);
        
        // Add to list
        scoreData.highScores.Add(newEntry);
        
        // Sort by score (descending)
        scoreData.highScores = scoreData.highScores
            .OrderByDescending(x => x.score)
            .ToList();
        
        // Trim to max count
        if (scoreData.highScores.Count > maxHighScores)
        {
            scoreData.highScores.RemoveRange(maxHighScores, scoreData.highScores.Count - maxHighScores);
        }
        
        // Save to PlayerPrefs
        SaveHighScores();
        
        // Return true if score made it to the high score list
        return scoreData.highScores.Contains(newEntry);
    }

    public List<HighScoreEntry> GetHighScores()
    {
        return scoreData.highScores;
    }

    public bool IsHighScore(int score)
    {
        // If we have fewer than maxHighScores, any score can be added
        if (scoreData.highScores.Count < maxHighScores)
            return true;
            
        // Otherwise, check if the score is higher than the lowest score
        return score > scoreData.highScores.Min(x => x.score);
    }
}
