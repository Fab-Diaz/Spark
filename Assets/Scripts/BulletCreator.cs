using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BulletTimerControllerNamespace;

public class BulletCreator : MonoBehaviour
{
    [System.Serializable]
    public class ButtonPathPair
    {
        public Button button;
        public string bulletPath;
    }

    private ButtonPathPair[] buttonPathPairs; // Private array of button-path pairs
    public AudioSource audioSource; // Assign the audio source in the inspector
    public GameObject bulletController; // Assign the BulletController_1 object in the inspector.
    public ExportFile exportFile;
    public MusicSwitcher musicSwitcher;

    private string activeBulletName = "Bullet";

    void Start()
    {
        // Path prefixes and suffixes
        string[] topMiddleBottomPositions = { "Top", "Middle", "Bottom" };
        string[] leftRightDirections = { "left_right", "right_left" };

        string[] firstSecondThirdPositions = { "First", "Second", "Third" };
        string[] upDownDirections = { "up_down", "down_up" };

        // Initialize the buttonPathPairs array
        buttonPathPairs = new ButtonPathPair[14]; // 6 paths for Top, Middle, Bottom, and 6 for First, Second, Third + Checkpoint + Beat

        // Add checkpoint button to the buttonPathPairs
        GenerateCustomPathPairs("Buttons/Spawn_checkpoint/Checkpoint_marker", "checkpoint", 0);

        // Add beat button to the buttonPathPairs
        GenerateCustomPathPairs("Buttons/MusicControls/Beat/Beat_marker", "beat", 1);

        // Generate ButtonPathPairs for Top, Middle, Bottom with left_right and right_left directions
        int index = GenerateButtonPathPairs(topMiddleBottomPositions, leftRightDirections, 2);

        // Generate ButtonPathPairs for First, Second, Third with up_down and down_up directions
        index = GenerateButtonPathPairs(firstSecondThirdPositions, upDownDirections, index);

        // Add listeners to each button with its specific path
        foreach (var pair in buttonPathPairs)
        {
            if (pair.button != null)
            {
                string path = pair.bulletPath;
                pair.button.onClick.AddListener(() => OnModifyButtonClick(path));
            }
        }
    }

    void GenerateCustomPathPairs(string buttonPath, string bulletPath, int index)
    {
        string checkpointPath = buttonPath;
        GameObject buttonObject = GameObject.Find(checkpointPath);
        Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        buttonPathPairs[index] = new ButtonPathPair
        {
            button = button,
            bulletPath = bulletPath
        };
    }

    // Reusable function to generate ButtonPathPairs
    int GenerateButtonPathPairs(string[] positions, string[] directions, int startIndex)
    {
        int index = startIndex;

        for (int i = 0; i < positions.Length; i++)
        {
            for (int j = 0; j < directions.Length; j++)
            {
                // Construct the GameObject name
                string buttonName = $"Spawn_{positions[i].ToLower()}_{directions[j].ToLower()}";

                // Find the Button component in the hierarchy
                GameObject buttonObject = GameObject.Find($"Buttons/Spawners/{buttonName}");
                Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;

                if (button != null)
                {
                    buttonPathPairs[index] = new ButtonPathPair
                    {
                        button = button, // Assign the found button
                        bulletPath = $"Bullets/{positions[i]}_{directions[j]}"
                    };
                    index++;
                }
                else
                {
                    Debug.LogWarning($"Button '{buttonName}' not found.");
                }
            }
        }

        return index; // Return the next available index
    }

