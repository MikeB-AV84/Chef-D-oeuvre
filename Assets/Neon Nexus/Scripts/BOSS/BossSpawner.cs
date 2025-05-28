using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab; // Assign your Boss prefab in Inspector
    public Transform player;      // Assign player transform in Inspector
    public float spawnDistance = 15f;
    
    [Header("Spawning Condition")]
    public int scoreToSpawnBoss = 1000; // Score required to spawn the first boss

    private bool bossActive = false;
    private int lastBossSpawnLevel = 0; // Track which boss level we've spawned (1 = 1000 points, 2 = 2000 points, etc.)

    // References to other managers (assign in Inspector or find in Start)
    public EnemySpawner enemySpawner; // Assign your EnemySpawner instance

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else Debug.LogError("BossSpawner: Player not found!");
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner == null) Debug.LogError("BossSpawner: EnemySpawner not found!");
        }
    }

    void Update()
    {
        if (bossActive || player == null) return;

        if (ScoreManager.Instance != null)
        {
            // Calculate current boss level (1 for 1000-1999, 2 for 2000-2999, etc.)
            int currentBossLevel = ScoreManager.Instance.score / scoreToSpawnBoss;
            
            // Check if we've reached a new boss level and haven't spawned that boss yet
            if (currentBossLevel > lastBossSpawnLevel && ScoreManager.Instance.score >= scoreToSpawnBoss)
            {
                SpawnBoss();
            }
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab == null || player == null)
        {
            Debug.LogError("BossSpawner: Boss Prefab or Player not assigned. Cannot spawn boss.");
            return;
        }

        bossActive = true;
        
        // Update the last spawned boss level
        int currentBossLevel = ScoreManager.Instance.score / scoreToSpawnBoss;
        lastBossSpawnLevel = currentBossLevel;

        Debug.Log($"--- BOSS SPAWNING --- Level {currentBossLevel} at score {ScoreManager.Instance.score}");

        // Pause Enemy Spawner
        if (enemySpawner != null)
        {
            enemySpawner.PauseSpawner();
        }

        // Pause regular music & Play Boss Music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
        }

        // Spawn Boss
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        if (spawnDirection == Vector2.zero) spawnDirection = Vector2.up; // Avoid zero vector
        Vector2 spawnPosition = (Vector2)player.position + spawnDirection * spawnDistance;
        
        GameObject bossGO = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        Boss bossComponent = bossGO.GetComponent<Boss>();

        if (bossComponent != null)
        {
            bossComponent.OnBossDefeated += HandleBossDefeated;
            // Show Boss Health Bar
            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.ShowHealthBar(bossComponent.maxHealth);
            }
        }
        else
        {
            Debug.LogError("Boss prefab does not have a Boss component!");
            // Need to handle this error, maybe revert states
            HandleBossDefeated(); // Clean up as if boss was instantly defeated (error state)
        }
    }

    void HandleBossDefeated()
    {
        Debug.Log("--- BOSS DEFEATED --- Handling post-defeat sequence.");
        bossActive = false;
        // lastBossSpawnLevel remains at the current level, allowing the next boss to spawn at the next milestone

        // Stop Boss Music & Resume Regular Music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBossMusic();
            AudioManager.Instance.ResumeMainTrackAfterBoss();
        }

        // Resume Enemy Spawner
        if (enemySpawner != null)
        {
            enemySpawner.ResumeSpawner();
        }

        // Hide Boss Health Bar
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.HideHealthBar();
        }
    }
}