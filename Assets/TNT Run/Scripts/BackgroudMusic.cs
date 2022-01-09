
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BackgroudMusic : UdonSharpBehaviour
{
    public AudioClip[] audioClips;
    AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate() {
        if (!audioSource.isPlaying) {
            int track = Random.Range(0, audioClips.Length);
            audioSource.clip = audioClips[track];
            audioSource.Play();
        }
    }
}
