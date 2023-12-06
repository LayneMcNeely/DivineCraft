using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory.Model
{
    [CreateAssetMenu]
    public class MaterialItemSO : itemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Drop";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState)
        {
            Debug.Log("item dropped");
            return true;
        }
    }
}

