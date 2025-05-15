using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    [Header("Controller Input")]
    public string menuButton = "Menu_Button"; // Xbox menu button
    
    [Header("Pause Menu")]
    public GameObject pauseMenuUI; // Assign your pause menu UI panel in the inspector
    
    private bool isPaused = false;
    private PlayerController playerController;
    private AudioManager audioManager;
    
    void Start()
    {
        // Make sure the pause menu is hidden at start
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Find references to important components
        playerController = FindObjectOfType<PlayerController>();
        audioManager = AudioManager.Instance;
    }
    
    void Update()
    {
        // Toggle pause menu with escape key or menu button
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(menuButton))
        {
            TogglePauseMenu();
        }
    }
    
    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        
        // Set time scale to 0 to pause the game
        Time.timeScale = 0f;
        isPaused = true;
        
        // Pause the music using AudioManager
        if (audioManager != null)
        {
            audioManager.PauseMusic();
        }
        
        // Disable player controls by setting player as "dead"
        // This prevents movement and shooting
        if (playerController != null)
        {
            playerController.SetPlayerDead(true);
        }
    }
    
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Set time scale back to 1 to resume normal time
        Time.timeScale = 1f;
        isPaused = false;
        
        // Resume music using AudioManager
        if (audioManager != null)
        {
            audioManager.ResumeMusic();
        }
        
        // Re-enable player controls
        if (playerController != null)
        {
            playerController.SetPlayerDead(false);
        }
    }

    public void ReturnToMenu()
    {
        // Make sure to reset timeScale before loading a new scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}