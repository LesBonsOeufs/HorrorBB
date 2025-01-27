using System.Collections.Generic;
using UnityEngine;

public delegate void InteractorEventHandler();
/// <summary>
/// Attach this script to the game object which will interact with Interactables
/// </summary>
public class Interactor : MonoBehaviour
{
    private List<Interactable> interactablesInRange = new();

    public bool IsInteractableInRange => interactablesInRange.Count > 0;
    public Interactable ActiveInteractable { get; private set; }
    public bool IsInteracting { get; private set; }

    public event InteractorEventHandler OnInteractableExit;

    public bool StartInteraction()
    {
        ActiveInteractable = GetClosestInteractable();

        //No interactables, could add feedback here
        if (ActiveInteractable == null)
            return false;

        ActiveInteractable.Interact(this, true);

        IsInteracting = true;
        return true;
    }

    public bool StopInteraction()
    {
        IsInteracting = false;

        //No active interactable
        if (ActiveInteractable == null)
            return false;

        ActiveInteractable.Interact(this, false);
        ActiveInteractable = null;

        return true;
    }

    private Interactable GetClosestInteractable()
    {
        Interactable lClosestInteractable = null;
        float lMinDistance = Mathf.Infinity;
        float lToInteractableDistance;

        foreach (Interactable lInteractable in interactablesInRange)
        {
            lToInteractableDistance = (lInteractable.transform.position - transform.position).magnitude;

            if (lToInteractableDistance < lMinDistance)
            {
                lClosestInteractable = lInteractable;
                lMinDistance = lToInteractableDistance;
            }
        }

        return lClosestInteractable;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable lInteractable))
        {
            interactablesInRange.Add(lInteractable);
            lInteractable.InteractorEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable lInteractable))
        {
            interactablesInRange.Remove(lInteractable);

            if (ActiveInteractable == lInteractable)
            {

                StopInteraction();
                OnInteractableExit?.Invoke();
            }

            lInteractable.InteractorExit();
        }
    }
}