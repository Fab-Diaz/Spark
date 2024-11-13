using UnityEngine;
using UnityEngine.UI;
using BulletTimerControllerNamespace;

public class MusicSwitcher : MonoBehaviour
{
    // Reference to the main AudioSource that should be replaced
    public AudioSource mainAudioSource;

    // Reference to the EditorConfig script
    public EditorConfig editorConfig;

    // Store the currently active button
    private GameObject activeButton;

    private Button[] buttons;

    public Slider musicSlider;  // Reference to the slider

    private bool levelCompleted = false; // Track if the level is completed
    public Button exportFileButton; // Reference to the ExportFile button

    void Start()
    {
        // Find the parent object containing all buttons
        Transform buttonsParent = GameObject.Find("Buttons/MusicControls/Songs").transform;

        if (buttonsParent != null)
        {
            // Get all Button components in the children of the buttonsParent object
            buttons = buttonsParent.GetComponentsInChildren<Button>();

            if (buttons.Length > 0)
            {
                // Set the first button as the active button by default
                activeButton = buttons[0].gameObject;
                // Switch audio based on the initial active button
                string type = editorConfig.isEditor ? "Radio" : "Official";
                SwitchAudioSource(type);
            }
            else
            {
                Debug.LogError("No buttons found under Buttons/MusicControls/Songs");
            }

            // Add listeners to all buttons
            foreach (Button button in buttons)
            {
                button.onClick.AddListener(() => OnButtonClick(button.gameObject));
            }
        }
        else
        {
            Debug.LogError("Button parent not found at path: Buttons/MusicControls/Songs");
        }

        // Disable the ExportFile button at the start
        if (exportFileButton != null)
        {
            exportFileButton.interactable = false;
            SetButtonColor(exportFileButton, Color.red);
        }
    }

    void Update()
    {
        // Check if the level is completed
        if (!levelCompleted && musicSlider != null)
        {
            // Calculate if the slider has reached its max value and if not in Editor mode
            if (musicSlider.value >= musicSlider.maxValue && !editorConfig.isEditor)
            {
                levelCompleted = true;
            }
        }

        // Enable/Disable the ExportFile button based on levelCompleted status
        if (exportFileButton != null)
        {
            exportFileButton.interactable = levelCompleted;
            SetButtonColor(exportFileButton, levelCompleted ? Color.green : Color.red);
        }
    }

    // This function is called when any button is clicked
    void OnButtonClick(GameObject clickedButton)
    {
        // Set the active button to the one that was clicked
        activeButton = clickedButton;

        // Switch the audio source based on the editor config
        string type = editorConfig.isEditor ? "Radio" : "Official";
        SwitchAudioSource(type);
    }

    // Function to switch audio based on the type (e.g., "Radio" or "Official")
    public void SwitchAudioSource(string type)
    {
        if (activeButton == null)
        {
            Debug.LogError("No active button set.");
            return;
        }

        // Find the corresponding child based on the type
        Transform audioTransform = activeButton.transform.Find(type);

        if (audioTransform != null)
        {
            // Get the AudioSource component from the corresponding child
            AudioSource newAudioSource = audioTransform.GetComponent<AudioSource>();

            if (newAudioSource != null)
            {
                // Replace the main audio source clip with the new one
                mainAudioSource.clip = newAudioSource.clip;

                // Play the new audio clip
                mainAudioSource.Play();
            }
            else
            {
                Debug.LogError("No AudioSource found on the " + type + " child of " + activeButton.name);
            }
        }
        else
        {
            Debug.LogError(type + " child not found on " + activeButton.name);
        }
    }

    // Function to get the index of the active button
    public int getActiveBulletIndex()
    {
        // If activeButton is not null and buttons array is populated
        if (activeButton != null && buttons != null)
        {
            // Loop through the buttons array to find the active button
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].gameObject == activeButton)
                {
                    return i; // Return the index of the active button
                }
            }
        }

        // Return -1 if active button is not found or if something went wrong
        return -1;
    }

    public void LoadSongFromJson(string output)
    {
                    // Deserialize from JSON
            TimedBulletTriggerFile file = JsonUtility.FromJson<TimedBulletTriggerFile>(output);

        // Check if the buttonIndex is within the valid range
        if (buttons != null && file.song >= 0 && file.song < buttons.Length)
        {
            // Set the active button to the button at the specified index
            activeButton = buttons[file.song].gameObject;

            SwitchAudioSource("Official");
        }
        else
        {
            Debug.LogError("Invalid button index: " + file.song);
        }
    }

    // Function to set the color of the button's Image component
    private void SetButtonColor(Button button, Color color)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    public void SetLevelCompleted(bool completed)
    {
        this.levelCompleted = completed;
    }
}
