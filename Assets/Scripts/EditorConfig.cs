using UnityEngine;

public class EditorConfig : MonoBehaviour
{
    // Public boolean to determine if in Editor mode
    public bool isEditor;

    // Reference to the "Spark" root object
    private GameObject rootObject;

    // Reference to the "Buttons" child object
    private GameObject buttonsObject;

    // Reference to the "Animation" child object that contains the sprite
    private GameObject animationObject;

    // Reference to the MusicSlider script
    private MusicSlider musicSlider;

    private MusicSwitcher musicSwitcher;

    void Start()
    {
        // Find the root object named "Spark"
        rootObject = GameObject.Find("Spark");

        if (rootObject == null)
        {
            Debug.LogError("Root object 'Spark' not found.");
            return;
        }

        // Find the "Buttons" child object within this GameObject
        buttonsObject = transform.Find("Buttons")?.gameObject;

        if (buttonsObject == null)
        {
            Debug.LogError("'Buttons' child object not found.");
            return;
        }

        // Find the "Animation" child object within the rootObject
        animationObject = rootObject.transform.Find("Animation")?.gameObject;

        if (animationObject == null)
        {
            Debug.LogError("'Animation' child object not found within 'Spark'.");
            return;
        }

        // Find the MusicSlider script
        musicSlider = FindObjectOfType<MusicSlider>();

        if (musicSlider == null)
        {
            Debug.LogError("MusicSlider script not found in the scene!");
            return;
        }

        // Find the MusicSlider script
        musicSwitcher = FindObjectOfType<MusicSwitcher>();

        if (musicSwitcher == null)
        {
            Debug.LogError("MusicSwitcher script not found in the scene!");
            return;
        }


        ApplyEditorMode(isEditor);
    }

    void Update()
    {
        // Check if the spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Toggle the editor mode
            ToggleEditorMode(!isEditor);
        }
    }

    // Function to hide all children of the "Buttons" child object
    void HideButtonsChildren()
    {
        foreach (Transform child in buttonsObject.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Function to show all children of the "Buttons" child object
    void ShowButtonsChildren()
    {
        foreach (Transform child in buttonsObject.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    // Function to set the "Invincible" parameter in the PlayerHealth script
    void SetPlayerInvincibility(bool invincible)
    {
        PlayerHealth playerHealth = rootObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetInvincible(invincible);
        }
        else
        {
            Debug.LogWarning("No PlayerHealth script found on root object.");
        }
    }

    // Function to apply the correct editor mode settings
    void ApplyEditorMode(bool enableEditor)
    {
        if (enableEditor)
        {
            ShowButtonsChildren(); // Show buttons in editor mode
            SetPlayerInvincibility(true); // Set invincibility to true
            musicSlider.SetInteractionsEnabled(true); // Enable interactions
            musicSwitcher.SwitchAudioSource("Radio"); // Switch audio source to "Radio"
        }
        else
        {
            HideButtonsChildren(); // Hide buttons in non-editor mode
            SetPlayerInvincibility(false); // Set invincibility to false
            musicSlider.SetInteractionsEnabled(false); // Disable interactions
            musicSwitcher.SwitchAudioSource("Official"); // Switch audio source to "Official"
        }
    }

    // Function to toggle isEditor and apply the appropriate actions
    public void ToggleEditorMode(bool enableEditor)
    {
        isEditor = enableEditor; // Set the isEditor variable
        ApplyEditorMode(isEditor); // Apply the mode based on the new value
    }
}
