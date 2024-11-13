using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BulletTimerControllerNamespace;

public class MusicSlider : MonoBehaviour
{
    public Slider progressBar;  // Reference to the UI Slider component
    public AudioSource audioSource;  // Reference to the AudioSource component

    // Buttons for slowing and resetting overall time
    public Button timeButton0;
    public Button timeButton25; 
    public Button timeButton50; 
    public Button timeButton75; 
    public Button timeButton100;

    public bool enabled = true;

    // Reference to the BulletTimerController
    public BulletTimerController bulletTimerController;

    private bool isDragging = false;  // Flag to check if the slider is being dragged

    void Start()
    {
        // Set the initial value of the progress bar to 0
        progressBar.value = 0f;
        progressBar.maxValue = 1f;

        // Add listeners for detecting dragging and releasing the slider
        progressBar.onValueChanged.AddListener(OnSliderValueChanged);

        // Add event listeners for pointer events
        EventTrigger trigger = progressBar.gameObject.AddComponent<EventTrigger>();

        // OnPointerDown
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => { OnPointerDown(); });
        trigger.triggers.Add(entryDown);

        // OnPointerUp
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => { OnPointerUp(); });
        trigger.triggers.Add(entryUp);

        // Add listeners to the slow and normal time buttons.
        timeButton0.onClick.AddListener(() => SetGameTimeScale(0));
        timeButton25.onClick.AddListener(() => SetGameTimeScale(0.25f));
        timeButton50.onClick.AddListener(() => SetGameTimeScale(0.5f));
        timeButton75.onClick.AddListener(() => SetGameTimeScale(0.75f));
        timeButton100.onClick.AddListener(() => SetGameTimeScale(1f));
    }

    void Update()
    {
        // Update the slider value to match the audio time if not dragging
        if (!isDragging)
        {
            progressBar.value = audioSource.time / audioSource.clip.length;
        }
    }

    // Method called when the slider's value is changed by the user
    void OnSliderValueChanged(float value)
    {
        if (isDragging)
        {
            // Set the audio playback time to match the slider's value
            audioSource.time = value * audioSource.clip.length;
        }
    }

    // These methods detect when the user starts and stops dragging the slider
    public void OnPointerDown()
    {
        if (!enabled)
        {
            return;
        }
        isDragging = true;
    }

    public void OnPointerUp()
    {
        if (!enabled)
        {
            return;
        }
        isDragging = false;

        // Update the audio time when the user releases the slider
        audioSource.time = progressBar.value * audioSource.clip.length;
        resetBullets();
    }

    public void resetBullets() {
        // Ensure bulletTimerController is assigned before calling its method
        if (bulletTimerController != null)
        {
            bulletTimerController.resetToSpecificTime(audioSource.time);
        }
        else
        {
            Debug.LogError("BulletTimerController is not assigned!");
        }
    }

    // Method to adjust the overall game time scale and music pitch
    void SetGameTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;  // Adjust physics step to match time scale
        audioSource.pitch = timeScale;  // Adjust the pitch to match the time scale
    }

    // Method to enable or disable all interactions
    public void SetInteractionsEnabled(bool interactionsEnabled)
    {
        progressBar.interactable = interactionsEnabled;
        enabled = interactionsEnabled;
        SetGameTimeScale(1f);  // Reset the game time scale
        audioSource.time = 0;
        resetBullets();
    }
}
