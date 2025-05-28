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

    private PlayerController playerController;
    // No direct AudioManager reference needed here if AudioManager is a Singleton an accessible via Instance

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
        // audioManager = FindObjectOfType<AudioManager>(); // Not strictly needed if using Singleton
    }

    void Update()
    {
        if (!isPlayerDead && (Input.GetButtonDown("Menu_Button") || Input.GetKeyDown(KeyCode.Escape)))
        {
            TogglePause();
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
        if (isPlayerDead) return; // Should already be handled by TogglePause check but good to be safe
        isPaused = true; // Ensure state is set
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        if (playerController != null)
        {
            // playerController.SetPlayerDead(true); // This seems too aggressive for a simple pause
                                                 // If SetPlayerDead also disables input, it's okay.
                                                 // Or use a separate playerController.SetInputEnabled(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseGameAudio(); // Use new method
        }
    }

    public void ResumeGame()
    {
        // isPaused = false; // Set by TogglePause before calling this
        Time.timeScale = 1f;
        // isPaused is already false here if called from TogglePause

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (!isPlayerDead && playerController != null)
        {
            // playerController.SetPlayerDead(false); // See comment in PauseGame
            // Or use playerController.SetInputEnabled(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeGameAudio(); // Use new method
        }
    }

    public void SetPlayerDead(bool isDead)
    {
        isPlayerDead = isDead;
        
        if (isDead)
        {
            if (isPaused) // If player dies while paused
            {
                // isPaused = false; // No, keep game paused but hide menu if that's the design
                Time.timeScale = 0f; // Ensure time remains paused
                if (pauseMenuUI != null)
                {
                    // pauseMenuUI.SetActive(false); // Or keep it, depends on design
                }
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PauseGameAudio(); // Player death should pause ongoing sounds
            }
        }
    }
    
    public bool IsPlayerDead()
    {
        return isPlayerDead;
    }
    
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isPlayerDead = false;

        // Explicitly stop boss music and resume main track if returning to menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBossMusic();
            AudioManager.Instance.ResumeMainTrackAfterBoss(); // Try to reset main music
            // Potentially a more forceful stop of all game music might be needed here
            // depending on how main menu handles its own music.
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}