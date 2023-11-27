using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VillageFunctions : MonoBehaviour
{
    public Player player;
    public HUD playerHUD;
    public TMP_Text dialogueText;

    [HideInInspector]
    public int count;
    public int maxActions = 2;
    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }
    public void Heal()
    {
        if(count < maxActions)
        {
            count++;
            player.currentHP += 20;
            if (player.currentHP > 100)
            {
                player.currentHP = 100;
            }
            playerHUD.SetHP(player.currentHP);
            dialogueText.text = "The Knight takes a rest. ("+ count +"/"+ maxActions +")";
        }
        else
        {
            dialogueText.text = "No more service.";
        }
    }
}
