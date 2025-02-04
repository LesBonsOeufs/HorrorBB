using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Root
{
    public class Collectable : OutlinedInteractable
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float alphaTweenDuration = 0.75f;
        [Expandable, SerializeField] private CollectableInfo info;
        [Foldout("Sound"), SerializeField] private AudioClip collectedSFX;
        [Foldout("Sound"), SerializeField] private float volume = 1f;

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

            if (collectedSFX != null)
                AudioSource.PlayClipAtPoint(collectedSFX, transform.position, volume);

            Inventory.Instance.Add(info);

            //Stop further interaction & fade out collectable
            Collider.enabled = false;
            meshRenderer.sharedMaterial = DynamicDitherManager.Instance.MakeSeeThroughVersion(meshRenderer.sharedMaterial);
            meshRenderer.sharedMaterial.DOFloat(0f, DynamicDitherManager.Instance.AlphaPropertyId, alphaTweenDuration)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}