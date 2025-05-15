using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    private bool isPaused = false;
    private bool isPlayerDead = false;  // Add this flag to track player death state

    [Header("UI References")]
    public GameObject pauseMenuUI;
    
    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";  // Add this for scene loading

    // References to other managers
    private PlayerController playerController;
    private AudioManager audioManager;

    void Awake()
    {
        Instance = this;

        // Make sure pause menu is initially hidden
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Find player controller reference
        playerController = FindObjectOfType<PlayerController>();

        // Find audio manager reference
        audioManager = FindObjectOfType<AudioManager>();
    }

    void Update()
    {
        // Check for pause input only if player is not dead
        if (!isPlayerDead && (Input.GetButtonDown("Menu_Button") || Input.GetKeyDown(KeyCode.Escape)))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        // Don't allow pausing if player is dead
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
        // Don't allow pausing if player is dead
        if (isPlayerDead) return;

        Time.timeScale = 0f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        // Disable player movement and shooting using SetPlayerDead
        if (playerController != null)
        {
            playerController.SetPlayerDead(true);
        }

        // Pause music
        if (audioManager != null)
        {
            audioManager.PauseMusic();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Re-enable player controller only if player is not dead
        if (!isPlayerDead && playerController != null)
        {
            playerController.SetPlayerDead(false);
        }

        // Resume music if player is not dead
        if (!isPlayerDead && audioManager != null)
        {
            audioManager.ResumeMusic();
        }
    }

    // Add this method to set the player's dead state
    public void SetPlayerDead(bool isDead)
    {
        isPlayerDead = isDead;
        
        // If player is dead, make sure we handle the game state properly
        if (isDead)
        {
            // If we're paused, make sure to hide pause menu
            if (isPaused)
            {
                isPaused = false;
                if (pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                }
            }
            
            // Stop/pause music if needed
            if (audioManager != null)
            {
                audioManager.PauseMusic();
            }
        }
    }
    
    // Add this method to check if player is dead - needed by PauseMenuUI
    public bool IsPlayerDead()
    {
        return isPlayerDead;
    }
    
    // Add this method to return to the main menu - needed by PauseMenuUI
    public void ReturnToMenu()
    {
        // Make sure time scale is back to normal
        Time.timeScale = 1f;
        
        // Reset game state
        isPaused = false;
        isPlayerDead = false;
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}