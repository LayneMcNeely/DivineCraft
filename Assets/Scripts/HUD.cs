using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Slider hpSlider;
    public Slider energySlider;
    public Slider durabilitySlider;

    public void SetPlayerHUD(Player player)
    {
        hpSlider.maxValue = player.maxHP;
        hpSlider.value = player.currentHP;
        energySlider.maxValue = player.maxEnergy;
        energySlider.value = player.currentEnergy;
        durabilitySlider.maxValue = player.maxDurability;
        durabilitySlider.value = player.currentDurability;
    }

    public void SetHP(float hp)
    {
        hpSlider.value = hp;
    }

    public void SetNRG(float nrg)
    {
        energySlider.value = nrg;
    }

    public void SetDurability(int dura)
    {
        durabilitySlider.value = dura;
    }
}
