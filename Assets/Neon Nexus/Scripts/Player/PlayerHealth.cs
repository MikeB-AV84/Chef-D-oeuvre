using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int playerLives = 5;
    public int maxLives = 5;
    public TextMeshProUGUI livesText;

    void Start()
    {
        UpdateLivesText();
    }

    public bool CanGainLife()
    {
        return playerLives < maxLives;
    }

    public void TakeDamage(int damage)
    {
        playerLives = Mathf.Max(0, playerLives - damage);
        Debug.Log("Vies restantes : " + playerLives);
        UpdateLivesText();

        if (playerLives <= 0)
        {
            Debug.Log("Le joueur est mort !");
            ReloadScene();
        }
    }

    public void AddLife()
    {
        if (CanGainLife())
        {
            playerLives++;
            Debug.Log("Vie ajoutée ! Nouveau total : " + playerLives);
            UpdateLivesText();
        }
    }

    void UpdateLivesText()
    {
        if (livesText != null)
        {
            livesText.text = "Lives : " + playerLives + "/" + maxLives;
        }
    }

    void ReloadScene()
    {
    // Show death screen before reloading
    if (DeathScreenManager.Instance != null)
    {
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.score : 0;
        DeathScreenManager.Instance.ShowDeathScreen(finalScore);
    }
    else
    {
        // Fallback if no death screen
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    }
}