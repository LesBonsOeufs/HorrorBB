using UnityEngine;

//Must have a Collider in trigger mode
public abstract class Interactable : MonoBehaviour
{
    public Collider Collider { get; private set; }
    public virtual Interactor ActiveInteractor { get; protected set; }

    public abstract void InteractorEnter();
    public abstract void InteractorExit();

    protected virtual void Awake()
    {
        Collider = GetComponent<Collider>();
    }

    public virtual void Interact(Interactor interactor, bool isInteracting)
    {
        ActiveInteractor = isInteracting ? interactor : null;
    }

    private void OnValidate()
    {
        if (!TryGetComponent(out Collider lCollider) || !lCollider.isTrigger)
            Debug.LogError($"Interactable \"{name}\" should have a Collider in trigger mode!");
    }
}