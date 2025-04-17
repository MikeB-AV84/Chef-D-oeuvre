using UnityEngine;

public class LayerSetup : MonoBehaviour
{
    // Placez ce script sur un GameObject dans votre scène
    
    void Start()
    {
        // Configuration des layers pour tous les objets existants
        SetupLayers();
    }
    
    public static void SetupLayers()
    {
        // Trouver tous les ennemis et assigner le layer Enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.layer = LayerMask.NameToLayer("Enemy");
        }
        
        // Trouver toutes les balles ennemies et assigner le layer EnemyBullet
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in enemyBullets)
        {
            bullet.layer = LayerMask.NameToLayer("EnemyBullet");
        }
        
        Debug.Log("Layers configurés pour les ennemis et leurs projectiles");
    }
}
