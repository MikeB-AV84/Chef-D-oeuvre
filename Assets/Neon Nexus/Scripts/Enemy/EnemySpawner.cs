using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    
    [Header("Base Difficulty Settings")]
    public float baseSpawnInterval = 2f;
    public float minSpawnInterval = 0.5f;
    public int baseMaxEnemies = 5;
    public int maxPossibleEnemies = 80;
    
    [Header("Speed Scaling")]
    public float baseEnemySpeed = 2f;
    public float maxEnemySpeed = 6f;
    
    [Header("Difficulty Progression")]
    public int scorePerDifficultyIncrease = 100;
    public float spawnRateIncrease = 0.15f;
    public float speedIncrease = 0.3f;
    public int enemyCountIncrease = 2;
    
    [Header("Spawn Distance")]
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;
    
    private Transform player;
    private float timer;
    private int currentDifficultyLevel = 0;
    // private int lastScoreCheck = 0; // Not used
    
    private float currentSpawnInterval;
    private int currentMaxEnemies;
    private float currentEnemySpeed;

    private bool isSpawnerPaused = false; // NEW: Flag to pause spawner

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("EnemySpawner: No player found with tag 'Player'. Disabling spawner.");
                enabled = false; // Disable component if no player
                return;
            }
        }
        
        UpdateDifficulty();
        
        for (int i = 0; i < 3; i++)
        {
            if (CanSpawn()) SpawnEnemy();
        }
    }

    void Update()
    {
        if (player == null || isSpawnerPaused) return; // Check if paused
        
        CheckDifficultyIncrease();
        
        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            if (CanSpawn())
            {
                SpawnEnemy();
                // Debug.Log($"Spawned enemy. Current count: {GameObject.FindGameObjectsWithTag("Enemy").Length}/{currentMaxEnemies}");
            }
            timer = 0f;
        }
    }

    bool CanSpawn()
    {
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        return existingEnemies.Length < currentMaxEnemies;
    }
    
    void CheckDifficultyIncrease()
    {
        if (ScoreManager.Instance == null) return;
        
        int currentScore = ScoreManager.Instance.score;
        int newDifficultyLevel = currentScore / scorePerDifficultyIncrease;
        
        if (newDifficultyLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            UpdateDifficulty();
            // Debug.Log($"Difficulty increased to level {currentDifficultyLevel}!");
            // Debug.Log($"Spawn interval: {currentSpawnInterval:F2}s, Max enemies: {currentMaxEnemies}, Enemy speed: {currentEnemySpeed:F2}");
        }
    }
    
    void UpdateDifficulty()
    {
        currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentDifficultyLevel * spawnRateIncrease));
        currentMaxEnemies = Mathf.Min(maxPossibleEnemies, baseMaxEnemies + (currentDifficultyLevel * enemyCountIncrease));
        currentEnemySpeed = Mathf.Min(maxEnemySpeed, baseEnemySpeed + (currentDifficultyLevel * speedIncrease));
    }

    void SpawnEnemy()
    {
        if (player == null) return;
        
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        if (enemy.tag != "Enemy") enemy.tag = "Enemy";
        
        EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.speed = currentEnemySpeed;
        }
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        Vector2 spawnPos;
        float distance;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float spawnDistanceValue = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            float x = Mathf.Cos(angle) * spawnDistanceValue;
            float y = Mathf.Sin(angle) * spawnDistanceValue;
            
            spawnPos = (player != null) ? new Vector2(player.position.x + x, player.position.y + y) : Vector2.zero;
            distance = (player != null) ? Vector2.Distance(player.position, spawnPos) : 0;
            
            attempts++;
        }
        while (player != null && distance < minSpawnDistance && attempts < maxAttempts);

        return spawnPos;
    }
    
    public string GetDifficultyInfo()
    {
        return $"Level: {currentDifficultyLevel}, Spawn Rate: {currentSpawnInterval:F2}s, Max Enemies: {currentMaxEnemies}, Speed: {currentEnemySpeed:F2}";
    }
    
    [ContextMenu("Increase Difficulty")]
    void TestDifficultyIncrease()
    {
        currentDifficultyLevel++;
        UpdateDifficulty();
        Debug.Log("Manual difficulty increase: " + GetDifficultyInfo());
    }

    // --- NEW Methods to control spawner state ---
    public void PauseSpawner()
    {
        isSpawnerPaused = true;
        Debug.Log("Enemy Spawner Paused");
    }

    public void ResumeSpawner()
    {
        isSpawnerPaused = false;
        timer = 0f; // Reset timer to avoid immediate spawn if interval was met while paused
        Debug.Log("Enemy Spawner Resumed");
    }
}