using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Root
{
    public class GameEnd : MonoBehaviour
    {
        [SerializeField] private Image whiteScreen;
        [SerializeField] private float fadeDuration;
        [SerializeField] private float readTextDuration;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //Player.Instance.SetInactiveMode(true);
                //DOTween.Sequence(this)
                //    .Append(whiteScreen.DOFade(1f, fadeDuration))
                //    .AppendInterval(readTextDuration)
                //    .Append(LOAD MAIN MENU)
            }
        }
    }
}