    void OnModifyButtonClick(string bulletPath)
    {
        // Get the BulletTimerController component from the bulletController object
        BulletTimerController bulletTimerController = bulletController.GetComponent<BulletTimerController>();

        if (bulletTimerController != null && audioSource != null)
        {
            // Get the current time of the audio source in seconds
            float currentTime = audioSource.time;

            // Convert the time into minutes, seconds, and milliseconds
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            int milliseconds = Mathf.FloorToInt((currentTime * 1000f) % 1000f);
            bool isCustom = bulletPath == "checkpoint" || bulletPath == "beat";
            int delay = isCustom ? 0 : 3;

            // Subtract 3 seconds, adjusting minutes if necessary
            seconds -= delay;
            if (seconds < 0)
            {
                seconds += 60;
                minutes = Mathf.Max(0, minutes - 1);
            }

            // Format the time string as "MM:SS:MS"
            string triggerTimeString = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            string fullBulletPath = isCustom ? bulletPath : bulletPath + "/" + activeBulletName;

            // Create a new TimedBulletTrigger instance with the desired path and formatted time
            TimedBulletTrigger newBulletTrigger = new TimedBulletTrigger
            {
                bulletPath = fullBulletPath,
                triggerTimeString = triggerTimeString,
                triggerTime = currentTime - delay, // Adjust the trigger time by 3 seconds
                triggered = false
            };

            // Add the new trigger to the BulletTriggers list
            bulletTimerController.bulletTriggers.Add(newBulletTrigger);

            // Create a list to hold TimedBulletTriggerSimplified instances
            List<TimedBulletTriggerSimplified> triggersSimplified = new List<TimedBulletTriggerSimplified>();

            // Loop through each item in bulletTimerController.bulletTriggers
            foreach (var trigger in bulletTimerController.bulletTriggers)
            {
                if (trigger.bulletPath == "checkpoint" || trigger.bulletPath == "beat"){
                    TimedBulletTriggerSimplified triggerSimplified = new TimedBulletTriggerSimplified
                    {
                        type = trigger.bulletPath,
                        triggerTimeString = trigger.triggerTimeString
                    };

                    triggersSimplified.Add(triggerSimplified);
                } else {
                    // Parse the bulletPath to get "rowOrColumn", "from", "top", and "type"
                    var parsedItem = ParseBulletPath(trigger.bulletPath);

                    TimedBulletTriggerSimplified triggerSimplified = new TimedBulletTriggerSimplified
                    {
                        rowOrColumn = parsedItem.rowOrColumn,
                        from = parsedItem.from,
                        to = parsedItem.to,
                        type = parsedItem.type,
                        triggerTimeString = trigger.triggerTimeString
                    };

                    triggersSimplified.Add(triggerSimplified);
                }
            }

            // Create a wrapper instance for the list
            TimedBulletTriggerFile triggerList = new TimedBulletTriggerFile
            {
                song = musicSwitcher.getActiveBulletIndex(),
                triggers = triggersSimplified
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(triggerList, true);

            exportFile.data = json;

            musicSwitcher.SetLevelCompleted(false);
        }
        else
        {
            Debug.LogError("BulletTimerController or AudioSource is not assigned properly.");
        }
    }

    private ParsedBulletPath ParseBulletPath(string bulletPath)
    {
        // Split the bulletPath by '/'
        string[] pathComponents = bulletPath.Split('/');

        // Assuming the structure is "Bullets/Top_left_right/Bullet"
        if (pathComponents.Length < 3)
        {
            Debug.LogError("Invalid bulletPath format.");
            return null;
        }

        // Extract relevant parts from the path
        string directionInfo = pathComponents[1];  // "Top_left_right"
        string bulletType = pathComponents[2];     // "Bullet"

        // Initialize variables for rowOrColumn, from, and to
        string rowOrColumn = "";
        string from = "";
        string to = "";

        // Parse the directionInfo, assuming format "Top_left_right"
        string[] directions = directionInfo.Split('_');

        if (directions.Length == 3)
        {
            rowOrColumn = directions[0].ToLower(); // Convert to lowercase for consistency
            from = directions[1].ToLower();
            to = directions[2].ToLower();
        }
        else
        {
            Debug.LogError("Invalid direction format in bulletPath.");
            return null;
        }

        // Set the bullet type (convert to lowercase)
        string type = bulletType.ToLower();

        // Return the parsed data in a structured way
        return new ParsedBulletPath
        {
            rowOrColumn = rowOrColumn,
            from = from,
            to = to,
            type = type
        };
    }

    public void changeActiveBulletName(string activeBulletName) {
        this.activeBulletName = activeBulletName;
    }
    
    // Define a class to hold the parsed bullet path details
    private class ParsedBulletPath
    {
        public string rowOrColumn;
        public string from;
        public string to;
        public string type;
    }
}
