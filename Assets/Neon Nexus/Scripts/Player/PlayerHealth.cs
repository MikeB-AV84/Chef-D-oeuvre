using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int playerLives = 5;
    public int maxLives = 5;
    public TextMeshProUGUI livesText;
    private HyperDash hyperDash;

    void Start()
    {
        hyperDash = GetComponent<HyperDash>();
        UpdateLivesText();
    }

    public bool CanGainLife()
    {
        return playerLives < maxLives;
    }

    public void TakeDamage(int damage)
    {
        if (hyperDash != null && hyperDash.isDashing) return;
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