using UnityEngine;
using System.Collections;

public class Memory : MonoBehaviour
{
    // Declare the audio clips to be played
    public AudioClip audioClip1;
    public AudioClip audioClip2;
    
    // Reference to the AudioSource component
    private AudioSource audioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Wait for a short time before playing the first audio clip
        StartCoroutine(PlayAudioWithDelay(audioClip1, 1f)); // 2 seconds delay before playing the first clip

    }

    // Update is called once per frame
    void Update()
    {
        // Check if the AudioSource is not playing and if it had previously played audioClip1
        if (!audioSource.isPlaying && audioSource.clip == audioClip1)
        {
            // Play the second audio clip after the first one finishes
            if (audioClip2 != null)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Audio Clip 2 is not assigned!");
            }
        }
    }

    // Coroutine to play audio with a delay
    private IEnumerator PlayAudioWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = clip;
        audioSource.Play();
    }
}