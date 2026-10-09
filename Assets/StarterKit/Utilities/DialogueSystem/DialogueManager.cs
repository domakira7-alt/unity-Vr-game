using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace StarterKit.DialogueSystem
{
    /// <summary>
    /// Generic dialogue manager that handles dialogue sequences and timeline integration
    /// </summary>
    [System.Serializable]
    public class DialogueManager<T> where T : Enum
    {
        [Header("Timeline Settings")]
        [SerializeField] private TimelineAsset timelineAsset;
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private SignalReceiver signalReceiver;
        [SerializeField] private SignalAsset signalAsset;

        [Header("Dialogue Configuration")]
        [SerializeField] private List<T> dialogueSequence = new List<T>();
        [SerializeField] private List<DialogueInfo<T>> allDialogues = new List<DialogueInfo<T>>();

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool autoPlayAudio = true;

        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool autoTriggerAnimations = true;

        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = true;
        
        [Header("Skip Settings")]
        [SerializeField] private bool enableSkip = true;
        [SerializeField] private UnityEngine.UI.Button skipButton;

        // Runtime data
        private Dictionary<T, DialogueInfo<T>> dialogueByType = new Dictionary<T, DialogueInfo<T>>();
        private int currentDialogueIndex = 0;
        private T currentDialogue = default(T);
        private bool isInitialized = false;
        private bool isCurrentDialogueActive = true;

        // Events
        [HideInInspector]public UnityEvent<T> OnDialogueStarted = new UnityEvent<T>();
        [HideInInspector]public UnityEvent<T> OnDialogueEnded = new UnityEvent<T>();
        [HideInInspector]public UnityEvent<T,T> OnDialogueChanged = new UnityEvent<T,T>();
        [HideInInspector]public UnityEvent<T> OnDialogueSkipped = new UnityEvent<T>();
        [HideInInspector]public UnityEvent OnSequenceCompleted = new UnityEvent();

        // Properties
        public TimelineAsset TimelineAsset => timelineAsset;
        public PlayableDirector PlayableDirector => playableDirector;
        public SignalReceiver SignalReceiver => signalReceiver;
        public SignalAsset SignalAsset => signalAsset;
        public List<T> DialogueSequence => new List<T>(dialogueSequence);
        public List<DialogueInfo<T>> AllDialogues => new List<DialogueInfo<T>>(allDialogues);
        public Dictionary<T, DialogueInfo<T>> DialogueByType => new Dictionary<T, DialogueInfo<T>>(dialogueByType);
        public T CurrentDialogue => currentDialogue;
        public int CurrentDialogueIndex => currentDialogueIndex;
        public bool IsInitialized => isInitialized;

        // Initialization
        public void Initialize(TimelineAsset timeline, PlayableDirector director, SignalReceiver receiver = null)
        {
            timelineAsset = timeline;
            playableDirector = director;
            signalReceiver = receiver;

            Initialize();


        }
        public void Initialize()
        {
            if (playableDirector != null)
            {
                playableDirector.playableAsset = timelineAsset;
            }

            LoadDialogueSignals();
            SetupSignalReceiver();
            monitoringMonoBehaviour = playableDirector.GetComponent<MonoBehaviour>();

            isInitialized = true;
            if (debugMode)
                Debug.Log($"DialogueManager initialized with {allDialogues.Count} dialogues");
        }

        // Signal Management
        private void LoadDialogueSignals()
        {
            allDialogues.Clear();
            dialogueByType.Clear();

            if (timelineAsset == null) return;

            List<double> signalTimes = new List<double>();

            // Get all signal times from timeline
            var markerTracks = timelineAsset.GetOutputTracks().OfType<MarkerTrack>();
            if (markerTracks == null) return;

            foreach (MarkerTrack track in markerTracks)
            {
                foreach (IMarker marker in track.GetMarkers())
                {
                    if (marker is SignalEmitter signalEmitter)
                    {
                        SignalAsset signal = signalEmitter.asset;
                        if (signal != null && (signalAsset == null || signal.name.ToLower() == signalAsset.name.ToLower()))
                        {
                            signalTimes.Add(marker.time);
                            if (debugMode)
                                Debug.Log($"Found signal: {signal.name} at {marker.time}s");
                        }
                    }
                }
            }

            signalTimes.Sort();

            // Assign dialogue times based on signal pairs
            for (int i = 0; i < dialogueSequence.Count && i < signalTimes.Count; i++)
            {
                T dialogueType = dialogueSequence[i];
                double startTime = signalTimes[i];
                double endTime = (i + 1 < signalTimes.Count) ? signalTimes[i + 1] : startTime + 3.0;

                DialogueInfo<T> dialogueInfo = new DialogueInfo<T>(dialogueType, startTime, endTime);
                allDialogues.Add(dialogueInfo);
                dialogueByType[dialogueType] = dialogueInfo;

                if (debugMode)
                    Debug.Log($"Loaded dialogue: {dialogueType} from {startTime}s to {endTime}s");
            }
        }

        private void SetupSignalReceiver()
        {
            if (signalReceiver == null || signalAsset == null) return;

            if (debugMode)
                Debug.Log($"Signal receiver setup with asset: {signalAsset.name}");
                
            // Setup skip button if assigned
            SetupSkipButton();
        }
        
        private void SetupSkipButton()
        {
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(SkipCurrentDialogue);
                
                if (debugMode)
                    Debug.Log("Skip button setup completed");
            }
        }

        // Timeline Control
        public void PlayTimeline()
        {
            if (playableDirector != null)
            {
                playableDirector.Play();
                StartMonitoring();
                if (debugMode)
                    Debug.Log("Timeline started playing");
            }
        }
        public void StartTimeline()
        {
            if (playableDirector != null)
            {
                playableDirector.Play();
                isCurrentDialogueActive = true;
                currentDialogueIndex = 0;
                currentDialogue = dialogueSequence[0];
                OnDialogueStarted?.Invoke(currentDialogue);
                StartMonitoring();
                if (debugMode)
                    Debug.Log("Timeline started playing");
            }
        }

        public void PauseTimeline()
        {
            if (playableDirector != null)
            {
                playableDirector.Pause();
                if (debugMode)
                    Debug.Log("Timeline paused");
            }
        }

        public void ResumeTimeline()
        {
            if (playableDirector != null)
            {
                playableDirector.Resume();
                // Resume monitoring since it stops when the director is paused
                StartMonitoring();
                if (debugMode)
                    Debug.Log("Timeline resumed");
            }
        }

        public void StopTimeline()
        {
            if (playableDirector != null)
            {
                EndCurrentDialogue();
                playableDirector.Stop();
                StopMonitoring();
                if (debugMode)
                    Debug.Log("Timeline stopped");
            }
        }

        public void JumpToTime(double time)
        {
            if (playableDirector != null)
            {
                playableDirector.time = time;
                playableDirector.Evaluate();
                if (debugMode)
                    Debug.Log($"Jumped to time: {time} seconds");
            }
        }

        public void JumpToDialogue(T dialogueType)
        {
            if (dialogueByType.ContainsKey(dialogueType))
            {
                DialogueInfo<T> dialogue = dialogueByType[dialogueType];
                JumpToTime(dialogue.startTime);
                if (debugMode)
                    Debug.Log($"Jumped to dialogue: {dialogueType}");
            }
        }

        // Dialogue Control
        public void TriggerDialogue(T dialogueType)
        {
            // Keep public API, but do not end previous by default
            SwitchToDialogue(dialogueType, false);
        }

        /// <summary>
        /// Ends the previous dialogue (optional) then starts the new one in the correct order.
        /// </summary>
        private void SwitchToDialogue(T newDialogueType, bool endPrevious)
        {
            if (!dialogueByType.ContainsKey(newDialogueType)) return;

            T oldDialogue = currentDialogue;

            if (endPrevious && isCurrentDialogueActive )
            {
                // End the currently active dialogue before starting the new one
                OnDialogueEnded?.Invoke(oldDialogue);
                if (debugMode)
                    Debug.Log($"Dialogue ended (switch): {oldDialogue}");
                isCurrentDialogueActive = false;
            }

            DialogueInfo<T> dialogue = dialogueByType[newDialogueType];
            OnDialogueChanged?.Invoke(oldDialogue, newDialogueType);
            currentDialogue = newDialogueType;

            // Play audio if available
            if (autoPlayAudio && dialogue.HasAudio() && audioSource != null)
            {
                dialogue.PlayAudio(audioSource);
            }

            // Trigger animation if available
            if (autoTriggerAnimations && dialogue.HasAnimation() && animator != null)
            {
                dialogue.TriggerAnimation(animator);
            }

            isCurrentDialogueActive = true;
            OnDialogueStarted?.Invoke(newDialogueType);
        }

        /// <summary>
        /// Ends the current dialogue once and only once.
        /// </summary>
        private void EndCurrentDialogue()
        {
            if (!isCurrentDialogueActive) return;
            if (currentDialogue.Equals(default(T))) return;

            OnDialogueEnded?.Invoke(currentDialogue);
            if (debugMode)
                Debug.Log($"Dialogue ended: {currentDialogue}");

            isCurrentDialogueActive = false;
        }

        // Runtime data for stopping functionality
        private bool shouldStopAtEnd = false;
        private double stopAtTime = 0.0;
        private float stopAfterDuration = 0.0f;
        private bool isDurationStopping = false;
        
        // Monitoring functionality
        private bool isMonitoring = false;
        private MonoBehaviour monitoringMonoBehaviour = null;

        /// <summary>
        /// Play a specific dialogue and stop after it completes
        /// </summary>
        /// <param name="dialogueType">The dialogue type to play</param>
        /// <param name="stopAfterCompletion">Whether to stop the timeline after the dialogue ends</param>
        public void PlaySpecificDialogue(T dialogueType, bool stopAfterCompletion = true)
        {
            if (!dialogueByType.ContainsKey(dialogueType)) 
            {
                if (debugMode)
                    Debug.LogWarning($"Dialogue type {dialogueType} not found!");
                return;
            }

            DialogueInfo<T> dialogue = dialogueByType[dialogueType];
            
            // Jump to the start of the dialogue
            JumpToTime(dialogue.startTime);
            
            // Switch to the dialogue (end previous if active)
            SwitchToDialogue(dialogueType, endPrevious: isCurrentDialogueActive);
            
            // Setup stopping if requested
            if (stopAfterCompletion)
            {
                shouldStopAtEnd = true;
                stopAtTime = dialogue.endTime;
                if (debugMode)
                    Debug.Log($"⏹️ Will stop at dialogue end: {stopAtTime}s");
            }
            else
            {
                shouldStopAtEnd = false;
            }
            
            // Start playing the timeline
            PlayTimeline();
            
            if (debugMode)
                Debug.Log($"🎬 Playing specific dialogue: {dialogueType} (stop after: {stopAfterCompletion})");
        }

        /// <summary>
        /// Play from a specific dialogue onwards (continues the sequence)
        /// </summary>
        /// <param name="dialogueType">The dialogue type to start from</param>
        public void PlayFromDialogue(T dialogueType)
        {
            if (!dialogueByType.ContainsKey(dialogueType)) 
            {
                if (debugMode)
                    Debug.LogWarning($"Dialogue type {dialogueType} not found!");
                return;
            }

            DialogueInfo<T> dialogue = dialogueByType[dialogueType];
            
            // Jump to the start of the dialogue
            JumpToTime(dialogue.startTime);
            
            // Switch to the dialogue (end previous if active)
            SwitchToDialogue(dialogueType, endPrevious: isCurrentDialogueActive);
            
            // Reset stopping flags (continue through sequence)
            shouldStopAtEnd = false;
            isDurationStopping = false;
            
            // Start playing the timeline (will continue through the sequence)
            PlayTimeline();
            
            if (debugMode)
                Debug.Log($"🎬 Playing from dialogue: {dialogueType}");
        }

        /// <summary>
        /// Play only a specific dialogue without timeline (just trigger the dialogue effects)
        /// </summary>
        /// <param name="dialogueType">The dialogue type to play</param>
        public void PlayDialogueOnly(T dialogueType)
        {
            if (!dialogueByType.ContainsKey(dialogueType)) 
            {
                if (debugMode)
                    Debug.LogWarning($"Dialogue type {dialogueType} not found!");
                return;
            }

            // Just trigger the dialogue without timeline control
            shouldStopAtEnd = true;
            SwitchToDialogue(dialogueType, endPrevious: isCurrentDialogueActive);
            
            if (debugMode)
                Debug.Log($"🎬 Playing dialogue only: {dialogueType}");
        }

        /// <summary>
        /// Play a dialogue and automatically stop after a specified duration
        /// </summary>
        /// <param name="dialogueType">The dialogue type to play</param>
        /// <param name="duration">How long to play before stopping (in seconds)</param>
        public void PlayDialogueWithDuration(T dialogueType, float duration)
        {
            if (!dialogueByType.ContainsKey(dialogueType)) 
            {
                if (debugMode)
                    Debug.LogWarning($"Dialogue type {dialogueType} not found!");
                return;
            }

            DialogueInfo<T> dialogue = dialogueByType[dialogueType];
            
            // Jump to the start of the dialogue
            JumpToTime(dialogue.startTime);
            
            // Switch to the dialogue (end previous if active)
            SwitchToDialogue(dialogueType, endPrevious: isCurrentDialogueActive);
            
            // Setup duration stopping
            isDurationStopping = true;
            stopAfterDuration = duration;
            shouldStopAtEnd = false;
            
            // Start playing the timeline
            PlayTimeline();
            
            if (debugMode)
                Debug.Log($"🎬 Playing dialogue with duration: {dialogueType} for {duration}s");
        }

        /// <summary>
        /// Check if timeline should be stopped (call this in Update method of MonoBehaviour)
        /// </summary>
        public void CheckForStopConditions()
        {
            if (playableDirector == null) return;

            // Check for end time stopping
            if (shouldStopAtEnd && playableDirector.time >= stopAtTime)
            {
                StopTimeline();
                shouldStopAtEnd = false;
                if (debugMode)
                    Debug.Log($"⏹️ Stopped timeline at dialogue end: {stopAtTime}s");
            }

            // Check for duration stopping
            if (isDurationStopping)
            {
                stopAfterDuration -= Time.deltaTime;
                if (stopAfterDuration <= 0)
                {
                    StopTimeline();
                    isDurationStopping = false;
                    if (debugMode)
                        Debug.Log($"⏹️ Stopped timeline after duration");
                }
            }
        }
        
        #region Monitoring System
        
        /// <summary>
        /// Start monitoring the timeline progress
        /// </summary>
        private void StartMonitoring()
        {
            if (isMonitoring) return;
            
            isMonitoring = true;
            monitoringMonoBehaviour.StartCoroutine(MonitorDialogueProgress());
            
            if (debugMode)
                Debug.Log("🔍 Dialogue monitoring started");
        }
        
        /// <summary>
        /// Stop monitoring the timeline progress
        /// </summary>
        private void StopMonitoring()
        {
            isMonitoring = false;
            // If timeline naturally finished, mark sequence complete and end any active dialogue
            if (playableDirector != null)
            {
                // Consider near-end as completion
                if (playableDirector.duration > 0 && playableDirector.time >= playableDirector.duration - 0.01f)
                {
                    EndCurrentDialogue();
                    OnSequenceCompleted?.Invoke();
                    if (debugMode)
                        Debug.Log("✅ Dialogue sequence completed");
                }
            }
            if (debugMode)
                Debug.Log("🔍 Dialogue monitoring stopped");
        }
        
        /// <summary>
        /// Monitor dialogue progress coroutine
        /// </summary>
        private IEnumerator MonitorDialogueProgress()
        {
            while (playableDirector != null && 
                   playableDirector.state == PlayState.Playing && 
                   isMonitoring)
            {
                double currentTime = playableDirector.time;
                UpdateCurrentDialogue(currentTime);
                
                // Check for stop conditions
                CheckForStopConditions();
                
                yield return null;
            }
            
            StopMonitoring();
        }
        
        #endregion

        public void UpdateCurrentDialogue(double currentTime)
        {
            bool foundActive = false;

            foreach (var dialogue in allDialogues)
            {
                if (dialogue.IsActive(currentTime))
                {
                    foundActive = true;
                    if (!currentDialogue.Equals(dialogue.dialogueType) || !isCurrentDialogueActive)
                    {

                        Debug.Log($"Switching to dialogue: {currentTime} - {dialogue.dialogueType} - {isCurrentDialogueActive} - {currentDialogue.Equals(dialogue.dialogueType)}");
                        // Switch properly, ending previous first if needed
                        SwitchToDialogue(dialogue.dialogueType, endPrevious: isCurrentDialogueActive && !currentDialogue.Equals(dialogue.dialogueType));
                    }
                    break;
                }
            }

            // If nothing is active but we still think a dialogue is active and we are past its end time, end it
            if (!foundActive && isCurrentDialogueActive && dialogueByType.ContainsKey(currentDialogue))
            {
                DialogueInfo<T> info = dialogueByType[currentDialogue];
                if (currentTime >= info.endTime)
                {
                    EndCurrentDialogue();
                }
            }
        }

        public T FindNearestDialogueType(double currentTime = -1)
        {
            if (playableDirector == null) return default(T);

            if (currentTime < 0)
                currentTime = playableDirector.time;

            T nearestDialogue = default(T);
            double smallestDistance = double.MaxValue;

            foreach (var dialogue in allDialogues)
            {
                double distance = Math.Abs(currentTime - dialogue.startTime);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    nearestDialogue = dialogue.dialogueType;
                }
            }

            return nearestDialogue;
        }

        // Utility Methods
        public void AddDialogue(DialogueInfo<T> dialogue)
        {
            allDialogues.Add(dialogue);
            dialogueByType[dialogue.dialogueType] = dialogue;
        }

        public void RemoveDialogue(T dialogueType)
        {
            if (dialogueByType.ContainsKey(dialogueType))
            {
                allDialogues.RemoveAll(d => d.dialogueType.Equals(dialogueType));
                dialogueByType.Remove(dialogueType);
            }
        }

        public void ClearDialogues()
        {
            allDialogues.Clear();
            dialogueByType.Clear();
        }

        public DialogueInfo<T> GetDialogue(T dialogueType)
        {
            return dialogueByType.ContainsKey(dialogueType) ? dialogueByType[dialogueType] : null;
        }

        public bool HasDialogue(T dialogueType)
        {
            return dialogueByType.ContainsKey(dialogueType);
        }

        public void PrintDialogueInfo()
        {
            Debug.Log("=== Dialogue Information ===");
            foreach (var dialogue in allDialogues)
            {
                Debug.Log(dialogue.ToString());
            }
        }
        
        #region Skip Functionality
        
        /// <summary>
        /// Skip the current dialogue and move to the next one
        /// </summary>
        public void SkipCurrentDialogue()
        {
            if (!enableSkip || playableDirector == null) return;
            
            T skippedDialogue = currentDialogue;
            EndCurrentDialogue();
            
            // Find the next dialogue in sequence
            int currentIndex = -1;
            for (int i = 0; i < allDialogues.Count; i++)
            {
                if (allDialogues[i].dialogueType.Equals(currentDialogue))
                {
                    currentIndex = i;
                    break;
                }
            }
            
            if (currentIndex >= 0 && currentIndex + 1 < allDialogues.Count)
            {
                // Jump to next dialogue
                DialogueInfo<T> nextDialogue = allDialogues[currentIndex + 1];
                JumpToTime(nextDialogue.startTime);
                
                if (debugMode)
                    Debug.Log($"Skipped dialogue: {skippedDialogue} -> {nextDialogue.dialogueType}");
                    
                OnDialogueSkipped?.Invoke(skippedDialogue);
            }
            else
            {
                // Skip to end if no next dialogue
                if (playableDirector.duration > 0)
                {
                    JumpToTime(playableDirector.duration);
                    
                    if (debugMode)
                        Debug.Log($"Skipped to end: {skippedDialogue}");
                        
                    OnDialogueSkipped?.Invoke(skippedDialogue);
                }
            }
        }
        
        /// <summary>
        /// Skip to a specific dialogue type
        /// </summary>
        public void SkipToDialogue(T dialogueType)
        {
            if (!enableSkip) return;
            
            if (!currentDialogue.Equals(dialogueType))
            {
                EndCurrentDialogue();
            }
            
            if (dialogueByType.ContainsKey(dialogueType))
            {
                DialogueInfo<T> targetDialogue = dialogueByType[dialogueType];
                JumpToTime(targetDialogue.startTime);
                
                if (debugMode)
                    Debug.Log($"Skipped to dialogue: {dialogueType}");
                    
                OnDialogueSkipped?.Invoke(currentDialogue);
            }
        }
        
        /// <summary>
        /// Skip to the end of the current dialogue
        /// </summary>
        public void SkipToEndOfCurrentDialogue()
        {
            if (!enableSkip) return;
            
            EndCurrentDialogue();
            
            if (dialogueByType.ContainsKey(currentDialogue))
            {
                DialogueInfo<T> currentDialogueInfo = dialogueByType[currentDialogue];
                JumpToTime(currentDialogueInfo.endTime);
                
                if (debugMode)
                    Debug.Log($"Skipped to end of current dialogue: {currentDialogue}");
                    
                OnDialogueSkipped?.Invoke(currentDialogue);
            }
        }
        
        
        #endregion
    }

}
