using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StarterKit;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yudiz.VRAwarenessExperience.Manager
{
    public enum SoundType
    {
        UIButtonClick,
        PillEatSound,
        DissolveSound,
        TickSound,
        WrongAttemptSound,
        BackgroundMusicSound,
        IntenseMusicSound,
        DizzinessSound,
        WinSound,
        LoseSound,
        AlarmSound,
        TransitionSound,
        WrongAttemptVoiceNormal,
        WrongAttemptVoicePill,

    }


    [Serializable]
    public class SoundData
    {
        public SoundType soundType;
        public bool isOneShot;
        public AudioClip audioClip;
        public bool isLooping;
    }


    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private List<SoundData> soundDatas = new List<SoundData>();
        [SerializeField] private float bgVolume = 1f;
        [SerializeField] private float oneShotVolume = 1f;
        [SerializeField] private float clockTickVolume = 1f;
        [SerializeField] private float pillEffectPitch = 0.5f;
        [SerializeField] private float pillEffectPitchChangeDuration = 1f;
        [SerializeField] private float normalPitch = 1f;
        private AudioSource backgroundMusicAudioSource;
        private AudioSource oneShotAudioSource;
        private AudioSource clockTickAudioSource;
        private List<AudioSource> audioSources = new List<AudioSource>();


        private float backgroundMusicChangeDuration = 4f;
        public override void OnAwake()
        {
            base.OnAwake();
            backgroundMusicAudioSource = gameObject.AddComponent<AudioSource>();
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();
            clockTickAudioSource = gameObject.AddComponent<AudioSource>();
            audioSources.Add(backgroundMusicAudioSource);
            audioSources.Add(oneShotAudioSource);
            audioSources.Add(clockTickAudioSource);
        }

        public void PlaySound(SoundType soundType)
        {
            SoundData soundData = soundDatas.Find(x => x.soundType == soundType);
            if (soundData != null)
            {
                if (soundData.isOneShot)
                {
                    PlayOneShotSound(soundData.audioClip);
                    Debug.Log("Playing One shot sound "+soundType);
                }
                else
                {
                    PlayBackgroundMusic(soundData.isLooping, soundData.audioClip);
                }
            }
        }


        private void PlayBackgroundMusic(bool isLoop, AudioClip audioClip)
        {
            backgroundMusicAudioSource.clip = audioClip;
            backgroundMusicAudioSource.loop = isLoop;
            backgroundMusicAudioSource.volume = bgVolume;
            backgroundMusicAudioSource.Play();
        }

        public void PlayClockTickSound()
        {
            AudioClip audioClip = soundDatas.Find(x => x.soundType == SoundType.TickSound).audioClip;
            clockTickAudioSource.clip = audioClip;
            clockTickAudioSource.loop = true;
            clockTickAudioSource.volume = clockTickVolume;
            clockTickAudioSource.Play();
        }

        public void StopClockTickSound()
        {
            clockTickAudioSource.Stop();
        }

        public async void StopBackgroundMusic()
        {
            if (backgroundMusicAudioSource.isPlaying)
            {
                await StopBackgroundMusic(backgroundMusicChangeDuration);
            }
        }

        private async Task StopBackgroundMusic(float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                float currentValue = Mathf.Lerp(backgroundMusicAudioSource.volume, 0f, elapsedTime / duration);
                backgroundMusicAudioSource.volume = currentValue;
                await Task.Yield();
            }
            backgroundMusicAudioSource.volume = 0f;
            backgroundMusicAudioSource.Stop();
        }


        private void PlayOneShotSound(AudioClip audioClip)
        {
            oneShotAudioSource.clip = audioClip;
            oneShotAudioSource.volume = oneShotVolume;
            oneShotAudioSource.PlayOneShot(audioClip);
        }

        public void StopOneShotSound()
        {
            oneShotAudioSource.Stop();
        }

        public async void ChangeSoundPitchToPillEffect()
        {
           await ChangeSoundPitchAsync(normalPitch, pillEffectPitch, pillEffectPitchChangeDuration);
        }

        public async void ResetSoundPitch()
        {
            await ChangeSoundPitchAsync(pillEffectPitch, normalPitch, pillEffectPitchChangeDuration);
        }

        

        public async Task ChangeSoundPitchAsync(float startPitch, float endPitch, float duration)
        {
            float elapsedTime = 0f;
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.pitch = startPitch;
            }
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                float currentValue = Mathf.Lerp(startPitch, endPitch, elapsedTime / duration);
                foreach (AudioSource audioSource in audioSources)
                {
                    audioSource.pitch = currentValue;
                }
                await Task.Yield();
            }
        }

        public AudioClip GetAudioClip(SoundType soundType)
        {
            return soundDatas.Find(x => x.soundType == soundType).audioClip;
        }

        public float GetClipLength(SoundType soundType)
        {
            SoundData soundData = soundDatas.Find(s => s.soundType == soundType);
            return soundData != null && soundData.audioClip != null ? soundData.audioClip.length : 0f;
        }

    }
}

