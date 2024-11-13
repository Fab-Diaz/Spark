using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float verticalAmplitude = 0.5f;    // Height of the vertical floating motion
    public float horizontalAmplitude = 0.5f;  // Width of the horizontal floating motion
    public float frequency = 1f;              // Speed of the floating motion
    public float rotationSpeed = 30f;         // Speed of rotation in degrees per second
    public float randomFactor = 0.2f;         // Adds randomness to the motion

    private Vector3 startPosition;
    private float randomOffset;
    private float elapsedTime;

    void Start()
    {
        // Save the starting position of the object
        startPosition = transform.position;

        // Generate a random offset to add variability to the motion
        randomOffset = Random.Range(-randomFactor, randomFactor);
    }

    void Update()
    {
        // Update the elapsed time
        elapsedTime += Time.deltaTime * frequency;

        // Calculate the vertical and horizontal positions
        float newX = startPosition.x + Mathf.Sin(elapsedTime + randomOffset) * horizontalAmplitude;
        float newY = startPosition.y + Mathf.Sin(elapsedTime * 2 + randomOffset) * verticalAmplitude;

        // Apply the new position
        transform.position = new Vector3(newX, newY, startPosition.z);

        // Add some random rotation to the object
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
