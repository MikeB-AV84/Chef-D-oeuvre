using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab du projectile ennemi.
    public float shootInterval = 2f; // Délai entre chaque tir.
    public float bulletSpeed = 5f; // Vitesse des projectiles

    private float shootTimer; // Timer pour le tir.
    private Coroutine shootCoroutine; // Référence à la coroutine de tir pour l'annuler en cas de besoin.
    private bool isDestroyed = false; // Flag pour éviter les doubles destructions

    void Start()
    {
        shootCoroutine = StartCoroutine(ShootRoutine()); // Démarre la coroutine de tir dès le début.
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        // Si l'ennemi est toujours vivant, il continue à tirer en suivant l'intervalle.
        if (shootTimer >= shootInterval && shootCoroutine == null)
        {
            shootCoroutine = StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        // Attendre jusqu'au prochain tir
        yield return new WaitForSeconds(shootInterval);

        Shoot(); // Appeler la méthode Shoot() pour créer un projectile

        shootTimer = 0; // Réinitialiser le timer
        shootCoroutine = null; // Libérer la référence de la coroutine
    }

    void Shoot()
    {
        Debug.Log("Enemy is shooting! " + gameObject.name);
        
        // Calcule la direction de la balle en fonction de la rotation de l'ennemi
        float angle = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // Décale légèrement la position de spawn pour éviter une collision immédiate
        Vector2 spawnPosition = (Vector2)transform.position + direction * 0.5f;
        
        // Crée la balle à la position décalée
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, transform.rotation);
        
        // Récupérer le Rigidbody2D du projectile
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Appliquer la vélocité dans la direction où l'ennemi regarde
            rb.linearVelocity = direction * bulletSpeed;
        }
        
        // Assurez-vous que la balle a le tag approprié
        bullet.tag = "EnemyBullet";
        
        // Ignorer les collisions avec tous les ennemis
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in allEnemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, enemyCollider, true);
            }
        }
    }

    // Cette méthode arrête la coroutine quand l'ennemi est détruit
    public void StopShooting()
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine); // Arrête la coroutine de tir.
            shootCoroutine = null;
        }
        isDestroyed = true;
    }

    void OnDestroy()
    {
        // IMPORTANT: NE PAS détruire le GameObject ici, sinon on crée une boucle
        // NE PAS appeler StopShooting() si déjà détruit
        if (!isDestroyed)
        {
            StopShooting();
        }
        
        // Ajouter des points au score SEULEMENT si c'est la première destruction
        // Mais cela devrait être géré par Enemy.DestroyEnemy() à la place
        /*
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
            Debug.Log("100 points ajoutés.");
        }
        */
    }
}