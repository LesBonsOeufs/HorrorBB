using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Root
{
    [RequireComponent(typeof(FadeInOut_CanvasGroup))]
    public class RunTutorial : MonoBehaviour
    {
        [SerializeField] private uint nLoops = 3;
        [SerializeField] private Image runIcon;
        [SerializeField] private CanvasGroup runInputGroup;
        [SerializeField] private TextMeshProUGUI runReleaseMessage;
        [SerializeField] private float messageChangeDuration = 0.5f;

        private bool playerTiredLastValue = false;
        private uint remainingLoopsCount;

        private void OnEnable()
        {
            remainingLoopsCount = nLoops;
        }

        private void OnDisable()
        {
            playerTiredLastValue = false;
            Refresh(false);
        }

        private void Update()
        {
            runIcon.fillAmount = 1f - Player_Run.Instance.PreTiredness;
            bool lIsPlayerTired = Player_Run.Instance.Tiredness > 0f;

            if (lIsPlayerTired != playerTiredLastValue)
            {
                Refresh(lIsPlayerTired);

                if (!lIsPlayerTired)
                {
                    remainingLoopsCount--;

                    if (remainingLoopsCount <= 0)
                    {
                        GetComponent<FadeInOut_CanvasGroup>().FadeOut();
                        enabled = false;
                    }
                }
            }

            playerTiredLastValue = lIsPlayerTired;
        }

        private void Refresh(bool isTired)
        {
            runInputGroup.DOFade(isTired ? 0f : 1f, messageChangeDuration);
            runReleaseMessage.DOFade(isTired ? 1f : 0f, messageChangeDuration);
        }
    }
}