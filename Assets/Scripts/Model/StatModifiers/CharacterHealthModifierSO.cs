using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        GameObject hud = GameObject.Find("PlayerHUD");
        Player health = character.GetComponent<Player>();
        HUD playerHUD = hud.GetComponent<HUD>();
        if (health != null)
        {
            health.currentHP += val;
            if (health.currentHP > health.maxHP)
            {
                health.currentHP = health.maxHP;
            }
            playerHUD.SetHP(health.currentHP);
        }
    }
}
