using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // Assign a prefab in the Inspector
    public Sprite[] noteSprites;  // Assign an array of sprites in the Inspector
    public Transform radioTransform; // Assign the Radio object in the Inspector
    public float zigzagIntensity = 2f; // Customizable intensity of the zigzag effect

    private int noteCounter = 1;    // Start from note_1
    private List<GameObject> activeNotes = new List<GameObject>();

    void Start()
    {
        // Start the repeating function
        InvokeRepeating("SpawnNote", 0f, 1f);
    }

    void SpawnNote()
    {
        // Only spawn if there are less than 3 notes
        if (activeNotes.Count < 3)
        {
            // Instantiate the note and set its name
            GameObject newNote = Instantiate(notePrefab, radioTransform.position, Quaternion.identity);
            newNote.name = "note_" + noteCounter;
            newNote.transform.SetParent(radioTransform); // Set the Radio object as parent

            // Assign a random sprite to the note
            SpriteRenderer spriteRenderer = newNote.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && noteSprites.Length > 0)
            {
                spriteRenderer.sprite = noteSprites[Random.Range(0, noteSprites.Length)];
                StartCoroutine(FadeSprite(spriteRenderer, 0.5f, fadeIn: true)); // Start fading in
            }
            else
            {
                Debug.LogWarning("SpriteRenderer component is missing on the prefab or no sprites assigned.");
            }

            // Add the note to the list of active notes
            activeNotes.Add(newNote);

            // Start the zigzag movement coroutine
            StartCoroutine(ZigzagMovement(newNote, 3f));

            // Start the coroutine to destroy the note after 3 seconds
            StartCoroutine(DestroyNoteAfterTime(newNote, 3f));

            // Update the note counter, cycling back to 1 after 5
            noteCounter = noteCounter % 5 + 1;
        }
    }

    IEnumerator DestroyNoteAfterTime(GameObject note, float delay)
    {
        yield return new WaitForSeconds(delay - 0.5f); // Wait for 2.5 seconds before starting the fade-out

        // Fade out the sprite before destroying
        SpriteRenderer spriteRenderer = note.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            yield return StartCoroutine(FadeSprite(spriteRenderer, 0.5f, fadeIn: false));
        }

        // Remove the note from the active list and destroy it
        activeNotes.Remove(note);
        Destroy(note);
    }

    IEnumerator ZigzagMovement(GameObject note, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = note.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 5f, 0); // Adjust the vertical displacement if needed

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float yOffset = Mathf.Sin(t * Mathf.PI * 4) * zigzagIntensity; // Smooth zigzag effect with customizable intensity
            note.transform.position = Vector3.Lerp(startPosition, endPosition, t) + new Vector3(yOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is exactly at the endPosition
        note.transform.position = endPosition;
    }

    IEnumerator FadeSprite(SpriteRenderer spriteRenderer, float duration, bool fadeIn)
    {
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = fadeIn ? new Color(startColor.r, startColor.g, startColor.b, 1) : new Color(startColor.r, startColor.g, startColor.b, 0);

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startColor.a, endColor.a, elapsedTime / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final alpha is exactly at the endColor alpha
        spriteRenderer.color = endColor;
    }
}
