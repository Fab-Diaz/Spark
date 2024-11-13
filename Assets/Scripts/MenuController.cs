using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject[] menuItems; // Array to hold the menu items (New Game, Options, Quit)
    private int activeIndex = 0; // Index of the currently selected menu item

    private Color inactiveColor = new Color(210f / 255f, 210f / 255f, 210f / 255f); // RGB(210, 210, 210)
    private Color activeColor = new Color(255f / 255f, 218f / 255f, 0f / 255f); // RGB(255, 218, 0)

    void Start()
    {
        UpdateMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectMenuItem();
        }
    }

    void MoveUp()
    {
        activeIndex = (activeIndex - 1 + menuItems.Length) % menuItems.Length;
        UpdateMenu();
    }

    void MoveDown()
    {
        activeIndex = (activeIndex + 1) % menuItems.Length;
        UpdateMenu();
    }

    void UpdateMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            Transform backgroundTransform = menuItems[i].transform.Find("Background");
            if (backgroundTransform != null)
            {
                SpriteRenderer backgroundSpriteRenderer = backgroundTransform.GetComponent<SpriteRenderer>();
                if (backgroundSpriteRenderer != null)
                {
                    if (i == activeIndex)
                    {
                        backgroundSpriteRenderer.color = activeColor; // Set active item background to yellow (RGB 255, 218, 0)
                    }
                    else
                    {
                        backgroundSpriteRenderer.color = inactiveColor; // Set inactive items background to light gray (RGB 210, 210, 210)
                    }
                }
            }
        }
    }

    void SelectMenuItem()
    {
        switch (activeIndex)
        {
            case 0:
                Debug.Log("New Game selected");
                // Load new game scene or start a new game
                break;
            case 1:
                Debug.Log("Options selected");
                // Open options menu
                break;
            case 2:
                Debug.Log("Quit selected");
                // Quit the application
                Application.Quit();
                break;
        }
    }
}
