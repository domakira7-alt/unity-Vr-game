using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace StarterKit.DialogueSystem
{
    /// <summary>
    /// Generic dialogue information class that can be used with different dialogue types
    /// </summary>
    [System.Serializable]
    public class DialogueInfo<T> where T : Enum
    {
        [Header("Dialogue Information")]
        public T dialogueType;
        
        [Header("Timeline Settings")]
        public double startTime;
        public double endTime;
        
        [Header("Dialogue Content")]
        public string characterName;
        public string dialogueText;
        
        [Header("Audio Settings")]
        public AudioClip audioClip;
        public float volume = 1.0f;
        
        [Header("Visual Settings")]
        public Sprite characterSprite;
        public Color textColor = Color.white;
        
        [Header("Animation Settings")]
        public AnimationClip animationClip;
        public string animationTrigger;
        
        [Header("Signal Settings")]
        public SignalAsset signalAsset;
        
        [Header("Additional Settings")]
        public bool isSkippable = true;
        public bool pauseTimeline = false;
        public float displayDuration = 3.0f;
        
        // Constructors
        public DialogueInfo()
        {
            dialogueType = default(T);
            startTime = 0.0;
            endTime = 0.0;
            characterName = "";
            dialogueText = "";
            textColor = Color.white;
            volume = 1.0f;
            isSkippable = true;
            pauseTimeline = false;
            displayDuration = 3.0f;
        }
        
        public DialogueInfo(T type, double start, double end)
        {
            dialogueType = type;
            startTime = start;
            endTime = end;
            characterName = "";
            dialogueText = "";
            textColor = Color.white;
            volume = 1.0f;
            isSkippable = true;
            pauseTimeline = false;
            displayDuration = 3.0f;
        }
        
        public DialogueInfo(T type, double start, double end, string character, string text)
        {
            dialogueType = type;
            startTime = start;
            endTime = end;
            characterName = character;
            dialogueText = text;
            textColor = Color.white;
            volume = 1.0f;
            isSkippable = true;
            pauseTimeline = false;
            displayDuration = 3.0f;
        }
        
        // Utility Methods
        public double GetDuration()
        {
            return endTime - startTime;
        }
        
        public bool IsActive(double currentTime)
        {
            return currentTime >= startTime && currentTime < endTime;
        }
        
        public bool IsWithinTolerance(double currentTime, double tolerance = 0.1)
        {
            return Math.Abs(currentTime - startTime) <= tolerance;
        }
        
        public bool HasAudio()
        {
            return audioClip != null;
        }
        
        public bool HasVisual()
        {
            return characterSprite != null;
        }
        
        public bool HasAnimation()
        {
            return animationClip != null || !string.IsNullOrEmpty(animationTrigger);
        }
        
        public bool HasSignal()
        {
            return signalAsset != null;
        }
        
        public void PlayAudio(AudioSource audioSource)
        {
            if (HasAudio() && audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.volume = volume;
                audioSource.Play();
            }
        }
        
        public void TriggerAnimation(Animator animator)
        {
            if (HasAnimation() && animator != null)
            {
                if (!string.IsNullOrEmpty(animationTrigger))
                {
                    animator.SetTrigger(animationTrigger);
                }
            }
        }
        
        public override string ToString()
        {
            return $"{dialogueType}: {startTime}s - {endTime}s ({characterName}: {dialogueText})";
        }
    }
    
}