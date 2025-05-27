using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    public float desiredDistance = 5f; // Configurable in Inspector: how far the enemy should stay from the player
    private Transform player;
    private SpriteRenderer spriteRenderer; // Retained from your original script

    void Start()
    {
        // Find the player GameObject by its tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            // Log an error and disable the script if the player isn't found
            // to prevent further errors in Update()
            Debug.LogError("Player GameObject not found. Ensure it has the 'Player' tag and is active in the scene.");
            enabled = false; // Disables this script component
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        // You could add a check here if spriteRenderer is essential for other logic:
        // if (spriteRenderer == null) Debug.LogWarning("SpriteRenderer component not found on this enemy.");
    }

    void Update()
    {
        if (player == null) return; // Safeguard, although Start() should prevent this if player isn't found

        // Calculate the vector from the enemy to the player
        Vector2 vectorToPlayer = (Vector2)player.position - (Vector2)transform.position;
        // Calculate the current distance to the player
        float distanceToPlayer = vectorToPlayer.magnitude;

        // --- Movement Decision ---
        // A small tolerance helps prevent the enemy from rapidly starting/stopping
        // or "jittering" if it's exactly at the desiredDistance.
        // This creates a small "dead zone" where the enemy will not move.
        float tolerance = 0.05f; // You can tune this value based on your game's feel.

        if (distanceToPlayer > desiredDistance + tolerance)
        {
            // Enemy is too far away: Move towards the player.
            Vector2 directionToMove = vectorToPlayer.normalized; // Get the unit vector pointing to the player
            transform.position += (Vector3)directionToMove * speed * Time.deltaTime;
        }
        else if (distanceToPlayer < desiredDistance - tolerance)
        {
            // Enemy is too close: Move away from the player.
            // This helps maintain the 'desiredDistance' and prevents enemies from
            // getting on top of the player or each other too much.
            Vector2 directionToMove = -vectorToPlayer.normalized; // Move in the opposite direction from the player
            transform.position += (Vector3)directionToMove * speed * Time.deltaTime;
        }
        // Else (distanceToPlayer is within desiredDistance +/- tolerance):
        // The enemy is at the desired distance and will stop moving.
        // No code is needed here for stopping, as no movement is applied.

        // --- Rotation ---
        // Make the enemy always face the player, regardless of whether it's moving or stopped.
        // This makes the enemy seem more aware of the player.
        if (vectorToPlayer != Vector2.zero) // Check to prevent issues if distance is exactly zero (enemy on player)
        {
            // Calculate the angle needed to face the player.
            // Mathf.Atan2 returns the angle in radians between the x-axis and the point (x,y).
            float angle = Mathf.Atan2(vectorToPlayer.y, vectorToPlayer.x) * Mathf.Rad2Deg;

            // The -90 degree offset is commonly used if your sprite's "forward" direction is its "up" (positive Y-axis).
            // If your sprite faces "right" (positive X-axis) by default, you might not need this offset,
            // or you might need a different one (e.g., +90 if it faces "left").
            // This line matches the rotation adjustment from your original script.
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
}