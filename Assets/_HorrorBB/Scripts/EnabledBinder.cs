using UnityEngine;

namespace Root
{
    [ExecuteAlways]
    public class EnabledBinder : MonoBehaviour
    {
        [SerializeField] private Behaviour source;
        [SerializeField] private Behaviour bound;

        private void Update()
        {
            bound.enabled = source.enabled;
        }
    }
}