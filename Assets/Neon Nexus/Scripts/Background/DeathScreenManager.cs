using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;
    
    [Header("UI References")]
    public GameObject deathScreen;
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public string scoreFormat = "SCORE: {0}";

    private bool deathScreenActive = false;

    void Awake()
    {
        Instance = this;
        deathScreen.SetActive(false);
    }

    void Update()
    {
        if (deathScreenActive && (
            Input.GetKeyDown(KeyCode.Space) || 
            Input.GetKeyDown(KeyCode.JoystickButton0) || // Gamepad A button
            Input.GetMouseButtonDown(0))) // Left click
        {
            RestartGame();
        }
    }

    public void ShowDeathScreen(int finalScore)
    {
        Time.timeScale = 0f;
        scoreText.text = string.Format(scoreFormat, finalScore);
        deathScreen.SetActive(true);
        deathScreenActive = true;
        
        // Auto-select the button for gamepad navigation
        restartButton.Select();
        restartButton.OnSelect(null); // Visual feedback
        
        // For mouse users, make sure navigation isn't stuck
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioManager.Instance?.StopMusic();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        deathScreenActive = false;
        AudioManager.Instance?.PlayRandomMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}