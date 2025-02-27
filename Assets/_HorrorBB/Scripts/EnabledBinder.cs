using UnityEngine;

namespace Root
{
    [ExecuteAlways]
    public class EnabledBinder : MonoBehaviour
    {
        [SerializeField] private Behaviour source;
        [SerializeField] private Behaviour bound;
        [SerializeField] private bool isOpposite = false;

        private bool initEnabled;

        private void OnEnable()
        {
            initEnabled = bound.enabled;
        }

        private void OnDisable()
        {
            bound.enabled = initEnabled;
        }

        private void Update()
        {
            bound.enabled = isOpposite ? !source.enabled : source.enabled;
        }
    }
}