using UnityEngine;

public class RotatingCircle : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 30f;

    void Update()
    {
        // Rotate around the Z-axis at a constant speed
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}