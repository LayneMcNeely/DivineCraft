using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentWeapon : MonoBehaviour
{
    [SerializeField]
    public EquippableItemSO weapon;

    [SerializeField]
    private InventorySO inventoyData;

    [SerializeField]
    private List<ItemParameter> parametersToModify, itemCurrentState;

    public Player playerStats;

    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState)//equips the weapon to player's hand
    {
        if (weapon != null)
        {
            inventoyData.AddItem(weapon, 1, itemCurrentState);
        }

        this.weapon = weaponItemSO;
        this.itemCurrentState = new List<ItemParameter>(itemState);
        playerStats.minDMG = weaponItemSO.MinDamage;
        playerStats.maxDMG = weaponItemSO.MaxDamage;
        playerStats.weaponACC = weaponItemSO.Accuracy;
        playerStats.energyDR = weaponItemSO.energyCost;
        playerStats.maxDurability = weaponItemSO.MaxDurability;
        playerStats.currentDurability = (int)itemCurrentState[0].value;
        Debug.Log("setWeap Durability "+ playerStats.currentDurability);
        //ModifyParameters();
    }

    public void ModifyDurability()//modify the durability value of the SO
    {
        foreach (var parameter in parametersToModify)
        {
            if (itemCurrentState.Contains(parameter))
            {
                //int index = itemCurrentState.IndexOf(parameter);
                float newValue = itemCurrentState[0].value - 1;
                Debug.Log("modifyDura "+ newValue);
                itemCurrentState[0] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
}
