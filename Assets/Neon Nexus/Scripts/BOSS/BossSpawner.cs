using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab; // Assign your Boss prefab in Inspector
    public Transform player;      // Assign player transform in Inspector
    public float spawnDistance = 15f;
    
    [Header("Spawning Condition")]
    public int scoreToSpawnBoss = 1000; // Score required to spawn the boss

    private bool bossActive = false;
    private bool bossHasSpawnedThisSession = false; // To prevent multiple spawns if score drops and rises again

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
        if (bossActive || bossHasSpawnedThisSession || player == null) return;

        if (ScoreManager.Instance != null && ScoreManager.Instance.score >= scoreToSpawnBoss)
        {
            SpawnBoss();
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
        bossHasSpawnedThisSession = true; // Mark that boss has been triggered

        Debug.Log("--- BOSS SPAWNING ---");

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
        // Note: bossHasSpawnedThisSession remains true. 
        // If you want the boss to respawn if conditions are met again, reset this flag based on game design.

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