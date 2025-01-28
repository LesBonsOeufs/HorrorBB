using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Root
{
    public class Inventory : Singleton<Inventory>
    {
        [SerializeField] private Transform UIContainer;
        [SerializeField] private Image collectibleUIPrefab;

        private readonly Dictionary<CollectableInfo, Image> collectableToUI = new();

        public void Add(CollectableInfo info)
        {
            Image lCollectibleUI = Instantiate(collectibleUIPrefab, UIContainer);
            lCollectibleUI.sprite = info.Icon;
            collectableToUI.Add(info, lCollectibleUI);
        }

        public void Remove(CollectableInfo info)
        {
            Destroy(collectableToUI[info].gameObject);
            collectableToUI.Remove(info);
        }
    }
}
