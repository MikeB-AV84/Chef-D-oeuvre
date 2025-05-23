using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button scoreboardButton;  // New scoreboard button
    public Button quitButton;
    
    [Header("Controller Settings")]
    public float selectedScale = 1.2f;
    public float scaleSpeed = 5f;
    
    [Header("Scoreboard")]
    public GameObject scoreboardPanel;
    public Button backButton;
    
    private Button[] menuButtons;
    private Vector3[] originalScales;
    private Button currentlySelected;
    private Button hoveredButton; // Track which button is being hovered
    private bool isScoreboardActive = false;

    private bool isUpPressed = false;
    private bool isDownPressed = false;
    private bool isTransitioning = false;
    private float transitionDelay = 0.3f;

    void Start()
    {
        // Initialize button arrays
        menuButtons = new Button[] { playButton, scoreboardButton, quitButton, backButton };
        originalScales = new Vector3[menuButtons.Length];
        
        // Store original scales
        for (int i = 0; i < menuButtons.Length; i++)
        {
            originalScales[i] = menuButtons[i].transform.localScale;
        }
        
        // Button listeners
        playButton.onClick.AddListener(StartGame);
        scoreboardButton.onClick.AddListener(ToggleScoreboard);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(ToggleScoreboard);
        
        // Add mouse hover event triggers to all buttons
        AddMouseHoverEvents();
        
        // Make sure scoreboard is initially hidden
        scoreboardPanel.SetActive(false);
        
        // Auto-select first button for gamepad
        SetSelectedButton(playButton);
    }

    void AddMouseHoverEvents()
    {
        // Add hover events for each button
        AddHoverEvent(playButton);
        AddHoverEvent(scoreboardButton);
        AddHoverEvent(quitButton);
        AddHoverEvent(backButton);
    }
    
    void AddHoverEvent(Button button)
    {
        // Get or add EventTrigger component
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Create mouse enter event
        EventTrigger.Entry enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((eventData) => OnMouseEnter(button));
        eventTrigger.triggers.Add(enterEvent);
        
        // Create mouse exit event
        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((eventData) => OnMouseExit(button));
        eventTrigger.triggers.Add(exitEvent);
    }
    
    void OnMouseEnter(Button button)
    {
        // Only respond if the button is currently active
        if (button.gameObject.activeInHierarchy)
        {
            hoveredButton = button;
            SetSelectedButton(button);
        }
    }
    
    void OnMouseExit(Button button)
    {
        // Clear hovered button when mouse leaves
        if (hoveredButton == button)
        {
            hoveredButton = null;
        }
    }

    void Update()
    {
        // Handle keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Space) && !isScoreboardActive) StartGame();
        if (Input.GetKeyDown(KeyCode.Escape) && isScoreboardActive) ToggleScoreboard();
        
        // Controller input for navigation
        HandleControllerInput();
        
        // Update button scaling
        UpdateButtonScaling();
    }
    
    private void HandleControllerInput()
    {
        if (isTransitioning) return;

        // Vertical navigation
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        if (verticalInput > 0.7f && !isUpPressed)
        {
            isUpPressed = true;
            hoveredButton = null; // Clear mouse hover when using controller
            if (!isScoreboardActive)
            {
                if (currentlySelected == scoreboardButton)
                    SetSelectedButton(playButton);
                else if (currentlySelected == quitButton)
                    SetSelectedButton(scoreboardButton);
            }
        }
        else if (verticalInput < -0.7f && !isDownPressed)
        {
            isDownPressed = true;
            hoveredButton = null; // Clear mouse hover when using controller
            if (!isScoreboardActive)
            {
                if (currentlySelected == playButton)
                    SetSelectedButton(scoreboardButton);
                else if (currentlySelected == scoreboardButton)
                    SetSelectedButton(quitButton);
            }
        }
        else if (Mathf.Abs(verticalInput) < 0.5f)
        {
            isUpPressed = false;
            isDownPressed = false;
        }
        
        // Submit handling
        if ((Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetButtonDown("Submit")) && currentlySelected != null)
        {
            currentlySelected.onClick.Invoke();
        }
        
        // Force back button selection when scoreboard is active
        if (isScoreboardActive && currentlySelected != backButton)
        {
            SetSelectedButton(backButton);
        }
    }

    void UpdateButtonScaling()
    {
        // Update scale of all buttons
        for (int i = 0; i < menuButtons.Length; i++)
        {
            // Skip if button is null
            if (menuButtons[i] == null) continue;
            
            Vector3 targetScale = originalScales[i];
            
            // If this is the selected button (either by controller or mouse), increase its scale
            if (menuButtons[i] == currentlySelected)
            {
                targetScale *= selectedScale;
            }
            
            // Smoothly interpolate to target scale
            menuButtons[i].transform.localScale = Vector3.Lerp(
                menuButtons[i].transform.localScale, 
                targetScale, 
                Time.deltaTime * scaleSpeed);
        }
    }
    
    void SetSelectedButton(Button button)
    {
        currentlySelected = button;
        button.Select();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    public void ToggleScoreboard()
    {
        if (isTransitioning) return;
        
        StartCoroutine(ToggleScoreboardRoutine());
    }

    private IEnumerator ToggleScoreboardRoutine()
    {
        isTransitioning = true;
        
        // Toggle the state
        isScoreboardActive = !isScoreboardActive;
        
        // Immediately update UI visibility
        scoreboardPanel.SetActive(isScoreboardActive);
        playButton.gameObject.SetActive(!isScoreboardActive);
        scoreboardButton.gameObject.SetActive(!isScoreboardActive);
        quitButton.gameObject.SetActive(!isScoreboardActive);
        
        // Clear any hover state when transitioning
        hoveredButton = null;
        
        // Force button selection
        if (isScoreboardActive)
        {
            SetSelectedButton(backButton);
            
            // Refresh scoreboard if needed
            ScoreboardUI scoreboardUI = GetComponentInChildren<ScoreboardUI>(true);
            if (scoreboardUI != null)
            {
                scoreboardUI.UpdateScoreboard();
            }
        }
        else
        {
            SetSelectedButton(scoreboardButton);
        }
        
        // Wait a brief moment before allowing another toggle
        yield return new WaitForSeconds(transitionDelay);
        isTransitioning = false;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}