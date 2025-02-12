using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    [RequireComponent(typeof(CrawlingMan))]
    public class CrawlingMan_Guarding : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(0f, 5f)] private Vector2 detectionMinMax = new (2f, 3f);

        //Annoyance system is very specific
        [SerializeField] private UnityEvent onAnnoyed;
        [SerializeField] private int repetitionsForAnnoyed = 2;

        private CrawlingMan crawlingMan;
        /// <summary>
        /// Init crawler's target position is guarded
        /// </summary>
        private Transform initTarget;
        private int annoyedCounter = 0;
        
        private bool IsChasing
        {
            get => crawlingMan.MoveTarget == Player.Instance.transform;

            set
            {
                crawlingMan.MoveTarget = value ? Player.Instance.transform : initTarget;

                if (!value)
                {
                    annoyedCounter++;

                    if (annoyedCounter >= repetitionsForAnnoyed)
                    {
                        onAnnoyed?.Invoke();
                        annoyedCounter = 0;
                    }
                }
            }
        }

        private void Awake()
        {
            crawlingMan = GetComponent<CrawlingMan>();
        }

        private void OnEnable()
        {
            initTarget = crawlingMan.MoveTarget;
        }

        private void OnDisable()
        {
            if (crawlingMan == null || Player.Instance == null || crawlingMan.MoveTarget == null)
                return;

            IsChasing = false;
        }

        private void Update()
        {
            float lTargetToPlayerDistance = (Player.Instance.transform.position - initTarget.position).magnitude;

            if (IsChasing)
            {
                if (lTargetToPlayerDistance > detectionMinMax.y)
                    IsChasing = false;
            }
            else if (lTargetToPlayerDistance < detectionMinMax.x)
                IsChasing = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;

            Transform lTransform = initTarget == null ? transform : initTarget;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lTransform.position, detectionMinMax.x);
            Gizmos.DrawWireSphere(lTransform.position, detectionMinMax.y);
        }
    }
}