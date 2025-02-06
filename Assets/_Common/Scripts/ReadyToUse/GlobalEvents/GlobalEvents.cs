using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GE
{
    public class GlobalEvents : Singleton<GlobalEvents>
    {
        private readonly HashSet<E_GlobalEvents> pastEvents = new();
        public event Action<E_GlobalEvents> OnSpread;

#if UNITY_EDITOR
        [HorizontalLine]
        [SerializeField] private E_GlobalEvents debugEvent;

        [Button]
        private void SpreadDebugEvent() => Spread(debugEvent);
#endif

        public static void ToGEGizmosColor()
        {
            ColorUtility.TryParseHtmlString("#FA6DB030", out Color lColor);
            Gizmos.color = lColor;
        }

        public bool WasSpread(E_GlobalEvents globalEvent) => pastEvents.Contains(globalEvent);

        public void Spread(E_GlobalEvents globalEvent)
        {
            if (WasSpread(globalEvent))
            {
                //Debug.LogWarning($"Event {globalEvent} has already been spread.");
                return;
            }

            OnSpread?.Invoke(globalEvent);
            pastEvents.Add(globalEvent);
            Debug.Log($"Spread event {globalEvent}!");
        }
    }

    public enum E_GlobalEvents
    {
        FirstKeyRetrieved,
        PlayerReturnedDownstairs,
        PlayerApproachedMonster
    }
}