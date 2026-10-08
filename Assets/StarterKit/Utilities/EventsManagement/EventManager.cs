using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace StarterKit.Utilities
{
    public class EventManager : MonoBehaviour
    {
        public struct Event
        {
            public string eventName;
            public List<UnityEvent> events;

            public Event(string _eventName, UnityEvent _unityEvent)
            {
                eventName = _eventName;
                events = new List<UnityEvent>();
                events.Add(_unityEvent);
            }
            public void Register(UnityEvent thisEvent)
            {
                events.Add(thisEvent);
            }

            public void Unregister(UnityEvent thisEvent)
            {
                events.Remove(thisEvent);
            }
        }
    
        private static Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        // Register event with no parameters
        public static void RegisterEvent(string eventName, UnityAction listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }

        // Register event with one parameter
        public static void RegisterEvent<T>(string eventName, UnityAction<T> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }

        // Register event with two parameters
        public static void RegisterEvent<T1, T2>(string eventName, UnityAction<T1, T2> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }

        // Register event with three parameters
        public static void RegisterEvent<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }


        // Unregister event with no parameters
        public static void UnregisterEvent(string eventName, UnityAction listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            }
        }

        // Unregister event with one parameter
        public static void UnregisterEvent<T>(string eventName, UnityAction<T> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            }
        }
        // Unregister event with two parameters
        public static void UnregisterEvent<T1, T2>(string eventName, UnityAction<T1, T2> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            }
        }

        // Unregister event with three parameters
        public static void UnregisterEvent<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);
            }
        }

        // Raise event with no parameters
        public static void RaiseEvent(string eventName)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                var del = eventDictionary[eventName] as UnityAction;
                del?.Invoke();
            }
        }

        // Raise event with one parameter
        public static void RaiseEvent<T>(string eventName, T param1)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                var del = eventDictionary[eventName] as UnityAction<T>;
                del?.Invoke(param1);
            }
        }

        // Raise event with two parameters
        public static void RaiseEvent<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                var del = eventDictionary[eventName] as UnityAction<T1, T2>;
                del?.Invoke(param1, param2);
            }
        }

        // Raise event with three parameters
        public static void RaiseEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                var del = eventDictionary[eventName] as UnityAction<T1, T2, T3>;
                del?.Invoke(param1, param2, param3);
            }
        }
    }
}