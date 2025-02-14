using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Root
{
    public class GameStart : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        [SerializeField] private float fadeOutDuration = 2f;

        private void Awake()
        {
            blackScreen.color = Color.black;
            blackScreen.DOFade(0f, fadeOutDuration);
        }
    }
}