using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    public class CrawlingMan_ReactToConcealers : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<Concealer, UnityEvent> enterConcealerReaction;
        [SerializeField] private SerializedDictionary<Concealer, UnityEvent> exitConcealerReaction;

        private void OnEnable()
        {
            Player.Instance.OnEnterConcealer += Player_OnEnterConcealer;
            Player.Instance.OnExitConcealer += Player_OnExitConcealer;
        }

        private void OnDisable()
        {
            Player.Instance.OnEnterConcealer -= Player_OnEnterConcealer;
            Player.Instance.OnExitConcealer -= Player_OnExitConcealer;
        }

        private void Player_OnEnterConcealer(Concealer concealer) => enterConcealerReaction[concealer]?.Invoke();

        private void Player_OnExitConcealer(Concealer concealer) => exitConcealerReaction[concealer]?.Invoke();
    }
}