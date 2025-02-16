using DG.Tweening;
using UnityEngine;

namespace Root
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeInOut_CanvasGroup : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 2f;

        public void FadeIn()
        {
            gameObject.SetActive(true);
            GetComponent<CanvasGroup>().DOFade(1f, fadeDuration);
        }

        public void FadeOut()
        {
            GetComponent<CanvasGroup>().DOFade(0f, fadeDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}