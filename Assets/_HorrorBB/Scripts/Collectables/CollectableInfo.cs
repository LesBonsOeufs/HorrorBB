using NaughtyAttributes;
using UnityEngine;

namespace Root
{
    [CreateAssetMenu(fileName = "Collectable", menuName = "ScriptableObjects/CollectableInfo", order = 1)]
    public class CollectableInfo : ScriptableObject
    {
        [field: SerializeField] public E_Collectable Type { get; private set; }
        [field: SerializeField, ShowAssetPreview] public Sprite Icon { get; private set; }
    }
}