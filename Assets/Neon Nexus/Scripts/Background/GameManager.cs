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
    public string pauseButton = "Menu_Button";

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
    
    void Start()
    {
        // --- MODIFIED CODE START ---
        // Set the initial cursor state for gameplay.
        // For your control scheme, the cursor MUST be visible and unlocked.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // --- MODIFIED CODE END ---
    }

    void Update()
    {
        if (!isPlayerDead)
        {
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
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        
        // Make the cursor visible and unlock it for the pause menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // --- MODIFIED CODE START ---
        // When resuming, ensure the cursor remains visible and unlocked for gameplay.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // --- MODIFIED CODE END ---
    }
    
    public void SetPlayerDead(bool dead)
    {
        isPlayerDead = dead;

        if (isPlayerDead)
        {
            if(isPaused)
            {
                isPaused = false;
                if (pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                }
            }
            
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
        else
        {
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

        // Ensure cursor is visible for the main menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusicImmediately();
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
