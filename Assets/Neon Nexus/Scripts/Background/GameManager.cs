using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    private bool isPaused = false;
    private bool isPlayerDead = false;

    [Header("UI References")]
    public GameObject pauseMenuUI;
    
    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Input Settings")]
    public string pauseButton = "Menu_Button"; // Default Unity Input Manager name for Select/Start button

    private PlayerController playerController;

    void Awake()
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

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        // Only check for pause input if player is not dead
        if (!isPlayerDead)
        {
            // Check for pause button (controller) or Escape key
            if (Input.GetButtonDown(pauseButton) || Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (isPlayerDead) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        if (isPlayerDead) return;
        
        Debug.Log("GameManager: Pausing game");
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        // Disable player input/movement
        if (playerController != null)
        {
            playerController.enabled = false; // This will disable all player input
        }

        // Pause all audio through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseGameAudio();
        }

        // Show cursor for menu navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        Debug.Log("GameManager: Resuming game");
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Re-enable player input/movement
        if (!isPlayerDead && playerController != null)
        {
            playerController.enabled = true;
        }

        // Resume all audio through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeGameAudio();
        }

        // Hide cursor and lock it for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetPlayerDead(bool isDead)
    {
        Debug.Log($"GameManager: SetPlayerDead called with {isDead}");
        isPlayerDead = isDead;
        
        if (isDead)
        {
            // If player dies while game is paused, close pause menu
            if (isPaused)
            {
                isPaused = false; // Reset pause state
                if (pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                }
            }
            
            // Time.timeScale will be set to 0 by DeathScreenManager
            // Audio will be stopped by DeathScreenManager calling StopAllMusicImmediately
            
            // Disable player controller
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
        else
        {
            // Player respawned/revived - this might not be used in your current setup
            // but good to have for completeness
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
    
    public bool IsPlayerDead()
    {
        return isPlayerDead;
    }
    
    public bool IsGamePaused()
    {
        return isPaused;
    }
    
    public void ReturnToMenu()
    {
        Debug.Log("GameManager: Returning to main menu");
        Time.timeScale = 1f;
        isPaused = false;
        isPlayerDead = false;

        // Stop all music and reset audio states
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusicImmediately();
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}