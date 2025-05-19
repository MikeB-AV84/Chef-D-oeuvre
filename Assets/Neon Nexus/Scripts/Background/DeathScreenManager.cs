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

    [Header("Name Input")]
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button submitNameButton;
    public TextMeshProUGUI highScorePromptText;
    public string defaultNamePrompt = "NEW HIGH SCORE! ENTER YOUR NAME:";
    public string regularScorePrompt = "GAME OVER! ENTER YOUR NAME:";

    private bool deathScreenActive = false;
    private int currentScore = 0;
    private bool isHighScore = false;
    
    [Header("References")]
    public GameManager gameManager;  // Reference to the GameManager

    // Add this to the class fields at the top
    // Add a cooldown timer to prevent input bleed-through
    private float inputCooldownTimer = 0f;
    private const float INPUT_COOLDOWN_DURATION = 1.5f; // Half-second cooldown
    private bool hasSubmittedName = false;

    void Awake()
    {
        Instance = this;
        deathScreen.SetActive(false);
        nameInputPanel.SetActive(false);
        hasSubmittedName = false;
        
        // If gameManager is not assigned, try to find it
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        // Setup button listeners
        submitNameButton.onClick.AddListener(SubmitName);
    }

    private GameObject currentSelection;
    private bool joystickMoved = false;

    void Update()
    {
        // Update cooldown timer
        if (inputCooldownTimer > 0)
        {
            inputCooldownTimer -= Time.unscaledDeltaTime;
        }

        // Handle navigation in the name input panel
        if (nameInputPanel.activeSelf)
        {
            HandleNameInputNavigation();
        }
        // Handle navigation in the death screen
        else if (deathScreenActive && deathScreen.activeSelf)
        {
            // Only process input after cooldown and if we have something selected
            if (inputCooldownTimer <= 0)
            {
                // Check for specific controller button (X button) first
                if (Input.GetKeyDown(KeyCode.JoystickButton2))
                {
                    RestartGame();
                    return;
                }
                
                // Check for keyboard or mouse input
                if (Input.GetKeyDown(KeyCode.Space) || 
                    (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == restartButton.gameObject))
                {
                    RestartGame();
                }
            }
            
            // Prevent Submit button from triggering restart - critical for controllers
            if (Input.GetButtonDown("Submit") && inputCooldownTimer <= 0)
            {
                // Consume this input without doing anything
                inputCooldownTimer = 0.1f; // Small cooldown
            }
        }
    }
    
    void HandleNameInputNavigation()
    {
        // Skip input handling during cooldown
        if (inputCooldownTimer > 0 )
        {
            return;
        }

        // Get vertical input for navigation
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // Only register joystick movement when it crosses thresholds
        bool joystickUp = verticalInput > 0.5f;
        bool joystickDown = verticalInput < -0.5f;
        
        // Handle joystick/d-pad navigation
        if ((joystickUp || Input.GetKeyDown(KeyCode.UpArrow)) && !joystickMoved)
        {
            // Move selection up
            if (currentSelection == submitNameButton.gameObject)
            {
                SetInputFieldSelected();
            }
            joystickMoved = true;
        }
        else if ((joystickDown || Input.GetKeyDown(KeyCode.DownArrow)) && !joystickMoved)
        {
            // Move selection down
            if (currentSelection == nameInputField.gameObject)
            {
                SetSubmitButtonSelected();
            }
            joystickMoved = true;
        }
        else if (Mathf.Abs(verticalInput) < 0.2f)
        {
            // Reset joystick movement when returned to neutral position
            joystickMoved = false;
        }
        
        // For Submit/A button, check specifically for the button down event
        if (nameInputPanel.activeSelf && 
            (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.JoystickButton0))) // JoystickButton0 is typically the A button
        {
            // Store currently selected item and process accordingly
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            
            if (selectedObject == submitNameButton.gameObject || selectedObject == nameInputField.gameObject)
            {
                // Clear any cached inputs before transition
                Input.ResetInputAxes();
                SubmitName();
                
                // Apply a longer cooldown after submission to prevent input bleed-through
                inputCooldownTimer = INPUT_COOLDOWN_DURATION;
            }
        }
        
        // Submit name with Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Input.ResetInputAxes();
            SubmitName();
        }
    }

    public void ShowDeathScreen(int finalScore)
    {
        Time.timeScale = 0f;
        currentScore = finalScore;
        scoreText.text = string.Format(scoreFormat, finalScore);
        
        // Check if this is a high score
        isHighScore = ScoreboardManager.Instance != null && 
                      ScoreboardManager.Instance.IsHighScore(finalScore);
        
        // Enable cursors for mouse users
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable player controller input
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerDead(true);
        }
        
        // Inform GameManager that player is dead (will prevent pause menu access)
        if (gameManager != null)
        {
            gameManager.SetPlayerDead(true);
        }
        else
        {
            // If GameManager not found for some reason, directly stop the music
            AudioManager.Instance?.StopMusic();
        }
        
        // Show name input
        ShowNameInput();
    }
    
    private void ShowNameInput()
    {
        deathScreenActive = true;
        
        // Show the appropriate UI
        deathScreen.SetActive(false);
        nameInputPanel.SetActive(true);
        
        // Set the prompt text based on whether it's a high score
        highScorePromptText.text = isHighScore ? defaultNamePrompt : regularScorePrompt;
        
        // Focus the input field
        nameInputField.text = "Player";
        SetInputFieldSelected();
        
        // IMPORTANT: Disable button's onClick to prevent it from being triggered by Submit/A button
        restartButton.onClick.RemoveAllListeners();
    }
    
    private void SetInputFieldSelected()
    {
        // Set the input field as the current selection
        currentSelection = nameInputField.gameObject;
        nameInputField.Select();
        nameInputField.ActivateInputField();
        
        // Visual feedback
        nameInputField.transform.localScale = Vector3.one * 1.1f;
        submitNameButton.transform.localScale = Vector3.one;
    }
    
    private void SetSubmitButtonSelected()
    {
        // Set the submit button as the current selection
        currentSelection = submitNameButton.gameObject;
        submitNameButton.Select();
        
        // Visual feedback
        submitNameButton.transform.localScale = Vector3.one * 1.1f;
        nameInputField.transform.localScale = Vector3.one;
    }
    
    public void SubmitName()
    {
        // Prevent double submission
        if (hasSubmittedName) return;
        hasSubmittedName = true;
        
        string playerName = nameInputField.text;
        
        // Make sure name isn't empty
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }
        
        // Save the score to the scoreboard
        if (ScoreboardManager.Instance != null)
        {
            ScoreboardManager.Instance.AddHighScore(playerName, currentScore);
        }
        
        // Cancel any pending controller/input events
        Input.ResetInputAxes();
        
        // Show death screen after submitting name
        nameInputPanel.SetActive(false);
        deathScreen.SetActive(true);
        
        // Clear any existing input from the event system
        EventSystem.current.SetSelectedGameObject(null);
        
        // Wait until next frame to set the button as selected
        StartCoroutine(SelectRestartButtonNextFrame());

        // Start the input cooldown to prevent immediate restart
        inputCooldownTimer = INPUT_COOLDOWN_DURATION;
    }
    
    private System.Collections.IEnumerator SelectRestartButtonNextFrame()
    {
        // Wait for 2 frames to ensure all input processing has completed
        yield return null;
        yield return null;
        
        // Now it's safe to select the restart button
        currentSelection = restartButton.gameObject;
        EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
    }

    public void RestartGame()
    {
        // Prevent input spamming
        if (inputCooldownTimer > 0) return;
        
        Time.timeScale = 1f;
        deathScreenActive = false;
        hasSubmittedName = false; // Reset submission flag
        
        // Reset player state (though we're reloading the scene, this is for safety)
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetPlayerDead(false);
        }
        
        // Reset game manager state (though we're reloading the scene, this is for safety)
        if (gameManager != null)
        {
            gameManager.SetPlayerDead(false);
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}