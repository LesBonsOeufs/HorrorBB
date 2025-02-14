using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Root
{
    [RequireComponent(typeof(RectTransform))]
    public class MoveOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float yShifting;
        [SerializeField] private float tweenDuration = 0.5f;

        private RectTransform rectTransform;
        private float baseAnchorPosY;

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosY(baseAnchorPosY + yShifting, tweenDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosY(baseAnchorPosY, tweenDuration);
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            baseAnchorPosY = rectTransform.anchoredPosition.y;
        }
    }
}