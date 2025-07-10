using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicLooper : MonoBehaviour
{
    public AudioClip musicClip;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}
