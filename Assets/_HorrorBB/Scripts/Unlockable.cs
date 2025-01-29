using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    public class Unlockable : OutlinedInteractable
    {
        [SerializeField] private UnityEvent onUnlock;
        [SerializeField] private E_Collectable requiredCollectable = E_Collectable.Key;

        private bool CanBeUnlocked => Inventory.Instance.HasCollectable(requiredCollectable);

        public override void InteractorEnter()
        {
            if (!CanBeUnlocked)
                return;

            base.InteractorEnter();
        }

        public override void Interact(Interactor interactor, bool isInteracting)
        {
            base.Interact(interactor, isInteracting);

            if (isInteracting == true)
                TryUnlock();
        }

        private void TryUnlock()
        {
            if (!CanBeUnlocked)
                return;

            if (ActiveInteractor != null)
                ActiveInteractor.RemoveFromUsables(this);

            Inventory.Instance.Consume(requiredCollectable);
            Unlock();
        }

        [Button]
        private void Unlock()
        {
            onUnlock?.Invoke();
            Destroy(Collider);
            Destroy(this);
        }
    }
}