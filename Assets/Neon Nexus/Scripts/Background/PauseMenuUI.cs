using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button resumeButton;
    public Button mainMenuButton;
    
    [Header("Controller Settings")]
    public float selectedScale = 1.2f;
    public float scaleSpeed = 5f;
    
    [Header("References")]
    public GameManager gameManager;
    
    private Button[] menuButtons;
    private Vector3[] originalScales;
    private Button currentlySelected;
    private Button hoveredButton; // Track which button is being hovered
    private float lastVerticalInput = 0f;
    private float inputDelay = 0.2f;
    private float lastInputTime = 0f;

    void OnEnable()
    {
        // Check if player is dead before proceeding
        if (gameManager != null && gameManager.IsPlayerDead())
        {
            // Player is dead, don't show pause menu
            gameObject.SetActive(false);
            return;
        }
        
        // Set initial selected button when menu opens
        SetSelectedButton(resumeButton);
    }

    void Start()
    {
        // Initialize button arrays
        menuButtons = new Button[] { resumeButton, mainMenuButton };
        originalScales = new Vector3[menuButtons.Length];
        
        // Store original scales and add listeners
        for (int i = 0; i < menuButtons.Length; i++)
        {
            originalScales[i] = menuButtons[i].transform.localScale;
        }
        
        // Button listeners
        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        // Add mouse hover event triggers to all buttons
        AddMouseHoverEvents();
        
        // If game manager not assigned, try to find it
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void AddMouseHoverEvents()
    {
        // Add hover events for each button
        AddHoverEvent(resumeButton);
        AddHoverEvent(mainMenuButton);
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
        // First check if player is dead
        if (gameManager != null && gameManager.IsPlayerDead())
        {
            // Player is dead, hide pause menu
            gameObject.SetActive(false);
            return;
        }
        
        // Handle controller input for navigation (when paused)
        HandleControllerInput();
        
        // Update button scaling
        UpdateButtonScaling();
    }
    
    void HandleControllerInput()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // Detect changes in input with delay to prevent rapid switching
        if ((Time.unscaledTime - lastInputTime) > inputDelay)
        {
            // Vertical navigation (Up/Down)
            if (verticalInput > 0.5f && lastVerticalInput <= 0.5f)
            {
                hoveredButton = null; // Clear mouse hover when using controller
                // Move selection up
                if (currentlySelected == mainMenuButton)
                {
                    SetSelectedButton(resumeButton);
                    lastInputTime = Time.unscaledTime;
                }
            }
            else if (verticalInput < -0.5f && lastVerticalInput >= -0.5f)
            {
                hoveredButton = null; // Clear mouse hover when using controller
                // Move selection down
                if (currentlySelected == resumeButton)
                {
                    SetSelectedButton(mainMenuButton);
                    lastInputTime = Time.unscaledTime;
                }
            }
        }
        
        // Store last input for comparison next frame
        lastVerticalInput = verticalInput;
        
        // Controller button press (A button is typically mapped to "Submit")
        if (Input.GetButtonDown("Submit"))
        {
            if (currentlySelected != null)
            {
                currentlySelected.onClick.Invoke();
            }
        }
        
        // Keyboard shortcuts for menu navigation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            hoveredButton = null; // Clear mouse hover when using keyboard
            if (currentlySelected == mainMenuButton)
                SetSelectedButton(resumeButton);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            hoveredButton = null; // Clear mouse hover when using keyboard
            if (currentlySelected == resumeButton)
                SetSelectedButton(mainMenuButton);
        }
        
        // Enter or Space to select
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentlySelected != null)
                currentlySelected.onClick.Invoke();
        }
    }
    
    void UpdateButtonScaling()
    {
        // Update scale of all buttons
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                Vector3 targetScale = originalScales[i];
                
                // If this is the selected button (either by controller or mouse), increase its scale
                if (menuButtons[i] == currentlySelected)
                {
                    targetScale *= selectedScale;
                }
                
                // Smoothly interpolate to target scale (using unscaledDeltaTime for when game is paused)
                menuButtons[i].transform.localScale = Vector3.Lerp(
                    menuButtons[i].transform.localScale, 
                    targetScale, 
                    Time.unscaledDeltaTime * scaleSpeed);
            }
        }
    }
    
    void SetSelectedButton(Button button)
    {
        if (button != null)
        {
            currentlySelected = button;
            button.Select();
            
            // Play UI selection sound here if you have one
            // AudioManager.Instance.PlayUISelectSound();
        }
    }

    void Resume()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    void ReturnToMainMenu()
    {
        if (gameManager != null)
        {
            gameManager.ReturnToMenu();
        }
    }
}