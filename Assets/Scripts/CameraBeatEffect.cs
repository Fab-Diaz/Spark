using UnityEngine;

public class CameraBeatEffect : MonoBehaviour
{
    public float zoomIntensity = 5f; // How intense the zoom effect will be
    public float zoomDuration = 0.1f; // How long the zoom lasts

    public SpriteRenderer backgroundSprite; // Reference to the background SpriteRenderer
    public Color colorA = Color.red; // The starting color (used during zoom out)
    public Color colorB = Color.blue; // The target color (used during zoom in)

    private float zoomTimer;
    private bool isZooming;
    private float targetSize;
    private float initialSize;

    void Start()
    {
        // Store the initial orthographic size of the camera
        initialSize = GetComponent<Camera>().orthographicSize;
        targetSize = initialSize;

        // Initialize background color
        if (backgroundSprite != null)
        {
            backgroundSprite.color = colorA;
        }
    }

    void Update()
    {
        // Handle zoom effect
        if (isZooming)
        {
            zoomTimer -= Time.deltaTime;
            if (zoomTimer <= 0)
            {
                isZooming = false;
                // Reset the camera's orthographic size to the initial size
                GetComponent<Camera>().orthographicSize = initialSize;

                // Reset the background color to colorA during zoom-out
                if (backgroundSprite != null)
                {
                    backgroundSprite.color = colorA;
                }
            }
            else
            {
                // Smoothly interpolate the size
                float t = 1 - (zoomTimer / zoomDuration);
                GetComponent<Camera>().orthographicSize = Mathf.Lerp(targetSize, initialSize, t);

                // Smoothly interpolate the background color from colorB to colorA during zoom-out
                if (backgroundSprite != null)
                {
                    backgroundSprite.color = Color.Lerp(colorB, colorA, t);
                }
            }
        }
    }

    // This function can be called by other scripts to trigger the beat effect
    public void TriggerBeat()
    {
        // Set the target size for the zoom effect
        targetSize = initialSize - zoomIntensity;
        isZooming = true;
        zoomTimer = zoomDuration;

        // Set the background color to colorB during zoom-in
        if (backgroundSprite != null)
        {
            backgroundSprite.color = colorB;
        }
    }
}
