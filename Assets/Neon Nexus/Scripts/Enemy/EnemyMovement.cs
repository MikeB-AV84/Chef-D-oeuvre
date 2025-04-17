using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f; // Vitesse de déplacement de l'ennemi.
    private Transform player; // Référence au Transform du joueur.
    private SpriteRenderer spriteRenderer; // Référence au SpriteRenderer pour retourner l'ennemi si nécessaire

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Cherche le joueur via son Tag "Player".
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player != null)
        {
            // Calcule la direction vers le joueur
            Vector2 direction = (player.position - transform.position).normalized;
            
            // Déplace l'ennemi
            transform.position += (Vector3)direction * speed * Time.deltaTime;
            
            // Rotation pour faire face au joueur
            // L'angle est calculé à partir de l'arc tangent de la direction y/x, converti en degrés
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Ajout de 90 degrés car par défaut le sprite fait face vers le haut (suivant la description)
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }
}