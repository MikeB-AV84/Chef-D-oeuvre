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

    [Header("Controller Settings")]
    public float selectedScale = 1.2f;
    public float scaleSpeed = 5f;

    private bool deathScreenActive = false;
    private int currentScore = 0;
    private bool isHighScore = false;
    private GameObject hoveredElement; 
    
    [Header("References")]
    public GameManager gameManager;

    private float inputCooldownTimer = 0f;
    private const float INPUT_COOLDOWN_DURATION = 1.5f; 
    private bool hasSubmittedName = false;

    private Vector3 originalRestartButtonScale;
    private Vector3 originalSubmitButtonScale;
    private Vector3 originalInputFieldScale;

    private GameObject currentSelection;
    private bool joystickMoved = false;

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

        deathScreen.SetActive(false);
        nameInputPanel.SetActive(false);
        hasSubmittedName = false;
        
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        submitNameButton.onClick.AddListener(SubmitName);
        // The restartButton listener is added dynamically after name submission
        
        if(restartButton != null) originalRestartButtonScale = restartButton.transform.localScale;
        if(submitNameButton != null) originalSubmitButtonScale = submitNameButton.transform.localScale;
        if(nameInputField != null) originalInputFieldScale = nameInputField.transform.localScale;
        
        AddMouseHoverEvents();
    }

    void AddMouseHoverEvents()
    {
        if(restartButton != null) AddHoverEvent(restartButton.gameObject, true);
        if(submitNameButton != null) AddHoverEvent(submitNameButton.gameObject, true);
        if(nameInputField != null) AddHoverEvent(nameInputField.gameObject, false);
    }
    
    void AddHoverEvent(GameObject element, bool isButton)
    {
        EventTrigger eventTrigger = element.GetComponent<EventTrigger>() ?? element.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enterEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEvent.callback.AddListener((eventData) => OnMouseEnter(element, isButton));
        eventTrigger.triggers.Add(enterEvent);
        
        EventTrigger.Entry exitEvent = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEvent.callback.AddListener((eventData) => OnMouseExit(element));
        eventTrigger.triggers.Add(exitEvent);
    }
    
    void OnMouseEnter(GameObject element, bool isButton)
    {
        if (element.activeInHierarchy)
        {
            hoveredElement = element;
            SetSelectedElement(element);
        }
    }
    
    void OnMouseExit(GameObject element)
    {
        if (hoveredElement == element)
        {
            hoveredElement = null;
        }
    }

    void Update()
    {
        if (inputCooldownTimer > 0)
        {
            inputCooldownTimer -= Time.unscaledDeltaTime;
        }

        UpdateElementScaling();

        if (nameInputPanel.activeSelf)
        {
            HandleNameInputNavigation();
        }
        else if (deathScreenActive && deathScreen.activeSelf && inputCooldownTimer <= 0)
        {
            // Restart logic for the final death screen (after name input)
            if (currentSelection == restartButton.gameObject && 
                Input.GetKeyDown(KeyCode.JoystickButton2)) // Common confirm buttons
            {
                RestartGame();
            }
        }
    }
    
    void UpdateElementScaling()
    {
        ScaleElement(restartButton, originalRestartButtonScale);
        ScaleElement(submitNameButton, originalSubmitButtonScale);
        ScaleElement(nameInputField.gameObject, originalInputFieldScale); // TMP_InputField is a component, scale its GameObject
    }

    void ScaleElement(Button button, Vector3 originalScale)
    {
        if (button == null) return;
        ScaleElement(button.gameObject, originalScale);
    }

    void ScaleElement(GameObject element, Vector3 originalScale)
    {
        if (element == null) return;
        Vector3 targetScale = originalScale;
        if (element == currentSelection)
        {
            targetScale *= selectedScale;
        }
        element.transform.localScale = Vector3.Lerp(
            element.transform.localScale, 
            targetScale, 
            Time.unscaledDeltaTime * scaleSpeed);
    }
    
    void HandleNameInputNavigation()
    {
        if (inputCooldownTimer > 0 ) return;

        float verticalInput = Input.GetAxisRaw("Vertical");
        bool joystickUp = verticalInput > 0.5f;
        bool joystickDown = verticalInput < -0.5f;
        
        if ((joystickUp || Input.GetKeyDown(KeyCode.UpArrow)) && !joystickMoved)
        {
            hoveredElement = null; 
            if (currentSelection == submitNameButton.gameObject) SetSelectedElement(nameInputField.gameObject);
            joystickMoved = true;
        }
        else if ((joystickDown || Input.GetKeyDown(KeyCode.DownArrow)) && !joystickMoved)
        {
            hoveredElement = null; 
            if (currentSelection == nameInputField.gameObject) SetSelectedElement(submitNameButton.gameObject);
            joystickMoved = true;
        }
        else if (Mathf.Abs(verticalInput) < 0.2f)
        {
            joystickMoved = false;
        }
        
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if (currentSelection == submitNameButton.gameObject || currentSelection == nameInputField.gameObject)
            {
                SubmitName();
            }
        }
    }

    void SetSelectedElement(GameObject element)
    {
        if (element == null) return;

        currentSelection = element;
        EventSystem.current.SetSelectedGameObject(element); // This is important for controller/keyboard focus

        if (element == nameInputField.gameObject)
        {
            nameInputField.ActivateInputField();
        }
    }

    public void ShowDeathScreen(int finalScore)
    {
        Time.timeScale = 0f; // Pause the game
        currentScore = finalScore;
        if(scoreText != null) scoreText.text = string.Format(scoreFormat, finalScore);
        
        isHighScore = ScoreboardManager.Instance != null && 
                      ScoreboardManager.Instance.IsHighScore(finalScore);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false; // Disable player actions
        }
        
        // Inform GameManager that player is dead.
        // GameManager's SetPlayerDead might also pause audio, but we want a definitive STOP.
        if (gameManager != null)
        {
            gameManager.SetPlayerDead(true); 
        }

        // Explicitly stop ALL music (main playlist and boss music)
        if (AudioManager.Instance != null)
        {
            Debug.Log("DeathScreenManager: Calling StopAllMusicImmediately on AudioManager.");
            AudioManager.Instance.StopAllMusicImmediately();
        }
        else
        {
            Debug.LogWarning("DeathScreenManager: AudioManager.Instance is null. Cannot stop music.");
        }
        
        ShowNameInput();
    }
    
    private void ShowNameInput()
    {
        deathScreenActive = true; // This flag is used in Update for restart logic
        
        if(deathScreen != null) deathScreen.SetActive(false);
        if(nameInputPanel != null) nameInputPanel.SetActive(true);
        
        if(highScorePromptText != null) highScorePromptText.text = isHighScore ? defaultNamePrompt : regularScorePrompt;
        
        if(nameInputField != null)
        {
            nameInputField.text = "Player"; // Default or last used name could be loaded here
            SetSelectedElement(nameInputField.gameObject);
        }
        
        // Ensure restart button doesn't have old listeners if any
        if(restartButton != null) restartButton.onClick.RemoveAllListeners();
    }
    
    public void SubmitName()
    {
        if (hasSubmittedName || inputCooldownTimer > 0) return; // Prevent double/quick submission
        hasSubmittedName = true;
        inputCooldownTimer = INPUT_COOLDOWN_DURATION; // Cooldown after submission
        
        string playerName = nameInputField.text;
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "Player";
        
        if (ScoreboardManager.Instance != null)
        {
            ScoreboardManager.Instance.AddHighScore(playerName, currentScore);
        }
        
        Input.ResetInputAxes(); // Clear any buffered input
        
        if(nameInputPanel != null) nameInputPanel.SetActive(false);
        if(deathScreen != null) deathScreen.SetActive(true);
        
        // Now that the final death screen is up, assign the restart listener
        if(restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            StartCoroutine(SelectRestartButtonNextFrame());
        }
    }
    
    private System.Collections.IEnumerator SelectRestartButtonNextFrame()
    {
        yield return null; // Wait one frame
        if(restartButton != null) SetSelectedElement(restartButton.gameObject);
    }

    public void RestartGame()
    {
        if (inputCooldownTimer > 0 && !hasSubmittedName) // Allow restart if name was submitted, even during its cooldown
        {
             // If name hasn't been submitted, and we are in cooldown, likely an accidental press.
             // However, if name *was* submitted, the cooldown is to prevent *re-submission*, not restart.
             // This logic might need refinement based on exact desired flow.
             // For now, if name is submitted, allow restart.
        }


        Time.timeScale = 1f;
        deathScreenActive = false;
        hasSubmittedName = false; 
        
        // No need to manually re-enable playerController or reset GameManager.SetPlayerDead(false)
        // as SceneManager.LoadScene will reset the scene state.

        // It's good practice to ensure AudioManager might restart its music on scene load
        // The AudioManager's OnSceneLoaded should handle this.
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
