using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;
    public Transform player;
    public float spawnDistance = 15f;
    private int nextSpawnThreshold = 200;
    private bool bossActive = false;

    void Update()
    {
        if (!bossActive && ScoreManager.Instance != null && ScoreManager.Instance.score >= nextSpawnThreshold)
        {
            SpawnBoss();
            nextSpawnThreshold += 200; // Set next threshold
            bossActive = true;
        }
    }

    void SpawnBoss()
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)player.position + spawnDirection * spawnDistance;
        
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        boss.GetComponent<Boss>().OnBossDefeated += HandleBossDefeated;
    }

    void HandleBossDefeated()
    {
        bossActive = false;
    }
}
