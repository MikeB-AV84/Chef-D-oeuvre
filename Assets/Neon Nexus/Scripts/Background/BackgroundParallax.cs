using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [Tooltip("How much the background moves relative to the player")]
    public float parallaxEffect = 0.5f;
    
    [Tooltip("The material of the background sprite/texture")]
    private Material backgroundMaterial;
    
    [Tooltip("Reference to the player controller")]
    public PlayerController playerController;
    
    // Store the offset for the texture
    private Vector2 textureOffset = Vector2.zero;

    void Start()
    {
        // Get the material from the renderer
        backgroundMaterial = GetComponent<Renderer>().material;
        
        // If player controller reference is not set in inspector, try to find it
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("BackgroundParallax: PlayerController not found!");
            }
        }
    }

    void Update()
    {
        if (playerController != null)
        {
            // Get the movement input from the player controller
            Vector2 moveInput = playerController.GetMoveInput();
            
            // Update the texture offset in the opposite direction of player movement
            // Scaled by deltaTime for frame rate independence and by parallaxEffect for control
            textureOffset.x -= moveInput.x * parallaxEffect * Time.deltaTime;
            textureOffset.y -= moveInput.y * parallaxEffect * Time.deltaTime;
            
            // Apply the offset to the material
            backgroundMaterial.mainTextureOffset = textureOffset;
        }
    }
}
