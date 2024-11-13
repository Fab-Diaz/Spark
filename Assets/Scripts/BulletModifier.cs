using UnityEngine;
using UnityEngine.UI;
using BulletTimerControllerNamespace;

public class BulletModifier : MonoBehaviour
{
    // An array of ButtonProperties
    public ButtonProperty[] buttonProperties;
    
    // Reference to the BulletCreator script
    public BulletCreator bulletCreator;

    // Reference to the BulletController script.
    public BulletTimerController bulletController;

    [System.Serializable]
    public class ButtonProperty
    {
        public Button button;
        public string bulletName;
        public Sprite bulletSprite;
    }

    private void Start()
    {
        // Initialize each button with its properties
        foreach (ButtonProperty prop in buttonProperties)
        {
            if (prop.button != null)
            {
                prop.button.onClick.AddListener(() => OnButtonClick(prop));
            }
            else
            {
                Debug.LogError("Button is missing!");
            }
        }
    }

    private void OnButtonClick(ButtonProperty prop)
    {
        // Check spawner validity
        CheckSpawnerValidity(prop.bulletName);

        // Change the images and dimensions of all objects under Buttons > Spawners
        ChangeAllSpawnerImagesAndDimensions(prop.bulletSprite, prop.button);

        // Call the method in BulletCreator
        if (bulletCreator != null)
        {
            bulletCreator.changeActiveBulletName(prop.bulletName);
        }
        else
        {
            Debug.LogError("BulletCreator reference is missing!");
        }
    }

    private void ChangeAllSpawnerImagesAndDimensions(Sprite newSprite, Button button)
    {
        // Find all GameObjects under the "Buttons > Spawners" hierarchy
        GameObject spawners = GameObject.Find("Buttons/Spawners");

        if (spawners != null)
        {
            // Get all Image components and RectTransforms under the "Spawners" GameObject
            Image[] images = spawners.GetComponentsInChildren<Image>();
            RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

            foreach (Image img in images)
            {
                img.sprite = newSprite;

                // Apply width, height, and scale from the button to the spawner's children
                RectTransform imgRectTransform = img.GetComponent<RectTransform>();
                if (imgRectTransform != null)
                {
                    imgRectTransform.sizeDelta = buttonRectTransform.sizeDelta;
                    imgRectTransform.localScale = buttonRectTransform.localScale;
                }
            }
        }
        else
        {
            Debug.LogError("Spawners GameObject not found!");
        }
    }

    private void CheckSpawnerValidity(string bulletName)
    {
        // Find all GameObjects under the "Buttons > Spawners" hierarchy
        GameObject spawners = GameObject.Find("Buttons/Spawners");

        if (spawners != null)
        {
            // Iterate through all spawners
            foreach (Transform spawner in spawners.transform)
            {
                // Construct the path to check
                string pathToCheck = $"Bullets/{formatBulletPath(spawner.name, "Spawn_")}/{bulletName}";

                // Check if the path is in the bulletController's bulletPaths
                spawner.gameObject.SetActive(IsPathInBulletPaths(pathToCheck));
            }
        }
        else
        {
            Debug.LogError("Spawners GameObject not found!");
        }
    }

    private bool IsPathInBulletPaths(string path)
    {
        if (bulletController != null && bulletController.bulletPaths != null)
        {
            foreach (string bulletPath in bulletController.bulletPaths)
            {
                if (bulletPath == path)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static string formatBulletPath(string input, string prefix)
    {
        string result = input.Substring(prefix.Length);

        return char.ToUpper(result[0]) + result.Substring(1);
    }
}
