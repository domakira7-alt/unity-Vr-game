using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaughtyAttributes;
using UISystem;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Data;
using Yudiz.VRAwarenessExperience.Interactions;
using Yudiz.VRAwarenessExperience.Utilities;
using Yudiz.VRAwarenessExperience.XR;
using Yudiz.XRStarter.Interactions;
using static Yudiz.VRAwarenessExperience.Utilities.TimerManager;
using System;
using Yudiz.VRAwarenessExperience.Events;
using StarterKit.Utilities;
using Unity.VisualScripting;
using Timer = Yudiz.VRAwarenessExperience.Utilities.TimerManager.Timer;
using Yudiz.VRAwarenessExperience.Manager;
using Yudiz.VRAwarenessExperience.Transition;
using Yudiz.VRAwarenessExperience.Effect;

namespace Yudiz.VRAwarenessExperience.Core
{
    public class Level : MonoBehaviour
    {
        [Header("Puzzle Pieces")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();
        [SerializeField] private List<XRPuzzleGrabbable> xrPuzzleGrabbables = new List<XRPuzzleGrabbable>();
        [SerializeField] private int wrongAttemptsLimit = 3;
        private int puzzlePiecesCount => puzzlePieces.Count;
        private int puzzlesCompletedCount = 0;

        [Header("Precision System")]
        [HorizontalLine(color: EColor.Blue)]
        private int correctAttempts = 0;
        private int wrongAttempts = 0;
        private float currentPrecisionValue = 0f;
        
        [Header("Pill Data")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private List<PillEffectContainer> pillEffectContainers = new List<PillEffectContainer>();
        private List<Pill> generatedPills = new List<Pill>();


        [Header("Parent Objects")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private Transform puzzleParent;
        [SerializeField] private Transform trayAndPillParent;

        [Header("Dissolve Effects Data")]
        [HorizontalLine(color: EColor.Indigo)]
        [SerializeField] private Transform completedAeroplaneParent;
        [SerializeField] private Transform puzzlePiecesParent;
        [SerializeField] private List<Material> trayAndPillMaterials = new List<Material>();
        [SerializeField] private List<Material> puzzleMaterials = new List<Material>();
        [SerializeField] private List<Material> completedAeroplaneMaterials = new List<Material>();

        [Header("Duration")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private float wrongAttemptDuration = 2f;
        [SerializeField] private float dissolveEffectDuration = 2.5f;
        
        [Header("Dissolve Effect")]
        [HorizontalLine(color: EColor.Yellow)]
        [SerializeField] private Vector2 trayAndPillDissolveEffectValue;
        [SerializeField] private Vector2 puzzleDissolveEffectValue;
        [SerializeField] private Vector2 completedAeroplaneDissolveEffectValue;

        [Header("PillIndicator")]
        [HorizontalLine(color: EColor.Indigo)]
        [SerializeField] private Material pillIndicatorMaterial;
        [SerializeField] private Transform pillIndicatorParent;
        [SerializeField] private Canvas pillIndicatorCanvas;
        [SerializeField] private UIAnimator pillIndicatorUIAnimator;
        [SerializeField] private List<PillIndicatorData> pillIndicatorDatas = new List<PillIndicatorData>();

        public delegate void OnPrecisionValueChangedDelegate(float value);
        public static event OnPrecisionValueChangedDelegate OnPrecisionValueChanged;

        public delegate void OnWrongAttemptDelegate(int currentWrongAttempts, int wrongAttemptsLimit);
        public static event OnWrongAttemptDelegate OnWrongAttempt;
        private LevelContainer levelContainer;

        private Timer timer;

        private float popupDuration = 0f;

        public void StartLevel(LevelContainer levelContainer)
        {
            Debug.Log("Level -> StartLevel -> Called!");
            this.levelContainer = levelContainer;
            PuzzlePiece.OnSnapComplete += OnSnapCompleted;
            Events.EventManager.LevelStarted();
            Debug.Log("Level -> StartLevel -> Level Container : " + levelContainer.levelName.ToString());
            if (levelContainer.isPillEffectRequired)
            {
                ManageXRPuzzleGrabbables(false);
                Debug.Log("Level -> StartLevel -> Pill Effect Required!");
                ResetDissolveEffect(trayAndPillMaterials, trayAndPillDissolveEffectValue.x);
                puzzleParent.gameObject.SetActive(false);
                trayAndPillParent.gameObject.SetActive(true);
                DoDissolveEffect(trayAndPillMaterials, trayAndPillDissolveEffectValue.x, trayAndPillDissolveEffectValue.y, dissolveEffectDuration);
                this.DelayedInvoke(() => GeneratePill(levelContainer.pillType), 0.8f);
                XRPillGrabbable.OnPillGrabbed += HidePillIndicator;
                XRPillGrabbable.OnPillReleased += ShowPillIndicatorOnGrabRelease;
            }
            else
            {
                ManageXRPuzzleGrabbables(false);
                Debug.Log("Level -> StartLevel -> Pill Effect Not Required!");
                puzzleParent.gameObject.SetActive(true);
                trayAndPillParent.gameObject.SetActive(false);
                SetMovementData(PillType.None);
                DoDissolveEffect(puzzleMaterials, puzzleDissolveEffectValue.x, puzzleDissolveEffectValue.y, dissolveEffectDuration, () => 
                {
                    Debug.Log("Puzzle Dissolve Effect Completed");
                    InitializeTimer();
                    InitializePrecision();
                    ManageXRPuzzleGrabbables(true);
                });
            }
        }

        public  void GeneratePill(PillType pillType)
        {
            foreach (PillEffectContainer pillEffectContainer in pillEffectContainers)
            {
                if (pillEffectContainer.pillPrefab != null)
                {
                    Pill pill = Instantiate(pillEffectContainer.pillPrefab, pillEffectContainer.pillParent);
                    pill.InitializePill(pillEffectContainer.pillType, this, pillEffectContainer.pillType == pillType, pillIndicatorMaterial);
                    generatedPills.Add(pill);
                }
            }
            ShowPillIndicator(pillType);
        }

        private void ShowPillIndicator(PillType pillType)
        {
            PillIndicatorData pillIndicatorData = pillIndicatorDatas.Find(x => x.pillType == pillType);
            if (pillIndicatorData != null)
            {
                Debug.Log("Showing Pill Indicator");
                Transform parent = pillIndicatorData.pillIndicatorSpawnPoint;
                pillIndicatorCanvas.transform.SetParent(parent);
                pillIndicatorCanvas.transform.localPosition = Vector3.zero;
                pillIndicatorUIAnimator.StartShow();
            }
        }

        private void ShowPillIndicatorOnGrabRelease()
        {
            Debug.Log("Pill realesed without eating .");
            ShowPillIndicator(levelContainer.pillType);
        }

        private void HidePillIndicator()
        {
            Debug.Log("Hide PillIndicator");
            pillIndicatorCanvas.enabled = false;
            pillIndicatorCanvas.transform.SetParent(pillIndicatorParent);
        }

        private void RemovePills()
        {
            foreach (Pill pill in generatedPills)
            {
                Destroy(pill.gameObject);
            }
            generatedPills.Clear();
        }

        public void OnPillEaten(PillType pillType)
        {
            SoundManager.instance.ChangeSoundPitchToPillEffect();
            Debug.Log("OnPillEaten "+pillType);
            HidePillIndicator();
            ShowTransitionEffect(pillType, () => 
            {
                ShowFullScreenEffect(pillType);
                puzzleParent.gameObject.SetActive(true);
                ResetDissolveEffect(trayAndPillMaterials, trayAndPillDissolveEffectValue.y);
                ResetDissolveEffect(puzzleMaterials, puzzleDissolveEffectValue.x);
                RemovePills();
                DoDissolveEffect(trayAndPillMaterials, trayAndPillDissolveEffectValue.y, trayAndPillDissolveEffectValue.x, dissolveEffectDuration, () => 
                {
                    trayAndPillParent.gameObject.SetActive(false);
                    SetMovementData(pillType);
                    DoDissolveEffect(puzzleMaterials, puzzleDissolveEffectValue.x, puzzleDissolveEffectValue.y, dissolveEffectDuration, () => 
                    {
                        InitializeTimer();
                        InitializePrecision();
                        ManageXRPuzzleGrabbables(true);
                    });
                });
            });
        }

        public void ShowTransitionEffect(PillType pillType, Action OnComplete)
        {
            PillEffectContainer pillEffectContainer = pillEffectContainers.Find(x => x.pillType == pillType);
            if (pillEffectContainer != null)
            {
                SoundManager.instance.PlaySound(SoundType.TransitionSound);
                TransitionsManager.instance.PlayTransition(TransitionType.PillWobble, pillEffectContainer.fromEffectData, pillEffectContainer.toEffectData, true, OnComplete);
            }
            else
            {
                OnComplete?.Invoke();
            }
        }

        public void ShowFullScreenEffect(PillType pillType)
        {
            PillEffectContainer pillEffectContainer = pillEffectContainers.Find(x => x.pillType == pillType);
            if (pillEffectContainer != null)
            {
                List<EffectDataContainer> effectContainers = pillEffectContainer.effectData;
                if (effectContainers.Count <= 0) return;
                foreach (EffectDataContainer effectDataContainer in effectContainers)
                {
                    EffectsManager.instance.PlayEffect(effectDataContainer.effectsType, effectDataContainer.effectData, null);
                }
            }
        }

        public async void StopFullScreenEffect(PillType pillType, Action OnComplete)
        {
            if (!levelContainer.isPillEffectRequired) 
            {
                OnComplete?.Invoke();
                return;
            }

            PillEffectContainer pillEffectContainer = pillEffectContainers.Find(x => x.pillType == pillType);
            if (pillEffectContainer != null)
            {
                List<EffectDataContainer> effectContainers = pillEffectContainer.effectData;
                if (effectContainers.Count <= 0) return;
                foreach (EffectDataContainer effectDataContainer in effectContainers)
                {
                    await EffectsManager.instance.StopEffect(effectDataContainer.effectsType);
                }
                OnComplete?.Invoke();
            }
        }

        private void SetMovementData(PillType pillType)
        {
            MovementData movementData = pillEffectContainers.Find(x => x.pillType == pillType).movementData;
            if (movementData != null)
            {
                foreach (PuzzlePiece puzzlePiece in puzzlePieces)
                {
                    puzzlePiece.InitializeMovementData(movementData);
                }
            }
        }

        private void ManageXRPuzzleGrabbables(bool isEnabled)
        {
            foreach (XRPuzzleGrabbable xrPuzzleGrabbable in xrPuzzleGrabbables)
            {
                xrPuzzleGrabbable.enabled = isEnabled;
            }
        }

        private void InitializeTimer()
        {
            Debug.Log("Level -> InitializeTimer -> Called!");
            timer = TimerManager.instance.CreateTimer(StringConstants.GAMEPLAY_TIMER_ID, levelContainer.levelTimeInSeconds);
            TimerManager.instance.RegisterTimerComplete(StringConstants.GAMEPLAY_TIMER_ID, OnLevelFailed);
            timer.Start();
            SoundManager.instance.PlayClockTickSound();
        }

        private void InitializePrecision()
        {
            correctAttempts = 0;
            wrongAttempts = 0;
            currentPrecisionValue = 0f;
            OnPrecisionValueChanged?.Invoke(currentPrecisionValue);
        }

        private void OnSnapCompleted(bool isCorrect)
        {
            if (isCorrect)
            {
                correctAttempts++;
                UpdatePrecisionValue();
                CheckForLevelCompeletion();
                Debug.Log($"Level -> OnSnap -> Correct Attempt! Total: {correctAttempts}");
            }
            else
            {
                SoundManager.instance.PlaySound(SoundType.WrongAttemptSound);
                wrongAttempts++;
                ManageWrongAttempt(wrongAttempts, wrongAttemptsLimit, levelContainer.isPillEffectRequired);
                UpdatePrecisionValue();
                Debug.Log($"Level -> OnSnap -> Wrong Attempt! Total: {wrongAttempts} / {wrongAttemptsLimit}");
                if (wrongAttempts >= wrongAttemptsLimit)
                {
                    OnLevelFailed();
                }
            }
        }


        private void UpdatePrecisionValue()
        {
            int totalAttempts = correctAttempts + wrongAttempts;

            if (wrongAttempts > wrongAttemptsLimit)
            {
                currentPrecisionValue = 0f;
                OnPrecisionValueChanged?.Invoke(currentPrecisionValue);
                return;
            }
            
            if (totalAttempts <= 0)
            {
                currentPrecisionValue = 0f;
            }
            else
            {
                float successRatio = (float)correctAttempts / totalAttempts;
                float wrongRatio = (float)wrongAttempts / totalAttempts;
                currentPrecisionValue = Mathf.Clamp01(successRatio - wrongRatio);
            }
            
            OnPrecisionValueChanged?.Invoke(currentPrecisionValue);
        }

        public void CheckForLevelCompeletion()
        {
            puzzlesCompletedCount = puzzlePieces.Count(x => x.IsAttached);
            Debug.Log($"Level -> Puzzle Completed Count : {puzzlesCompletedCount} / {puzzlePiecesCount}");
            if (puzzlesCompletedCount >= puzzlePiecesCount)
            {
                OnLevelCompleted();
            }
        }

        private void OnLevelCompleted()
        {
            timer.Stop();
            SoundManager.instance.StopClockTickSound();
            TimerManager.instance.RemoveTimer(StringConstants.GAMEPLAY_TIMER_ID);
            TimerManager.instance.UnregisterTimerComplete(StringConstants.GAMEPLAY_TIMER_ID, OnLevelFailed);
            SoundManager.instance.ResetSoundPitch();
            PuzzlePiece.OnSnapComplete -= OnSnapCompleted;
            XRPillGrabbable.OnPillGrabbed -= HidePillIndicator;
            XRPillGrabbable.OnPillReleased -= ShowPillIndicatorOnGrabRelease;
            Events.EventManager.LevelCompleted();
            completedAeroplaneParent.gameObject.SetActive(true);
            ResetDissolveEffect(completedAeroplaneMaterials, completedAeroplaneDissolveEffectValue.x);
            StopFullScreenEffect(levelContainer.pillType, () => 
            {
                OnWrongAttempt?.Invoke(0, wrongAttemptsLimit);
                SoundManager.instance.PlaySound(SoundType.DissolveSound);
                DoDissolveEffect(completedAeroplaneMaterials, completedAeroplaneDissolveEffectValue.x, completedAeroplaneDissolveEffectValue.y, dissolveEffectDuration, () => 
                {
                    SoundManager.instance.PlaySound(SoundType.WinSound);
                    puzzlePiecesParent.gameObject.SetActive(false);
                    //ResetLevelData();
                    ShowUI(true);
                    ResetLevelData();
                });
            });
        }

        private void ResetLevelData()
        {
            puzzlesCompletedCount = 0;
            correctAttempts = 0;
            wrongAttempts = 0;
            currentPrecisionValue = 0f;
            popupDuration = 0f;
            timer = null;
            levelContainer = null;
        }

        private void ShowUI(bool hasWon)
        {
            GameCompletionScreen gameCompletionScreen = ViewController.instance.GetScreen<GameCompletionScreen>(ScreenName.GameCompletionScreen);
            if (gameCompletionScreen != null)
            {
                bool isPillLevel = levelContainer != null && levelContainer.isPillEffectRequired;
                PillType pillType = levelContainer != null ? levelContainer.pillType : PillType.None;
                gameCompletionScreen.SetData(hasWon, wrongAttempts, isPillLevel, pillType);
                ViewController.instance.ChangeView(ScreenName.GameCompletionScreen);
            }
        }

        private void ManageWrongAttempt(int currentWrongAttempts, int wrongAttemptsLimit, bool isPillLevel)
        {
            OnWrongAttempt?.Invoke(currentWrongAttempts, wrongAttemptsLimit);
            HapticManager.instance.TriggerHapticBothHands(0.5f, 0.2f);
            WrongAttemptPopup wrongAttempt = ViewController.instance.GetScreen<WrongAttemptPopup>(ScreenName.WrongAttemptPopup);
            if (wrongAttempt != null)
            {
                string description = isPillLevel ? StringConstants.TEXT_STRING_WRONG_ATTEMPT_DESCRIPTION_PILL : StringConstants.TEXT_STRING_WRONG_ATTEMPT_DESCRIPTION;
                SoundType voiceType = isPillLevel ? SoundType.WrongAttemptVoicePill : SoundType.WrongAttemptVoiceNormal;

                float voiceDuration = SoundManager.instance.GetClipLength(voiceType);
                popupDuration = Mathf.Max(wrongAttemptDuration, voiceDuration);

                wrongAttempt.SetData(StringConstants.TEXT_STRING_WRONG_ATTEMPT_TITLE, description, true, popupDuration);

                SoundManager.instance.PlaySound(voiceType);
                SoundManager.instance.PlaySound(SoundType.WrongAttemptSound);

                ViewController.instance.OpenPopup(ScreenName.WrongAttemptPopup);
            }
        }


        private void ResetDissolveEffect(List<Material> materials, float value)
        {
            foreach (Material material in materials)
            {
                material.SetFloat("_CutoffHeight", value);
            }
        }

        private async void DoDissolveEffect(List<Material> materials, float startValue, float endValue, float duration, Action onComplete = null)
        {
            await Utilities.Utilities.LerpDissolveValue(materials, startValue, endValue, duration);
            onComplete?.Invoke();
        }

        private void OnLevelFailed()
        {
            Debug.Log("Level -> OnLevelFailed -> Called!");
            ManageXRPuzzleGrabbables(false);
            this.DelayedInvoke(() => {
                SoundManager.instance.StopOneShotSound();
                //ManageXRPuzzleGrabbables(false);
            //}, 1.5f);

            timer.Stop();
            TimerManager.instance.UnregisterTimerComplete(StringConstants.GAMEPLAY_TIMER_ID, OnLevelFailed);
            TimerManager.instance.RemoveTimer(StringConstants.GAMEPLAY_TIMER_ID);
            SoundManager.instance.StopClockTickSound();
            SoundManager.instance.PlaySound(SoundType.AlarmSound);
            SoundManager.instance.ResetSoundPitch();
            PuzzlePiece.OnSnapComplete -= OnSnapCompleted;
            XRPillGrabbable.OnPillGrabbed -= HidePillIndicator;
            XRPillGrabbable.OnPillReleased -= ShowPillIndicatorOnGrabRelease;

            StopFullScreenEffect(levelContainer.pillType, () =>
            {
                OnWrongAttempt?.Invoke(0, wrongAttemptsLimit);
                Events.EventManager.LevelFailed();

                DoDissolveEffect(
                    puzzleMaterials,
                    puzzleDissolveEffectValue.y,
                    puzzleDissolveEffectValue.x,
                    dissolveEffectDuration,
                    () =>
                    {
                        SoundManager.instance.PlaySound(SoundType.LoseSound);
                        //ResetLevelData();
                        ShowUI(false);
                        ResetLevelData();
                    }
                );
            });
            }, popupDuration);


            //this.DelayedInvoke(() =>
            //{
            //    StopFullScreenEffect(levelContainer.pillType, () =>
            //    {
            //        OnWrongAttempt?.Invoke(0, wrongAttemptsLimit);
            //        Events.EventManager.LevelFailed();

            //        DoDissolveEffect(
            //            puzzleMaterials,
            //            puzzleDissolveEffectValue.y,
            //            puzzleDissolveEffectValue.x,
            //            dissolveEffectDuration,
            //            () =>
            //            {
            //                SoundManager.instance.PlaySound(SoundType.LoseSound);
            //                ResetLevelData();
            //                ShowUI(false);
            //            }
            //        );
            //    });
            //}, popupDuration);
        }


        [ContextMenu("Complete Level")]
        public void CompleteLevel()
        {
            OnLevelCompleted();
        }


        [ContextMenu("Fail Level")]
        public void FailLevel()
        {
            OnLevelFailed();
        }

        [ContextMenu("Eat Pills")]
        public void EatPills()
        {
            OnPillEaten(levelContainer.pillType);
        }

    }
}

