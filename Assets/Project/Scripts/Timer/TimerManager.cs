using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using StarterKit.Utilities;
using StarterKit;
using System.Linq;

namespace Yudiz.VRAwarenessExperience.Utilities
{
    public class TimerManager : Singleton<TimerManager>
    {
        public delegate void OnNewTimerAddedDelegate(string timerId);
        public static OnNewTimerAddedDelegate onNewTimerAdded;
        public delegate void OnTimerRemovedDelegate(string timerId);
        public static OnTimerRemovedDelegate onTimerRemoved;

        private Dictionary<string, Timer> activeTimers = new Dictionary<string, Timer>();

        public Timer CreateTimer(string timerId, float duration, TimerMode timerMode = TimerMode.Reverse)
        {
            if (activeTimers.ContainsKey(timerId))
            {
                Debug.LogWarning($"Timer with ID {timerId} already exists. Returning existing timer.");
                return activeTimers[timerId];
            }
            Timer timer = new Timer(timerId, duration, timerMode);
            activeTimers.Add(timerId, timer);
            onNewTimerAdded?.Invoke(timerId);
            Debug.Log("New timer added with id : " + timerId);
            return timer;
        }

        public void RemoveTimer(string timerId)
        {
            if (activeTimers.ContainsKey(timerId))
            {
                Debug.Log("Timer removed with Id : " + timerId);
                activeTimers[timerId].Stop();
                activeTimers.Remove(timerId);
                onTimerRemoved?.Invoke(timerId);
            }
        }

        public void LogActiveTimers()
        {
            if (activeTimers.Count == 0)
            {
                Debug.Log("🕓 No active timers.");
                return;
            }

            Debug.Log("📋 Active Timers:");

            foreach (var kvp in activeTimers)
            {
                var timer = kvp.Value;
                Debug.Log($"🕒 Timer ID: {timer.Id} | Remaining: {timer.RemainingTime:F2}s | Running: {timer.IsRunning} | Complete: {timer.IsComplete}");
            }
        }
        
        public bool IsTimerActive(string timerId)
        {
            return activeTimers.ContainsKey(timerId) && activeTimers[timerId].IsRunning;
        }

        public Timer GetTimer(string timerId)
        {
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
                return timer;
            }
            return null;
        }

        public void RegisterTimerComplete(string timerId, UnityAction onComplete)
        {
            // If timer already exists, register the callback
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
                timer.OnComplete += onComplete;
            }
        }

        public void UnregisterTimerComplete(string timerId, UnityAction onComplete)
        {
            // If timer exists, unregister the callback
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
                timer.OnComplete -= onComplete;
            }
        }

        public void RegisterTimerUpdate(string timerId, UnityAction<float, float> onUpdate)
        {
            // If timer already exists, register the callback
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
                timer.OnUpdate += onUpdate;
            }
        }

        public void UnregisterTimerUpdate(string timerId, UnityAction<float, float> onUpdate)
        {
            // If timer exists, unregister the callback
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
                timer.OnUpdate -= onUpdate;
            }
        }

        public float GetCurrentTime(string timerId)
        {
            if (activeTimers.TryGetValue(timerId, out Timer timer))
            {
               return timer.GetCurrentTime();
            }

            return float.MinValue;
        }

        /*private void Update()
        {
            List<string> completedTimers = new List<string>();
            
            // foreach (var timer in activeTimers.Values)
            int timersCount = activeTimers.Count;
            for (int i = 0; i < timersCount; i++)
            {
                Timer timer = activeTimers.Values.ElementAt(i);
                timer.Update(Time.deltaTime);
                if (timer.IsComplete)
                {
                    completedTimers.Add(timer.Id);
                }
            }

            foreach (var timerId in completedTimers)
            {
                RemoveTimer(timerId);
            }
        }*/

        private void Update()
        {
            List<string> completedTimers = new List<string>();

            // Take a snapshot of timers
            var timersSnapshot = activeTimers.Values.ToList();

            foreach (var timer in timersSnapshot)
            {
                timer.Update(Time.deltaTime);
                if (timer.IsComplete)
                {
                    completedTimers.Add(timer.Id);
                }
            }

            foreach (var timerId in completedTimers)
            {
                RemoveTimer(timerId);
            }
        }


        public class Timer
        {
            public string Id { get; private set; }
            public float Duration { get; private set; }
            public float RemainingTime { get; private set; }
            public bool IsRunning { get; private set; }
            public bool IsComplete { get; private set; }
			public TimerMode TimerMode { get; private set; }

			public UnityAction OnComplete;
            public UnityAction<float, float> OnUpdate;

            public Timer(string id, float duration, TimerMode timerMode = TimerMode.Reverse)
            {
                Id = id;
                TimerMode = timerMode;
                Duration = duration;
                RemainingTime = duration;
                IsRunning = false;
                IsComplete = false;
            }

            public void Start()
            {
                IsRunning = true;
                IsComplete = false;
                RemainingTime = Duration;
            }

            public void Stop()
            {
                IsRunning = false;
            }

            public void Pause()
            {
                IsRunning = false;
            }

            public void Resume()
            {
                IsRunning = true;
            }

            public void Update(float deltaTime)
            {
                if (!IsRunning || IsComplete) return;

                if (TimerMode == TimerMode.Forward)
                {
                    RemainingTime += deltaTime;
                    OnUpdate?.Invoke(RemainingTime, Duration);
                }
                else
                {
					RemainingTime -= deltaTime;
					OnUpdate?.Invoke(RemainingTime, Duration);

					if (RemainingTime <= 0)
					{
						Debug.Log("Timer completed with Id : " + Id);
						RemainingTime = 0;
                        OnUpdate?.Invoke(RemainingTime, Duration);
						IsComplete = true;
						IsRunning = false;
						OnComplete?.Invoke();
					}
				}
            }

            public void Reset()
            {
                RemainingTime = Duration;
                IsComplete = false;
            }

            public float GetCurrentTime()
            {
                return RemainingTime;
            }
        }
    }

    public enum TimerMode
    {
        Forward,
        Reverse
    }
}