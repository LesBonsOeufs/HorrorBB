using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Root
{
    public class Inventory : Singleton<Inventory>
    {
        [SerializeField] private Transform UIContainer;
        [SerializeField] private Image collectibleUIPrefab;

        private readonly Dictionary<CollectableInfo, Image> collectableToUI = new();

        public bool HasCollectable(E_Collectable type) => collectableToUI.Keys.Where(info => info.Type == type).Count() > 0;

        public void Add(CollectableInfo info)
        {
            Image lCollectibleUI = Instantiate(collectibleUIPrefab, UIContainer);
            lCollectibleUI.sprite = info.Icon;
            collectableToUI.Add(info, lCollectibleUI);
        }

        public void Consume(E_Collectable type)
        {
            if (HasCollectable(type) == false)
                return;

            CollectableInfo lInfo = collectableToUI.Keys.First(info => info.Type == type);
            Remove(lInfo);
        }

        private void Remove(CollectableInfo info)
        {
            Destroy(collectableToUI[info].gameObject);
            collectableToUI.Remove(info);
        }
    }
}
