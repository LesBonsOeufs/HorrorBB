using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Root
{
    public class GameEnd : MonoBehaviour
    {
        [SerializeField] private Image whiteScreen;
        [SerializeField] private float fadeDuration;
        [SerializeField] private float readTextDuration;
        [SerializeField, Scene] private int sceneToLoad;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Player.Instance.SetInactiveMode(true);
                DOTween.Sequence(this)
                    .Append(whiteScreen.DOFade(1f, fadeDuration))
                    .AppendInterval(readTextDuration)
                    .AppendCallback(() => SceneManager.LoadSceneAsync(sceneToLoad));
            }
        }
    }
}