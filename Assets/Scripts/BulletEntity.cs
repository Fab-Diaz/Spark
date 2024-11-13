using UnityEngine;
using BulletTimerControllerNamespace;

public class BulletEntity : MonoBehaviour
{
    public string bulletPath; // Path to the bullet or identifier
    public float triggerTime; // Time at which the bullet should be removed

    private BulletTimerController bulletTimerController;
    public EditorConfig editorConfig;

    private void Start()
    {
        // Get the BulletTimerController from the scene
        bulletTimerController = FindObjectOfType<BulletTimerController>();

        if (bulletTimerController == null)
        {
            Debug.LogError("BulletTimerController not found in the scene.");
        }
    }

    private void OnMouseDown()
    {
        if (bulletTimerController != null && editorConfig.isEditor)
        {
            // Call RemoveBullet on the BulletTimerController
            bulletTimerController.RemoveBullet(bulletPath, triggerTime);
            // Destroy this game object
            Destroy(gameObject);
        }
    }
}
