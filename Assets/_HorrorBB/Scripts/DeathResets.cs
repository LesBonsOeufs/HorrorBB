using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    public class DeathResets : MonoBehaviour
    {
        [SerializeField] private UnityEvent onDeath;

        private void Awake()
        {
            Player.Instance.OnDeath += Player_OnDeath;
        }

        private void Player_OnDeath(int nDeaths)
        {
            onDeath?.Invoke();
        }
    }
}