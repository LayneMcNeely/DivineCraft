using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    public BattleSystem battleSystem;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        if (collision.gameObject.tag == "Player")
        {
            other.GetComponent<Player>().gameIsActive = true;
            battleSystem.state = BattleState.START;
            StartCoroutine(battleSystem.SetupBattle());
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
}
