using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;  // L'objet astéroïde (sphère ou autre)
    public int numberOfAsteroids = 100; // Nombre d'astéroïdes à générer
    public float spawnRange = 10f;     // Plage de génération autour du joueur

    // Start is called before the first frame update
    void Start()
    {
        SpawnAsteroidPath(); // Génère les astéroïdes au lancement du jeu
        //SpawnAsteroids();
    }

       void SpawnAsteroidPath()
    {
        int numberOfAsteroids = 100; // Nombre total d'astéroïdes
        float pathWidth = 50f; // Largeur du chemin à suivre
        float minZSpacing = 2f; // Distance minimale entre les astéroïdes sur l'axe Z
        float maxZSpacing = 5f; // Distance maximale pour éviter une ligne trop régulière

        float currentZ = 10f; // Position initiale en Z (devant le joueur)

        for (int i = 0; i < numberOfAsteroids; i++)
        {
            // Position X aléatoire dans la largeur du chemin
            float randomX = Random.Range(-pathWidth, pathWidth);
            
            // Ajout d'une légère variation sur l'axe Y
            float randomY = Random.Range(-1f, 1f); 
            
            // Espacement aléatoire en profondeur (Z)
            currentZ += Random.Range(minZSpacing, maxZSpacing);

            // Position finale
            Vector3 spawnPosition = new Vector3(randomX, randomY, currentZ);
            
            // Instanciation de l'astéroïde
            GameObject asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
            
            // Taille aléatoire pour la diversité
            float randomSize = Random.Range(0.5f, 2.0f);
            asteroid.transform.localScale = new Vector3(randomSize, randomSize, randomSize);
        }
    }
       

    

}