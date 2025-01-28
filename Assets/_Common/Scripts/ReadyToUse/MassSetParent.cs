using UnityEngine;

namespace Root
{
    public class MassSetParent : MonoBehaviour
    {
        [SerializeField] private Transform targetParent;
        [SerializeField] private Transform[] objectsToReparent;

        private void Awake()
        {
            foreach (Transform lTransform in objectsToReparent)
                lTransform.SetParent(targetParent);
        }
    }
}