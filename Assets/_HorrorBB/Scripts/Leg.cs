using NaughtyAttributes;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Leg : MonoBehaviour
{
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform rayForwardOrigin;
    [SerializeField] private Transform rayDownOrigin;
    public GameObject ikTarget;

    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private AnimationCurve heightCurve;

    [Foldout("Advanced")] public float tipAnimationDuration = 0.15f;
    [Foldout("Advanced"), SerializeField] private float tipAnimationFrameTime = 1 / 60.0f;

    [Foldout("Advanced"), SerializeField] private float tipMaxHeight = 0.2f;
    [Foldout("Advanced"), SerializeField] private float initTipPosZOffset = 0f;
    [Foldout("Advanced"), SerializeField] private float ikYOffset = 1.0f;
    [Foldout("Advanced"), SerializeField] private float tipMoveDist = 0.55f;
    [Foldout("Advanced"), SerializeField] private float maxRayDist = 2.0f;
    //Currently should stay at 0, for preventing passing tip through wall
    [Foldout("Advanced"), SerializeField] private float tipPassOver = 0.55f / 2.0f;

    public Vector3 TipPos { get; private set; }
    public Vector3 TipUpDir { get; private set; }
    public Vector3 RaycastTipPos { get; private set; }
    public Vector3 RaycastTipNormal { get; private set; }

    public bool Animating { get; private set; } = false;
    public bool Movable { get; set; } = false;
    public float TipDistance { get; private set; }

    public Ray ForwardRay => new(rayForwardOrigin.position, 
        Vector3.ProjectOnPlane(rayForwardOrigin.position - lastRayForwardOriginPos, bodyTransform.up).normalized);
    public Ray DownRay => new(rayDownOrigin.position, bodyTransform.up * -1);

    //Used for forwardRay's direction
    private Vector3 lastRayForwardOriginPos;

    private void Awake()
    {
        ikTarget.transform.parent = null;
        TipPos = ikTarget.transform.position;
    }

    private void Start()
    {
        lastRayForwardOriginPos = rayForwardOrigin.position;
        TipPos += bodyTransform.forward * initTipPosZOffset;
        UpdateIKTargetTransform();
    }

    private void Update()
    {
        //Choose the closest valid hit
        RaycastHit? lHit =
            new RaycastHit?[] { /*Raycast(ForwardRay),*/ Raycast(DownRay) }
            .OrderBy(hit => hit == null ? Mathf.Infinity : hit.Value.distance).First();

        if (lHit != null)
        {
            RaycastTipPos = lHit.Value.point;
            RaycastTipNormal = lHit.Value.normal;
        }

        //TipDistance = Vector3.ProjectOnPlane(RaycastTipPos - TipPos, bodyTransform.up).magnitude;
        TipDistance = (RaycastTipPos - TipPos).magnitude;

        // If the distance gets too far, animate and move the tip to new position
        if (!Animating && TipDistance > tipMoveDist && Movable)
            StartCoroutine(AnimateLeg());

        if (lastRayForwardOriginPos != rayForwardOrigin.position)
            lastRayForwardOriginPos = rayForwardOrigin.position;
    }

    private RaycastHit? Raycast(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit lHit, maxRayDist))
            return lHit;

        return null;
    }

    private IEnumerator AnimateLeg()
    {
        Animating = true;

        float lTimer = 0.0f;
        float lAnimTime;
        Vector3 lInitTipPos = TipPos;

        while (lTimer < tipAnimationDuration + tipAnimationFrameTime)
        {
            lAnimTime = speedCurve.Evaluate(lTimer / tipAnimationDuration);

            Vector3 lTipToTarget = RaycastTipPos - lInitTipPos;
            Vector3 lTipDirection = lTipToTarget.normalized;
            lTipToTarget += lTipDirection * tipPassOver;
            TipUpDir = Vector3.Cross(lTipDirection, Vector3.Cross(bodyTransform.up, lTipDirection).normalized);

            TipPos = lInitTipPos + lTipToTarget * lAnimTime;
            TipPos += TipUpDir * heightCurve.Evaluate(lAnimTime) * tipMaxHeight;

            UpdateIKTargetTransform();
            lTimer += tipAnimationFrameTime;
            yield return new WaitForSeconds(tipAnimationFrameTime);
        }

        Animating = false;
    }

    private void UpdateIKTargetTransform()
    {
        // Update leg ik target transform depend on tip information
        ikTarget.transform.position = TipPos + bodyTransform.up.normalized * ikYOffset;
        ikTarget.transform.rotation = Quaternion.LookRotation(bodyTransform.forward, bodyTransform.up);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(RaycastTipPos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(TipPos, RaycastTipPos);

        Gizmos.color = Color.white;
        Gizmos.DrawRay(ForwardRay);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(DownRay);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(ikTarget.transform.position, 0.1f);
    }
}
