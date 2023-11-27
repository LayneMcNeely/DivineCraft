using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class EquippableItemSO : itemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        public AudioClip actionSFX { get; private set; }

        [field: SerializeField]
        public float MinDamage { get; set; } = 0;

        [field: SerializeField]
        public float MaxDamage { get; set; } = 0;

        [field: SerializeField]
        public int Accuracy { get; set; } = 0;

        [field: SerializeField]
        public int energyCost { get; set; } = 0;

        [field: SerializeField]
        public int MaxDurability { get; set; } = 0;

        [field: SerializeField]
        public int CurrentDurability { get; set; } = 0;

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null) 
            {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState);
                return true;
            }
            return false;
        }
    }
}