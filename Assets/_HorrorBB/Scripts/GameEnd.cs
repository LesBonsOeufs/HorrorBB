using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Root
{
    public class GameEnd : MonoBehaviour
    {
        [SerializeField] private Image whiteScreen;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float screenFadeDuration;
        [SerializeField] private float textFadeDuration;
        [SerializeField] private float readDuration;
        [SerializeField, Scene] private int sceneToLoad;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                whiteScreen.gameObject.SetActive(true);

                Player.Instance.SetInactiveMode(true);
                DOTween.Sequence(this)
                    .Append(whiteScreen.DOFade(1f, screenFadeDuration))
                    .Append(text.DOFade(1f, textFadeDuration))
                    .AppendInterval(readDuration)
                    .AppendCallback(() => SceneManager.LoadSceneAsync(sceneToLoad))
                    .SetUpdate(true);
            }
        }
    }
}