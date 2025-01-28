using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    public class Unlockable : Interactable
    {
        [SerializeField] private UnityEvent onUnlock;
        [SerializeField] private E_Collectable requiredCollectable = E_Collectable.Key;
        [SerializeField] private Outline outline;

        private bool CanBeUnlocked => Inventory.Instance.HasCollectable(requiredCollectable);

        public override void InteractorEnter()
        {
            if (!CanBeUnlocked)
                return;

            outline.enabled = true;
        }

        public override void InteractorExit()
        {
            if (!CanBeUnlocked)
                return;

            outline.enabled = false;
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