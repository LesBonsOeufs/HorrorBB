using System;

namespace GE
{
    public static class GlobalEvents
    {
        public static event Action<E_GlobalEvents> OnSpread;

        public static void Spread(E_GlobalEvents globalEvent)
        {
            OnSpread?.Invoke(globalEvent);
        }
    }

    public enum E_GlobalEvents
    {
        FirstKeyRetrieved
    }
}