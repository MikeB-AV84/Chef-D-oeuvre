using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    
    [Header("Base Difficulty Settings")]
    public float baseSpawnInterval = 2f; // Starting spawn interval
    public float minSpawnInterval = 0.5f; // Fastest possible spawn rate
    public int baseMaxEnemies = 5; // Starting max enemies on screen
    public int maxPossibleEnemies = 80; // Maximum enemies allowed on screen
    
    [Header("Speed Scaling")]
    public float baseEnemySpeed = 2f; // Starting enemy speed
    public float maxEnemySpeed = 6f; // Maximum enemy speed
    
    [Header("Difficulty Progression")]
    public int scorePerDifficultyIncrease = 100; // Score needed for each difficulty bump
    public float spawnRateIncrease = 0.15f; // How much faster spawning gets per level
    public float speedIncrease = 0.3f; // How much faster enemies get per level
    public int enemyCountIncrease = 2; // How many more enemies per difficulty level
    
    [Header("Spawn Distance")]
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 20f;
    
    private Transform player;
    private float timer;
    private int currentDifficultyLevel = 0;
    private int lastScoreCheck = 0;
    
    // Current difficulty values
    private float currentSpawnInterval;
    private int currentMaxEnemies;
    private float currentEnemySpeed;

    void Start()
    {
        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("No player found with tag 'Player'");
                return;
            }
        }
        
        // Initialize difficulty values
        UpdateDifficulty();
        
        // Spawn initial enemies (fewer at start)
        for (int i = 0; i < 3; i++)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        if (player == null) return;
        
        // Check if difficulty should increase based on score
        CheckDifficultyIncrease();
        
        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            // Debug info to help diagnose spawning issues
            if (existingEnemies.Length >= currentMaxEnemies)
            {
                Debug.Log($"Max enemies reached: {existingEnemies.Length}/{currentMaxEnemies}");
            }
            
            if (existingEnemies.Length < currentMaxEnemies)
            {
                SpawnEnemy();
                Debug.Log($"Spawned enemy. Current count: {existingEnemies.Length + 1}/{currentMaxEnemies}");
            }
            
            // Always reset timer, regardless of whether we spawned
            timer = 0f;
        }
    }
    
    void CheckDifficultyIncrease()
    {
        if (ScoreManager.Instance == null) 
        {
            Debug.LogWarning("ScoreManager.Instance is null!");
            return;
        }
        
        int currentScore = ScoreManager.Instance.score;
        int newDifficultyLevel = currentScore / scorePerDifficultyIncrease;
        
        if (newDifficultyLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            UpdateDifficulty();
            
            Debug.Log($"Difficulty increased to level {currentDifficultyLevel}!");
            Debug.Log($"Spawn interval: {currentSpawnInterval:F2}s, Max enemies: {currentMaxEnemies}, Enemy speed: {currentEnemySpeed:F2}");
        }
    }
    
    void UpdateDifficulty()
    {
        // Calculate spawn rate (gets faster with difficulty)
        currentSpawnInterval = Mathf.Max(
            minSpawnInterval, 
            baseSpawnInterval - (currentDifficultyLevel * spawnRateIncrease)
        );
        
        // Calculate max enemies (DOUBLES each difficulty level)
        currentMaxEnemies = Mathf.Min(
            maxPossibleEnemies,
            baseMaxEnemies + (currentDifficultyLevel * enemyCountIncrease)
        );
        
        // Calculate enemy speed (gets faster with difficulty)
        currentEnemySpeed = Mathf.Min(
            maxEnemySpeed,
            baseEnemySpeed + (currentDifficultyLevel * speedIncrease)
        );
    }

    void SpawnEnemy()
    {
        if (player == null) return;
        
        Vector2 spawnPos = GetRandomSpawnPosition();
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // Set enemy tag if not already set
        if (enemy.tag != "Enemy")
        {
            enemy.tag = "Enemy";
        }
        
        // Apply current difficulty speed to the enemy
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
            float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            float x = Mathf.Cos(angle) * spawnDistance;
            float y = Mathf.Sin(angle) * spawnDistance;
            
            spawnPos = new Vector2(player.position.x + x, player.position.y + y);
            distance = Vector2.Distance(player.position, spawnPos);
            
            attempts++;
        }
        while (distance < minSpawnDistance && attempts < maxAttempts);

        return spawnPos;
    }
    
    // Public method to get current difficulty info (useful for UI or debugging)
    public string GetDifficultyInfo()
    {
        return $"Level: {currentDifficultyLevel}, Spawn Rate: {currentSpawnInterval:F2}s, Max Enemies: {currentMaxEnemies}, Speed: {currentEnemySpeed:F2}";
    }
    
    // Method to manually test difficulty increase (for debugging)
    [ContextMenu("Increase Difficulty")]
    void TestDifficultyIncrease()
    {
        currentDifficultyLevel++;
        UpdateDifficulty();
        Debug.Log("Manual difficulty increase: " + GetDifficultyInfo());
    }
}