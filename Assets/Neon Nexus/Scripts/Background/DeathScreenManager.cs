using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
        if (deathScreenActive)
        {
            // Only allow X button (JoystickButton2), Space, or Mouse click
            if (Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.JoystickButton2) || // X button only
                Input.GetMouseButtonDown(0))
            {
                RestartGame();
            }
        }
    }

    public void ShowDeathScreen(int finalScore)
    {
        Time.timeScale = 0f;
        scoreText.text = string.Format(scoreFormat, finalScore);
        deathScreen.SetActive(true);
        deathScreenActive = true;
        
        // IMPORTANT: Disable button's onClick to prevent it from being triggered by Submit/A button
        restartButton.onClick.RemoveAllListeners();
        
        // Completely disable automatic button navigation
        EventSystem.current.SetSelectedGameObject(null);
        
        // Visual feedback without actual selection
        restartButton.transform.localScale = restartButton.transform.localScale * 1.1f;
        
        // For mouse users
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable player controller input
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerDead(true);
        }

        AudioManager.Instance?.StopMusic();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        deathScreenActive = false;
        
        // Reset player state (though we're reloading the scene, this is for safety)
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerDead(false);
        }
        
        AudioManager.Instance?.PlayRandomMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}