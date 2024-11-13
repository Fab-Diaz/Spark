using System.Collections;
using UnityEngine;
using Kino;
using BulletTimerControllerNamespace;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invincibilityDuration = 2f; // Time player is invincible after being hit
    public float blinkInterval = 0.1f; // How often the player blinks during invincibility
    public SpriteRenderer playerSprite;

    public GameObject[] hearts;
    public BulletTimerController bulletTimerController; // Reference to the bulletTimerController.
    public AudioClip gameOverClip; // Reference to the Game Over audio clip
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioSource playerHit; // Reference to the AudioSource component
    public AudioSource battleMusicSource; // Reference to the music component in Battle_tutorial

    public bool invincible = false; // New invincible parameter

    private int currentHealth;
    private bool isInvincible = false;
    private DigitalGlitch digitalGlitch; // Reference to the Digital Glitch script on the camera
    private AnalogGlitch analogGlitch; // Reference to the Digital Glitch script on the camera

    void Start()
    {
        currentHealth = maxHealth;
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
        }
        digitalGlitch = Camera.main.GetComponent<DigitalGlitch>(); // Assuming the script is on the main camera
        analogGlitch = Camera.main.GetComponent<AnalogGlitch>(); // Assuming the script is on the main camera
        UpdateHeartsDisplay();

        SetPlayerOpacity();
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible && !invincible)
        {
            currentHealth -= damage;
            UpdateHeartsDisplay();

            if (currentHealth <= 0)
            {
                StartCoroutine(InvincibilityCoroutine());
                StartCoroutine(DeathSequence());
            }
            else
            {
                playerHit.Play();
                StartCoroutine(InvincibilityCoroutine());
                StartCoroutine(ChangePitchCoroutine());
            }
        }
    }

    private IEnumerator ChangePitchCoroutine()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        float initialPitch = 1f;
        float targetPitch = 0.85f;

        // Lower pitch from 1 to 0.85 over 0.3 seconds
        while (elapsed < duration)
        {
            battleMusicSource.pitch = Mathf.Lerp(initialPitch, targetPitch, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure pitch is exactly at 0.85 at the end of the duration
        battleMusicSource.pitch = targetPitch;

        // Raise pitch from 0.85 to 1 over 0.15 seconds
        elapsed = 0f;
        duration = 0.15f;
        initialPitch = 0.85f;
        targetPitch = 1f;

        while (elapsed < duration)
        {
            battleMusicSource.pitch = Mathf.Lerp(initialPitch, targetPitch, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure pitch is exactly at 1 at the end of the duration
        battleMusicSource.pitch = targetPitch;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            // Toggle sprite visibility to create blink effect
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // Ensure sprite is enabled at the end of the invincibility period
        playerSprite.enabled = true;
        isInvincible = false;
    }

    public void SetInvincible(bool state)
    {
        invincible = state;
        currentHealth = 3;
        UpdateHeartsDisplay();
        SetPlayerOpacity();
    }

    private void SetPlayerOpacity()
    {
        Color spriteColor = playerSprite.color;
        spriteColor.a = invincible ? 0.5f : 1f; // Set opacity to 50% if invincible, otherwise 100%
        playerSprite.color = spriteColor;
    }

    private void resetToLastCheckpoint()
    {
        if (bulletTimerController != null)
        {
            bulletTimerController.resetToLastCheckpoint();
            currentHealth = maxHealth;
            UpdateHeartsDisplay(); // Update the hearts display when healing
        }
        else
        {
            Debug.LogError("bulletTimerController reference is not set in PlayerHealth.");
        }
    }

    private IEnumerator DeathSequence()
    {
        // Play the Game Over audio
        if (audioSource != null && gameOverClip != null)
        {
            audioSource.PlayOneShot(gameOverClip);
        }

        // Gradually increase glitch effect parameters from 0 to 0.7 over 1.5 seconds and reduce music pitch
        float elapsed = 0f;
        while (elapsed < 1.7f)
        {
            float tGlitch = Mathf.Clamp01(elapsed / 1.5f) * 0.7f; // Go from 0 to 0.7
            float tPitch = Mathf.Lerp(1f, 0.4f, Mathf.Clamp01(elapsed / 1.7f)); // Go from 1 to 0.4 over 1.7 seconds
            if (digitalGlitch != null)
            {
                digitalGlitch.intensity = tGlitch;
            }
            if (analogGlitch != null)
            {
                analogGlitch.scanLineJitter = tGlitch;
                analogGlitch.verticalJump = tGlitch;
                analogGlitch.horizontalShake = tGlitch;
                analogGlitch.colorDrift = tGlitch;
            }
            if (battleMusicSource != null)
            {
                battleMusicSource.pitch = tPitch;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Gradually decrease glitch effect parameters back to 0 over 0.3 seconds and reset music pitch
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            float tGlitch = Mathf.Clamp01(1f - (elapsed / 0.3f)) * 0.7f; // Go from 0.7 to 0
            float tPitch = Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(elapsed / 0.3f)); // Go from 0.4 to 1 over 0.3 seconds
            if (digitalGlitch != null)
            {
                digitalGlitch.intensity = tGlitch;
            }
            if (analogGlitch != null)
            {
                analogGlitch.scanLineJitter = tGlitch;
                analogGlitch.verticalJump = tGlitch;
                analogGlitch.horizontalShake = tGlitch;
                analogGlitch.colorDrift = tGlitch;
            }
            if (battleMusicSource != null)
            {
                battleMusicSource.pitch = tPitch;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        this.resetToLastCheckpoint();

        // Optionally, you can reset parameters to 0 explicitly at the end
        if (digitalGlitch != null)
        {
            digitalGlitch.intensity = 0f;
        }
        if (analogGlitch != null)
        {
            analogGlitch.scanLineJitter = 0f;
            analogGlitch.verticalJump = 0f;
            analogGlitch.horizontalShake = 0f;
            analogGlitch.colorDrift = 0f;
        }
        if (battleMusicSource != null)
        {
            battleMusicSource.pitch = 1f;
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        UpdateHeartsDisplay(); // Update the hearts display when healing
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateHeartsDisplay()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].SetActive(true); // Show heart
            }
            else
            {
                hearts[i].SetActive(false); // Hide heart
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);
        }
    }
}
