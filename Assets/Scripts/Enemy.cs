using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public float minDMG;
    public float maxDMG;
    public float maxHP;
    public float currentHP;
    public string mainATK;
    public List<int> moves = new List<int>();
    public List<GameObject> loot = new List<GameObject>();

    public bool TakeDamage(float dmg)
    {
        currentHP -= dmg;

        if(currentHP <= 0)
            return true; else return false;
    }
}
