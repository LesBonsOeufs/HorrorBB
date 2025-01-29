using System;
using System.Collections.Generic;
using UnityEngine;

namespace GE
{
    public class GlobalEvents : Singleton<GlobalEvents>
    {
        private readonly HashSet<E_GlobalEvents> pastEvents = new();
        public event Action<E_GlobalEvents> OnSpread;

        public bool WasSpread(E_GlobalEvents globalEvent) => pastEvents.Contains(globalEvent);

        public void Spread(E_GlobalEvents globalEvent)
        {
            if (WasSpread(globalEvent))
            {
                Debug.LogWarning($"Event {globalEvent} has already been spread.");
                return;
            }

            OnSpread?.Invoke(globalEvent);
            pastEvents.Add(globalEvent);
        }
    }

    public enum E_GlobalEvents
    {
        FirstKeyRetrieved,
        DownStairsCrawler
    }
}