using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource; // The AudioSource component to play the audio clips
    public List<AudioClip> audioClips; // List of music audio clips to play randomly
    public List<AudioClip> radioGuyAudios; // List of radio guy audio clips to play randomly
    public List<AudioClip> creepyAudios; // List of creepy audio clips to play randomly

    private Queue<int> recentlyPlayedMusicIndices = new Queue<int>(); // Queue to keep track of the last three played music indices
    private Queue<int> recentlyPlayedRadioGuyIndices = new Queue<int>(); // Queue to keep track of the last three played radio guy indices
    private Queue<int> recentlyPlayedCreepyIndices = new Queue<int>(); // Queue to keep track of the last three played creepy indices

    private List<int> availableMusicIndices; // List of currently available music indices
    private List<int> availableRadioGuyIndices; // List of currently available radio guy indices
    private List<int> availableCreepyIndices; // List of currently available creepy indices

    private bool playMusicNext = true; // Flag to alternate between music and radio guy

    // Frequency settings for creepy audios
    [Range(0, 1)]
    public float creepyAudioChance = 0.2f; // 20% chance by default to play creepy audio

    void Start()
    {
        if ((audioClips.Count == 0 && radioGuyAudios.Count == 0 && creepyAudios.Count == 0) || audioSource == null)
        {
            Debug.LogWarning("Please assign an AudioSource and at least one AudioClip in each category.");
            return;
        }

        // Initialize available indices lists
        availableMusicIndices = new List<int>();
        for (int i = 0; i < audioClips.Count; i++)
        {
            availableMusicIndices.Add(i);
        }

        availableRadioGuyIndices = new List<int>();
        for (int i = 0; i < radioGuyAudios.Count; i++)
        {
            availableRadioGuyIndices.Add(i);
        }

        availableCreepyIndices = new List<int>();
        for (int i = 0; i < creepyAudios.Count; i++)
        {
            availableCreepyIndices.Add(i);
        }

        // Start playing audios randomly
        StartCoroutine(PlayRandomAudio());
    }

    IEnumerator PlayRandomAudio()
    {
        while (true)
        {
            int randomIndex;
            AudioClip selectedClip;

            // Determine whether to play a creepy audio
            bool playCreepy = (Random.value < creepyAudioChance) && availableCreepyIndices.Count > 0;

            if (playCreepy)
            {
                // Play a random creepy clip
                randomIndex = availableCreepyIndices[Random.Range(0, availableCreepyIndices.Count)];
                selectedClip = creepyAudios[randomIndex];

                // Remove the selected index from available creepy indices and add it to the cooldown queue
                availableCreepyIndices.Remove(randomIndex);
                recentlyPlayedCreepyIndices.Enqueue(randomIndex);

                // Check if we need to release any creepy clips from the cooldown
                if (recentlyPlayedCreepyIndices.Count > 3)
                {
                    int releasedIndex = recentlyPlayedCreepyIndices.Dequeue();
                    availableCreepyIndices.Add(releasedIndex);
                }
            }
            else if (playMusicNext)
            {
                // Play a random music clip
                randomIndex = availableMusicIndices[Random.Range(0, availableMusicIndices.Count)];
                selectedClip = audioClips[randomIndex];

                // Remove the selected index from available music indices and add it to the cooldown queue
                availableMusicIndices.Remove(randomIndex);
                recentlyPlayedMusicIndices.Enqueue(randomIndex);

                // Check if we need to release any music clips from the cooldown
                if (recentlyPlayedMusicIndices.Count > 3)
                {
                    int releasedIndex = recentlyPlayedMusicIndices.Dequeue();
                    availableMusicIndices.Add(releasedIndex);
                }
            }
            else
            {
                // Play a random radio guy clip
                randomIndex = availableRadioGuyIndices[Random.Range(0, availableRadioGuyIndices.Count)];
                selectedClip = radioGuyAudios[randomIndex];

                // Remove the selected index from available radio guy indices and add it to the cooldown queue
                availableRadioGuyIndices.Remove(randomIndex);
                recentlyPlayedRadioGuyIndices.Enqueue(randomIndex);

                // Check if we need to release any radio guy clips from the cooldown
                if (recentlyPlayedRadioGuyIndices.Count > 3)
                {
                    int releasedIndex = recentlyPlayedRadioGuyIndices.Dequeue();
                    availableRadioGuyIndices.Add(releasedIndex);
                }
            }

            // Play the selected audio clip
            audioSource.clip = selectedClip;
            audioSource.Play();

            // Wait for the clip to finish playing
            yield return new WaitForSeconds(selectedClip.length);

            // Alternate between music and radio guy
            playMusicNext = !playMusicNext;
        }
    }
}
