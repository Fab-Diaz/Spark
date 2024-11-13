using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public int gridSizeX = 3; // Number of cells in the x direction
    public int gridSizeY = 3; // Number of cells in the y direction
    public float cellSize = 2f; // Distance between cells
    public float moveSpeed = 5f; // Speed multiplier for movement
    public float bounceDistance = 0.5f; // How far the player moves in the bounce-back
    public float bounceSpeed = 10f; // Speed of the bounce-back
    public MoveType moveType = MoveType.Direct; // Movement type

    private Vector2Int currentPosition; // Player's current grid position
    private Vector3 initialPosition; // Starting position in world space
    private bool isMoving = false; // To prevent input during movement
    private Queue<Vector2Int> inputQueue = new Queue<Vector2Int>(); // Queue to store inputs

    public enum MoveType
    {
        Direct,     // Linear movement
        EaseIn,     // Slow start, fast end
        EaseOut,    // Fast start, slow end
        EaseInOut   // Slow start, fast middle, slow end
    }

    private void Start()
    {
        // Calculate the middle position of the grid
        int startX = gridSizeX / 2;
        int startY = gridSizeY / 2;

        currentPosition = new Vector2Int(startX, startY);

        // Calculate the initial world position based on the grid position
        initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + new Vector3(currentPosition.x * cellSize, currentPosition.y * cellSize, 0);
        transform.position = targetPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            EnqueueMove(Vector2Int.up); // Enqueue up movement
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            EnqueueMove(Vector2Int.down); // Enqueue down movement
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            EnqueueMove(Vector2Int.left); // Enqueue left movement
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            EnqueueMove(Vector2Int.right); // Enqueue right movement
        }

        // If no current movement is happening and there are queued inputs, process the next input
        if (!isMoving && inputQueue.Count > 0)
        {
            Vector2Int direction = inputQueue.Dequeue();
            MovePlayer(direction);
        }
    }

    private void EnqueueMove(Vector2Int direction)
    {
        // Add the direction to the input queue
        inputQueue.Enqueue(direction);
    }

    private void MovePlayer(Vector2Int direction)
    {
        // Calculate new grid position
        Vector2Int newPosition = currentPosition + direction;

        // Check if the new position is within grid bounds
        if (newPosition.x >= 0 && newPosition.x < gridSizeX && newPosition.y >= 0 && newPosition.y < gridSizeY)
        {
            currentPosition = newPosition;
            // Calculate the new world position based on the grid position
            Vector3 targetPosition = initialPosition + new Vector3(currentPosition.x * cellSize, currentPosition.y * cellSize, 0);

            // Start the movement coroutine to move smoothly to the new position
            StartCoroutine(MoveToPosition(targetPosition));
        }
        else
        {
            // If outside bounds, calculate bounce-back target position
            Vector3 bounceTarget = transform.position + new Vector3(direction.x * bounceDistance, direction.y * bounceDistance, 0);
            // Start the bounce-back coroutine
            StartCoroutine(BounceBack(bounceTarget));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true; // Set moving flag to prevent other moves

        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            float t = Mathf.Clamp01(elapsedTime);

            // Apply selected easing method
            switch (moveType)
            {
                case MoveType.EaseIn:
                    t = Mathf.Pow(t, 3); // Ease in (cubic)
                    break;
                case MoveType.EaseOut:
                    t = Mathf.Pow(t - 1, 3) + 1; // Ease out (cubic)
                    break;
                case MoveType.EaseInOut:
                    t = t < 0.5f ? 4 * Mathf.Pow(t, 3) : 1 - Mathf.Pow(-2 * t + 2, 3) / 2; // Ease in-out (cubic)
                    break;
                case MoveType.Direct:
                default:
                    // Linear movement (direct) does not change 't'
                    break;
            }

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition; // Ensure player is exactly at the target position
        isMoving = false; // Reset moving flag
    }

    private IEnumerator BounceBack(Vector3 bounceTarget)
    {
        isMoving = true; // Set moving flag to prevent other moves

        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        // Move towards the bounce target
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(startPosition, bounceTarget, elapsedTime);
            yield return null;
        }

        // Move back to the start position
        elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(bounceTarget, startPosition, elapsedTime);
            yield return null;
        }

        transform.position = startPosition; // Ensure player is exactly at the start position
        isMoving = false; // Reset moving flag
    }
}
