using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    public class CrawlingMan_ReactToConcealers : MonoBehaviour
    {
        [SerializeField] private float reactionDelay = 1f;
        [SerializeField] private SerializedDictionary<E_Concealer, UnityEvent> enterConcealerReaction;
        [SerializeField] private SerializedDictionary<E_Concealer, UnityEvent> exitConcealerReaction;

        private void OnEnable()
        {
            Player.Instance.OnEnterConcealer += Player_OnEnterConcealer;
            Player.Instance.OnExitConcealer += Player_OnExitConcealer;
            Player.Instance.OnDeath += Player_OnDeath;
        }

        private void OnDisable()
        {
            Player.Instance.OnEnterConcealer -= Player_OnEnterConcealer;
            Player.Instance.OnExitConcealer -= Player_OnExitConcealer;
            Player.Instance.OnDeath -= Player_OnDeath;
        }

        private void Player_OnEnterConcealer(E_Concealer concealer)
        {
            DOVirtual.DelayedCall(reactionDelay, () => enterConcealerReaction[concealer]?.Invoke(), false).SetTarget(this);
        }

        private void Player_OnExitConcealer(E_Concealer concealer)
        {
            DOVirtual.DelayedCall(reactionDelay, () => exitConcealerReaction[concealer]?.Invoke(), false).SetTarget(this);
        }

        private void Player_OnDeath(int nDeaths)
        {
            DOTween.Kill(this);
        }
    }
}