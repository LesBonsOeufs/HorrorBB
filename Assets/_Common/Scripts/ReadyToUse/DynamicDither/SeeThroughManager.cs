using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SeeThroughManager : Singleton<SeeThroughManager>
{
    [SerializeField] List<Transform> keepVisibleTargets;
    [SerializeField] private LayerMask seeThroughMask;
    [SerializeField, Foldout("Optimization"), Range(0f, 0.2f)] private float updateInterval = 0.05f;

    private new Camera camera;
    private List<DynamicDitherObject> activatedObjects = new List<DynamicDitherObject>();
    private float updateCounter;

    protected override void Awake()
    {
        base.Awake();
        camera = GetComponent<Camera>();
        updateCounter = updateInterval;
    }

    private void Update()
    {
        updateCounter -= Time.deltaTime;

        if (updateCounter <= 0f)
		{
            updateCounter = updateInterval;
            PeriodicUpdate();
		}
    }

    private void PeriodicUpdate()
    {
        Vector3 lTargetToCamera;
        List<DynamicDitherObject> lToDeactivate = new List<DynamicDitherObject>(activatedObjects);

        foreach (Transform lTarget in keepVisibleTargets)
        {
            lTargetToCamera = camera.transform.position - lTarget.position;

            RaycastHit[] lHits = Physics.RaycastAll(lTarget.position, lTargetToCamera, lTargetToCamera.magnitude, seeThroughMask);

            foreach (RaycastHit lHit in lHits)
            {
                if (lHit.transform.TryGetComponent(out DynamicDitherObject lObject)
                    || lHit.transform.parent.TryGetComponent(out lObject))
                {
                    if (!activatedObjects.Contains(lObject))
                    {
                        lObject.IsActive = true;
                        activatedObjects.Add(lObject);
                    }
                    else
                        lToDeactivate.Remove(lObject);
                }
            }
        }

        foreach (DynamicDitherObject lObject in lToDeactivate)
        {
            activatedObjects.Remove(lObject);
            lObject.IsActive = false;
        }
    }
}