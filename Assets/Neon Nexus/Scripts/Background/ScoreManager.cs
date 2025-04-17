using UnityEngine;
using TMPro; // Importation de TextMeshPro pour gérer l'affichage du texte

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Instance statique pour le pattern Singleton
    public int score; // Variable pour stocker le score du joueur
    public TextMeshProUGUI scoreText; // Référence au composant TextMeshProUGUI pour afficher le score

    void Awake()
    {
        // Implémentation du pattern Singleton
        if (Instance == null)
        {
            Instance = this; // Si aucune instance n'existe, définir cette instance comme l'instance active
        }
        else
        {
            Destroy(gameObject); // Si une instance existe déjà, détruire cet objet pour éviter les duplications
        }
    }

    void Start()
    {
        UpdateScoreText(); // Met à jour l'affichage du score dès le démarrage
    }

    public void AddScore(int points)
    {
        score += points; // Ajoute des points au score actuel
        UpdateScoreText(); // Met à jour l'affichage du score après l'ajout de points
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString(); // Met à jour le texte du score dans l'interface utilisateur
        }
    }

    public void Test()
    {
        Debug.Log("Test"); // Méthode de test pour vérifier si le script fonctionne correctement
    }
}