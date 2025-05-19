using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

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
    private string saveFileName = "highscores.json";

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

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public void LoadHighScores()
    {
        string filePath = GetSavePath();
        
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            scoreData = JsonUtility.FromJson<HighScoreData>(json);
            
            // Ensure the list is properly sorted after loading
            scoreData.highScores = scoreData.highScores
                .OrderByDescending(x => x.score)
                .ToList();
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
        string filePath = GetSavePath();
        string json = JsonUtility.ToJson(scoreData);
        File.WriteAllText(filePath, json);
    }

    public bool AddHighScore(string playerName, int score)
    {
        // Check if score qualifies for the high score list
        if (!IsHighScore(score))
            return false;

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
        
        // Save to JSON file
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

    // Helper method to clear high scores (for testing)
    public void ClearHighScores()
    {
        scoreData.highScores.Clear();
        SaveHighScores();
    }

    // Helper method to get the save file path (for debugging)
    public string GetHighScoreFilePath()
    {
        return GetSavePath();
    }
}