using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button quitButton;
    
    [Header("Controller Settings")]
    public float selectedScale = 1.2f;
    public float scaleSpeed = 5f;
    
    private Button[] menuButtons;
    private Vector3[] originalScales;
    private Button currentlySelected;

    void Start()
    {
        // Initialize button arrays
        menuButtons = new Button[] { playButton, quitButton };
        originalScales = new Vector3[menuButtons.Length];
        
        // Store original scales and add listeners
        for (int i = 0; i < menuButtons.Length; i++)
        {
            originalScales[i] = menuButtons[i].transform.localScale;
        }
        
        // Button listeners
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
        
        // Auto-select first button for gamepad
        SetSelectedButton(playButton);
    }

    void Update()
    {
        // Handle keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Space)) StartGame();
        if (Input.GetKeyDown(KeyCode.Escape)) QuitGame();
        
        // Controller input for navigation
        HandleControllerInput();
        
        // Update button scaling
        UpdateButtonScaling();
    }
    
    void HandleControllerInput()
    {
        // Vertical navigation (Up/Down)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxisRaw("Vertical") > 0.5f)
        {
            // Move selection up
            if (currentlySelected == quitButton)
                SetSelectedButton(playButton);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < -0.5f)
        {
            // Move selection down
            if (currentlySelected == playButton)
                SetSelectedButton(quitButton);
        }
        
        // Controller button press (A button is typically mapped to "Submit")
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetButtonDown("Submit"))
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
            Vector3 targetScale = originalScales[i];
            
            // If this is the selected button, increase its scale
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

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}