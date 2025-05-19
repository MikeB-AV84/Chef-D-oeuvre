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

    void Awake()
    {
        Instance = this;
        deathScreen.SetActive(false);
        nameInputPanel.SetActive(false);
        
        // If gameManager is not assigned, try to find it
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        // Setup button listeners
        submitNameButton.onClick.AddListener(SubmitName);
        restartButton.onClick.AddListener(RestartGame);
    }

    private GameObject currentSelection;
    private bool joystickMoved = false;

    void Update()
{
    // Handle navigation in the name input panel
    if (nameInputPanel.activeSelf)
    {
        HandleNameInputNavigation();
    }
    // Handle navigation in the death screen
    else if (deathScreenActive && deathScreen.activeSelf)
    {
        // Only allow X button (JoystickButton2), Space, or Mouse click
        if (Input.GetKeyDown(KeyCode.Space) || 
            Input.GetKeyDown(KeyCode.JoystickButton2) || // X button only
            (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == restartButton.gameObject))
        {
            RestartGame();
        }
    }
}
    
    void HandleNameInputNavigation()
{
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
    
    // Handle A button (Submit) press - only in name input panel
    if (nameInputPanel.activeSelf && Input.GetButtonDown("Submit"))
    {
        if (currentSelection == submitNameButton.gameObject)
        {
            SubmitName();
        }
        else if (currentSelection == nameInputField.gameObject)
        {
            // When pressing submit on the input field, submit the name
            SubmitName();
        }
    }
    
    // Submit name with Enter key
    if (Input.GetKeyDown(KeyCode.Return))
    {
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
    
    // Show death screen after submitting name
    nameInputPanel.SetActive(false);
    deathScreen.SetActive(true);
    
    // Focus the restart button for controller navigation
    EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
    currentSelection = restartButton.gameObject;
    
    // Re-enable the restart button's onClick listener
    restartButton.onClick.AddListener(RestartGame);
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
        
        // Reset game manager state (though we're reloading the scene, this is for safety)
        if (gameManager != null)
        {
            gameManager.SetPlayerDead(false);
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}