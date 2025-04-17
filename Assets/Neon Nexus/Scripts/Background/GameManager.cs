using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    [Header("Controller Input")]
    public string menuButton = "Menu_Button"; // Xbox menu button
    void Update()
    {
        // Return to menu (optional)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(menuButton))
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}