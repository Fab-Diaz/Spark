using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BulletTimerControllerNamespace // Only add this if you're using namespaces
{
    [System.Serializable]
    public class TimedBulletTrigger
    {
        public string bulletPath; // Path to the bullet in the hierarchy
        public string triggerTimeString; // Time in "minutes:seconds:milliseconds" format
        [HideInInspector] public float triggerTime; // Converted time in seconds
        public bool triggered; // Indicates if the trigger has been activated
    }

    [System.Serializable]
    public class TimedBulletTriggerSimplified
    {
        public string rowOrColumn;
        public string from;
        public string to;
        public string type;
        public string triggerTimeString;
    }


    [System.Serializable]
    public class TimedBulletTriggerFile
    {
        public int song;
        public List<TimedBulletTriggerSimplified> triggers; // List of timed bullet triggers
    }

    public class BulletTimerController : MonoBehaviour {
        [HideInInspector]public List<string> bulletPaths = new List<string>();

        void GenerateBulletPaths() {
            string[] directions = {
                "First_up_down", "Second_up_down", "Third_up_down",
                "First_down_up", "Second_down_up", "Third_down_up",
                "Top_left_right", "Middle_left_right", "Bottom_left_right",
                "Top_right_left", "Middle_right_left", "Bottom_right_left"
            };

            string[] bulletTypes = { "Bullet", "Boomerang", "Arrow" };

            // Generate paths for Bullets and Boomerangs
            foreach (string type in bulletTypes) {
                foreach (string dir in directions) {
                    bulletPaths.Add($"Bullets/{dir}/{type}");
                }
            }

            // Add specific Arrow paths not following the pattern
            // bulletPaths.AddRange(new string[] {
            // });
        }

        // List of timed bullet triggers for this section
        public List<TimedBulletTrigger> bulletTriggers = new List<TimedBulletTrigger>();

        // Dictionary to store original bullet prefabs
        private Dictionary<string, GameObject> bulletPrefabs = new Dictionary<string, GameObject>();

        private MusicSlider musicSlider;

        // List to track active bullet copies
        private List<GameObject> activeBulletCopies = new List<GameObject>();

        public GameObject checkpointTag; // The checkpoint marker object with the animation
        public GameObject newLifeObject; // The object with the animator that will play the health-based animation

        public CameraBeatEffect CameraBeatEffect; // Reference to the CameraBeatEffect component

        private PlayerHealth playerHealth; // Reference to the PlayerHealth component
        private float newLifeAnimationDuration = 1.8f; // Duration of the new life animation in seconds

        void Start()
        {
            // Find the BulletTimerManager in the scene
            musicSlider = FindObjectOfType<MusicSlider>();
            if (musicSlider == null)
            {
                Debug.LogError("Music Slider not found in the scene.");
            }

            // Convert trigger times from string to float
            for (int i = 0; i < bulletTriggers.Count; i++)
            {
                TimedBulletTrigger trigger = bulletTriggers[i];
                trigger.triggerTime = ConvertTimeStringToFloat(trigger.triggerTimeString);
                trigger.triggered = false; // Initialize the triggered flag
                bulletTriggers[i] = trigger;
            }

            GenerateBulletPaths();

            // Load all bullet prefabs based on the provided paths
            foreach (var path in bulletPaths)
            {
                Transform bulletTransform = transform.Find(path);
                if (bulletTransform != null)
                {
                    bulletPrefabs[path] = bulletTransform.gameObject;
                }
                else
                {
                    Debug.LogError("Bullet path not found: " + path);
                }
            }
            
            // Find the PlayerHealth component on the "Spark" object
            GameObject sparkObject = GameObject.Find("Spark");
            if (sparkObject != null)
            {
                playerHealth = sparkObject.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    Debug.LogError("PlayerHealth component not found on Spark object.");
                }
            }
            else
            {
                Debug.LogError("Spark object not found in the scene.");
            }
        }
        

        void Update()
        {
            // Check if any bullet should be triggered
            for (int i = bulletTriggers.Count - 1; i >= 0; i--)
            {
                bool isCheckpoint = bulletTriggers[i].bulletPath == "checkpoint";
                bool isBeat = bulletTriggers[i].bulletPath == "beat";
                var trigger = bulletTriggers[i];
                if (musicSlider.audioSource.time >= trigger.triggerTime && musicSlider.audioSource.time <= (trigger.triggerTime + 1) && !trigger.triggered)
                {
                    if (isCheckpoint) {
                        TriggerCheckpointTagAnimation();
                        TriggerNewLifeAnimation();
                    }
                    else if (isBeat) {
                        CameraBeatEffect.TriggerBeat();
                    }
                    else {
                        // Instantiate a copy of the bullet
                        GameObject originalBullet = bulletPrefabs[bulletTriggers[i].bulletPath];
                        GameObject bulletCopy = Instantiate(originalBullet, originalBullet.transform.position, originalBullet.transform.rotation);

                        SpriteRenderer bulletSprite = bulletCopy.GetComponent<SpriteRenderer>();
                        BoxCollider2D bulletCollider = bulletCopy.GetComponent<BoxCollider2D>();

                        bulletCopy.transform.SetParent(originalBullet.transform.parent, worldPositionStays: true);
                        bulletSprite.enabled = true;
                        bulletCollider.enabled = true;
                        
                        // Trigger the animation on the copy
                        Animator bulletAnimator = bulletCopy.GetComponent<Animator>();

                        BulletEntity bulletEntity = bulletCopy.GetComponent<BulletEntity>();
                        
                        if (bulletEntity != null)
                        {
                            // set public bulletPath variable in BulletEntity
                            bulletEntity.bulletPath = bulletTriggers[i].bulletPath;
                            bulletEntity.triggerTime = bulletTriggers[i].triggerTime;
                        }

                        if (bulletAnimator != null)
                        {
                            bulletAnimator.SetTrigger("Attack");

                            // Track the active bullet copy
                            activeBulletCopies.Add(bulletCopy);

                            // Start coroutine to destroy the object after animation finishes
                            StartCoroutine(DestroyAfterAnimation(bulletAnimator, bulletCopy));
                        }
                        else
                        {
                            Debug.LogError("No Animator found on " + bulletTriggers[i].bulletPath);
                        }
                    }
                    trigger.triggered = true;
                    
                    bulletTriggers[i] = trigger; // Update the trigger in the list
                }
            }
        }

        public void RemoveBullet(string bulletPath, float triggerTime)
        {
            // Iterate through the bulletTriggers list from the end to the beginning
            // to avoid issues with indices changing when removing elements.
            for (int i = bulletTriggers.Count - 1; i >= 0; i--)
            {
                var trigger = bulletTriggers[i];
                if (trigger.bulletPath == bulletPath && Mathf.Approximately(trigger.triggerTime, triggerTime))
                {
                    // Remove the matching trigger
                    bulletTriggers.RemoveAt(i);
                    return; // Exit after removing the first match
                }
            }

            Debug.LogWarning("Bullet trigger not found: " + bulletPath + " at time: " + triggerTime);
        }

        // Coroutine to destroy the object after its animation is complete
        private IEnumerator DestroyAfterAnimation(Animator animator, GameObject bulletCopy)
        {
            if (animator != null && bulletCopy != null) {
                // Wait until the animation is no longer playing
                while (animator != null && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                {
                    yield return null;
                }

                // Destroy the object
                Destroy(bulletCopy);

                // Remove the bullet from the active list
                activeBulletCopies.Remove(bulletCopy);
            }
        }

        // Convert time string in "minutes:seconds:milliseconds" format to float in seconds
        private float ConvertTimeStringToFloat(string timeString)
        {
            string[] timeParts = timeString.Split(':');
            if (timeParts.Length == 3)
            {
                int minutes = int.Parse(timeParts[0]);
                int seconds = int.Parse(timeParts[1]);
                int milliseconds = int.Parse(timeParts[2]);
                return minutes * 60f + seconds + milliseconds / 1000f;
            }
            else
            {
                Debug.LogError("Invalid time format: " + timeString);
                return 0f;
            }
        }

        public void resetToSpecificTime(float time)
        {
            musicSlider.audioSource.time = time;
            
            // Destroy all active bullet copies
            foreach (var bulletCopy in activeBulletCopies)
            {
                Destroy(bulletCopy);
            }

            // Clear the list of active bullet copies
            activeBulletCopies.Clear();

            // Reset all triggers to not triggered
            for (int i = 0; i < bulletTriggers.Count; i++)
            {
                TimedBulletTrigger trigger = bulletTriggers[i];
                trigger.triggered = false;
                bulletTriggers[i] = trigger;
            }
        }

        // Method to reset the state to the last checkpoint and destroy all active bullets
        public void resetToLastCheckpoint()
        {
            // Initialize a variable to hold the latest trigger time
            float lastCheckpointTime = 0f;

            // Iterate through the bulletTriggers array to find the last triggered checkpoint
            foreach (var trigger in bulletTriggers)
            {
                // Check if the trigger is a checkpoint and has been triggered
                if (trigger.bulletPath == "checkpoint" && trigger.triggered)
                {
                    // Update the lastCheckpointTime to the current trigger's triggerTime
                    lastCheckpointTime = trigger.triggerTime;
                }
            }

            resetToSpecificTime(lastCheckpointTime);
        }

        private void TriggerCheckpointTagAnimation()
        {
            if (checkpointTag != null)
            {
                Animator checkpointAnimator = checkpointTag.GetComponent<Animator>();
                if (checkpointAnimator != null)
                {
                    checkpointAnimator.SetTrigger("Enabled");
                }
                else
                {
                    Debug.LogError("No Animator component found on checkpoint marker.");
                }
            }
            else
            {
                Debug.LogError("Checkpoint marker object is not assigned.");
            }
        }

        private void TriggerNewLifeAnimation()
        {
            if (playerHealth != null && newLifeObject != null)
            {
                int currentHealth = playerHealth.GetCurrentHealth();

                if (currentHealth >= 1 && currentHealth <= 4)
                {
                    Animator newLifeAnimator = newLifeObject.GetComponent<Animator>();
                    if (newLifeAnimator != null)
                    {
                        string triggerName = "New_life_" + (currentHealth + 1); // e.g., "New_life_2" for currentHealth = 1
                        newLifeAnimator.SetTrigger(triggerName);

                        // Start coroutine to wait for the animation to finish and then heal the player
                        StartCoroutine(WaitAndHeal());
                    }
                    else
                    {
                        Debug.LogError("No Animator component found on newLifeObject.");
                    }
                }
            }
            else
            {
                Debug.LogError("PlayerHealth or newLifeObject is not properly assigned.");
            }
        }

        private IEnumerator WaitAndHeal()
        {
            // Wait for the duration of the new life animation
            yield return new WaitForSeconds(newLifeAnimationDuration);

            // Heal the player
            if (playerHealth != null)
            {
                playerHealth.Heal(1);
            }
            else
            {
                Debug.LogError("PlayerHealth component is not assigned.");
            }
        }

        // Optional: Method to load bullet triggers from a JSON file
        public void LoadBulletTriggersFromJson(string output)
        {
            // Deserialize from JSON
            TimedBulletTriggerFile file = JsonUtility.FromJson<TimedBulletTriggerFile>(output);

            // Clear the current bulletTriggers list before adding new items
            bulletTriggers.Clear();

            // Loop through the deserialized list and create new TimedBulletTrigger objects
            for (int i = 0; i < file.triggers.Count; i++)
            {
                // Get the original rowOrColumn string
                string rowOrColumn = file.triggers[i].rowOrColumn;
                string type = file.triggers[i].type;
                TimedBulletTrigger newTrigger;

                if (type == "checkpoint" || type == "beat")
                {
                    // Add the trigger directly to the list
                    newTrigger = new TimedBulletTrigger
                    {
                        bulletPath = type,
                        triggerTimeString = file.triggers[i].triggerTimeString,
                        triggerTime = ConvertTimeStringToFloat(file.triggers[i].triggerTimeString),
                        triggered = false
                    };
                } else {
                    // Capitalize the first character and concatenate with the rest of the string
                    string capitalizedRowOrColumn = rowOrColumn.Length > 0 
                        ? char.ToUpper(rowOrColumn[0]) + rowOrColumn.Substring(1) 
                        : rowOrColumn;

                    string capitalizedType = type.Length > 0 
                        ? char.ToUpper(type[0]) + type.Substring(1) 
                        : type;

                    // Use the capitalized version in the formatted string
                    string directionInfo = $"{capitalizedRowOrColumn}_{file.triggers[i].from}_{file.triggers[i].to}";

                    // Combine everything to generate the bulletPath
                    string bulletPath = $"Bullets/{directionInfo}/{capitalizedType}";

                    // Create a new TimedBulletTrigger instance and populate its fields
                    newTrigger = new TimedBulletTrigger
                    {
                        bulletPath = bulletPath,
                        triggerTimeString = file.triggers[i].triggerTimeString,
                        triggerTime = ConvertTimeStringToFloat(file.triggers[i].triggerTimeString),
                        triggered = false
                    };
                }

                // Add the new trigger to the bulletTriggers list
                bulletTriggers.Add(newTrigger);
            }
        }
    }
}