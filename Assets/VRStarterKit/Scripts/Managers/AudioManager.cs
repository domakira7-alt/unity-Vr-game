using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource globalAudioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        globalAudioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayGlobalAudio(AudioClip clip)
    {
        if(globalAudioSource.isPlaying)
        {
            globalAudioSource.Stop();
        }
        globalAudioSource.clip = clip;
        globalAudioSource.Play();
    }
    public void StopGlobalAudio(AudioClip clip)
    {
        if(globalAudioSource.isPlaying && globalAudioSource.clip == clip)
        {
            globalAudioSource.Stop();
        }
    }
}
