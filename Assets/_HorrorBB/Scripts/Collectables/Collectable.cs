using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace Root
{
    public class Collectable : Interactable
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float alphaTweenDuration = 0.75f;
        [Expandable, SerializeField] private CollectableInfo info;

        public override void InteractorEnter() { }

        public override void InteractorExit() { }

        public override void Interact(Interactor interactor, bool isInteracting)
        {
            base.Interact(interactor, isInteracting);

            if (isInteracting == true)
                Collect();
        }

        private void Collect()
        {
            if (ActiveInteractor != null)
                ActiveInteractor.RemoveFromUsables(this);

            Inventory.Instance.Add(info);

            //Stop further interaction & fade out collectable
            Collider.enabled = false;
            meshRenderer.sharedMaterial = DynamicDitherManager.Instance.MakeSeeThroughVersion(meshRenderer.sharedMaterial);
            meshRenderer.sharedMaterial.DOFloat(0f, DynamicDitherManager.Instance.AlphaPropertyId, alphaTweenDuration)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}