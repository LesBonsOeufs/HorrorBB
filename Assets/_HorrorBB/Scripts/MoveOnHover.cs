using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Root
{
    [RequireComponent(typeof(RectTransform))]
    public class MoveOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float yShifting;
        [SerializeField] private float tweenDuration = 0.5f;
        [SerializeField] private Image image;
        [SerializeField] private Color color = Color.white;

        private RectTransform rectTransform;
        private float baseAnchorPosY;
        private Color baseColor;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            baseColor = image.color;
            baseAnchorPosY = rectTransform.anchoredPosition.y;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosY(baseAnchorPosY + yShifting, tweenDuration);
            image.DOColor(color, tweenDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosY(baseAnchorPosY, tweenDuration);
            image.DOColor(baseColor, tweenDuration);
        }
    }
}