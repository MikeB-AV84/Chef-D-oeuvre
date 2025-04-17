using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 0.5f; // Vitesse de défilement
    public Transform player;        // Référence au joueur
    private Material backgroundMaterial;
    private Vector2 offset;

    void Start()
    {
        // Récupérer le matériau appliqué à l'objet
        backgroundMaterial = GetComponent<Renderer>().material;

        // Initialiser le décalage
        offset = Vector3.zero;
    }

    void Update()
    {
        if (player != null)
        {
            // Synchroniser le défilement avec le mouvement du joueur
            offset.x = player.position.x * scrollSpeed; // Défilement horizontal
            offset.y = player.position.z * scrollSpeed; // Défilement vertical (si nécessaire)

            // Appliquer le décalage au matériau
            backgroundMaterial.mainTextureOffset = offset;
        }
    }
}
