using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab de l'ennemi à instancier
    public float spawnInterval = 3f; // Temps entre chaque spawn d'ennemi
    public int maxEnemiesOnScreen = 20; // Nombre maximum d'ennemis simultanés
    
    public Transform player; // Référence au joueur
    public float minSpawnDistance = 10f; // Distance min pour éviter le spawn trop proche
    public float maxSpawnDistance = 20f; // Distance max autour du joueur

    private float timer; // Timer pour suivre le temps écoulé entre chaque spawn

    void Start()
    {
        // Si le joueur n'est pas assigné manuellement, on le trouve par tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Aucun joueur trouvé avec le tag 'Player'");
            }
        }
        
        // Génère quelques ennemis initiaux
        for (int i = 0; i < 5; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (player == null) return;
        
        timer += Time.deltaTime;

        // Vérifie si le temps écoulé dépasse l'intervalle de spawn
        if (timer >= spawnInterval)
        {
            // Compte le nombre actuel d'ennemis
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // Ne spawne que si on n'a pas atteint le nombre max d'ennemis
            if (existingEnemies.Length < maxEnemiesOnScreen)
            {
                SpawnEnemy();
            }
            
            timer = 0f; // Remise à zéro du timer
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;
        
        Vector2 spawnPos;
        float distance;

        // Trouve une position qui respecte minSpawnDistance et maxSpawnDistance
        do
        {
            // Angle aléatoire
            float angle = Random.Range(0f, Mathf.PI * 2);
            // Distance aléatoire entre min et max
            float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            // Calcul des coordonnées
            float x = Mathf.Cos(angle) * spawnDistance;
            float y = Mathf.Sin(angle) * spawnDistance;
            
            // Position finale relative au joueur
            spawnPos = new Vector2(player.position.x + x, player.position.y + y);
            
            // Calcul de la distance réelle
            distance = Vector2.Distance(player.position, spawnPos);
        }
        while (distance < minSpawnDistance);

        // Instancie l'ennemi
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // Ajoute le tag Enemy s'il n'en a pas déjà
        if (enemy.tag != "Enemy")
        {
            enemy.tag = "Enemy";
        }
    }
}