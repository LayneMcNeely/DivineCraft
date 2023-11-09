using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    public Slider hpSlider;

    public void SetEnemyHUD(Enemy enemy)
    {
        hpSlider.maxValue = enemy.maxHP;
        hpSlider.value = enemy.currentHP;
    }

    public void SetHP(float hp)
    {
        hpSlider.value = hp;
    }
}
