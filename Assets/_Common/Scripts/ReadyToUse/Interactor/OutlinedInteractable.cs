using QuickOutline;
using UnityEngine;

public abstract class OutlinedInteractable : Interactable
{
    [SerializeField] protected Outline outline;

    public override void InteractorEnter() => outline.enabled = true;
    public override void InteractorExit() => outline.enabled = false;
}