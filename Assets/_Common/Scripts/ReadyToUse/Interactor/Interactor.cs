using System.Collections.Generic;
using UnityEngine;

public delegate void InteractorEventHandler();
/// <summary>
/// Attach this script to the game object which will interact with Interactables
/// </summary>
public class Interactor : MonoBehaviour
{
    #region Camera frustum mode
    [SerializeField, Tooltip("Only interacts with items that are visible from the main camera")] private bool useCameraFrustum = false;
    /// <summary>
    /// Frustum mode only
    /// </summary>
    private Dictionary<Collider, Interactable> colliderToInteractableToFrustumTest = new();
    #endregion

    /// <summary>
    /// Use add/remove methods instead of directly accessing this
    /// </summary>
    private HashSet<Interactable> _usableInteractables = new();

    public bool HasUsableInteractable => _usableInteractables.Count > 0;
    public Interactable ActiveInteractable { get; private set; }
    public bool IsInteracting { get; private set; }

    public event InteractorEventHandler OnInteractableExit;

    private void AddToUsables(Interactable interactable)
    {
        _usableInteractables.Add(interactable);
        interactable.InteractorEnter();
    }

    //Public is quick & dirty solution for collectable
    public void RemoveFromUsables(Interactable interactable)
    {
        _usableInteractables.Remove(interactable);

        if (ActiveInteractable == interactable)
        {
            StopInteraction();
            OnInteractableExit?.Invoke();
        }

        interactable.InteractorExit();
    }

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

        foreach (Interactable lInteractable in _usableInteractables)
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
            if (useCameraFrustum)
                colliderToInteractableToFrustumTest.Add(other, lInteractable);
            else
                AddToUsables(lInteractable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable lInteractable))
        {
            if (useCameraFrustum)
                colliderToInteractableToFrustumTest.Remove(other);

            RemoveFromUsables(lInteractable);
        }
    }

    //Could be more optimized
    private void OnTriggerStay(Collider other)
    {
        if (!useCameraFrustum)
            return;

        if (colliderToInteractableToFrustumTest.TryGetValue(other, out Interactable lInteractable))
        {
            Vector3 lViewportPos = Camera.main.WorldToViewportPoint(lInteractable.transform.position);
            bool lIsInFrustum = lViewportPos.x >= 0f && lViewportPos.x <= 1f && lViewportPos.y >= 0f && lViewportPos.y <= 1f && lViewportPos.z > 0f;

            if (!_usableInteractables.Contains(lInteractable) && lIsInFrustum)
                AddToUsables(lInteractable);
            else if (_usableInteractables.Contains(lInteractable) && !lIsInFrustum)
                RemoveFromUsables(lInteractable);
        }
    }
}