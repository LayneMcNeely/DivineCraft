using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, END }
public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    public GameObject playerPrefab;
    public SpriteRenderer enemySprite;
    public List<GameObject> enemyPool = new List<GameObject>();

    public List<Transform> enemySpaces = new List<Transform>();
    private int enemySpaceIndex = 0;
    private int enemyAction;

    private float enemyATK;
    private float enemyScaler = -3;

    public TMP_Text enemyNameText;
    public TMP_Text dialogueText;

    Enemy enemyUnit;
    Player playerUnit;

    public HUD playerHUD;
    public EnemyHUD enemyHUD;

    private float brace;
    public IEnumerator SetupBattle()//loads enemy and player info
    {
        int enemyIndex = Random.Range(0, enemyPool.Count);//pick enemy from pool
        GameObject enemyGO = Instantiate(enemyPool[enemyIndex], enemySpaces[enemySpaceIndex]);//spawn enemy prefab
        enemySprite = enemyGO.GetComponent<SpriteRenderer>();
        enemyUnit = enemyGO.GetComponent<Enemy>();
        enemyNameText.text = enemyUnit.enemyName + " HP";
        enemyHUD.SetEnemyHUD(enemyUnit);
        enemyScaler = -3;
        Debug.Log(enemyScaler);
        dialogueText.text = "A "+ enemyUnit.enemyName +" approaches...";

        playerUnit = playerPrefab.GetComponent<Player>();
        playerUnit.currentEnergy = 100;
        playerHUD.SetNRG(100);
        playerHUD.SetPlayerHUD(playerUnit);

        yield return new WaitForSeconds(1);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }
    IEnumerator PlayerAttack()//main player attack function that passes after checks have been cleared
    {
        float playerATK = Random.Range(playerUnit.minDMG, playerUnit.maxDMG);//roll for base damage
        playerATK = Mathf.Round(playerATK);
        bool isDead = enemyUnit.TakeDamage(playerATK);//damage the enemy

        playerUnit.currentEnergy -= playerUnit.energyDR;//player loses energy
        Debug.Log(playerUnit.currentEnergy);

        playerUnit.currentDurability--;

        playerHUD.SetNRG(playerUnit.currentEnergy);//update player hud
        playerHUD.SetDurability(playerUnit.currentDurability);
        playerHUD.SetPlayerHUD(playerUnit);

        enemyHUD.SetHP(enemyUnit.currentHP);//update enemy hud
        dialogueText.text = "Knight uses Broadsword for "+ playerATK +" damage.";
        playerUnit.animator.SetTrigger("Attack1");//play animation for attacking

        yield return new WaitForSeconds(1);
        if (isDead)//check if enemy died
        {
            if(state == BattleState.END)
            {
                Debug.Log("stop");
                yield break; 
            }
            enemySprite.enabled = false;
            playerUnit.gameIsActive = false;//disables combat buttons and enables movement
            state = BattleState.WON;
            EndBattle();
        }

    }
    void enemyATKcalc()
    {
        enemyATK = Random.Range(enemyUnit.minDMG+enemyScaler, enemyUnit.maxDMG+enemyScaler);//roll for enemy base damage
        enemyATK = enemyATK - enemyATK * brace;//calculate damage after brace

        if (enemyAction == 2)
        {
            enemyATK *= 2;
        }

        enemyATK = Mathf.Round(enemyATK);
    }

    //Enemy Actions: 0 nothing, 1 basic attack, 2 pummel, 3 armor, 4 strength, 5 retaliation, 6 plated, 7 weaken, 8 drain, 9 expose, 10 poison, 11 life steal, 12 hex, 13 poison attack, 14 regenerate, 15 armor attack
    IEnumerator EnemyTurn()//enemy performs action here
    {
        switch (enemyAction)
        {
            case 1://basic attack
                enemyATKcalc();
                dialogueText.text = enemyUnit.enemyName + " uses " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 2://pummel
                enemyATKcalc();
                dialogueText.text = enemyUnit.enemyName + " uses Pummel" + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 4://strengh
                enemyScaler += 3;
                Debug.Log(enemyScaler);
                dialogueText.text = enemyUnit.enemyName + " gains strength.";
                break;
            default://does nothing
                dialogueText.text = enemyUnit.enemyName + " does nothing.";
                break;
        }
        
        
        
        yield return new WaitForSeconds(0.5f);

        bool isDead = playerUnit.TakeDamage(((int)enemyATK));//damage the player
        switch (enemyAction)
        {
            case 1://basic attack
                playerUnit.animator.SetTrigger("Hurt");//play hurt animation
                break;
            case 2://pummel
                playerUnit.animator.SetTrigger("Hurt");//play hurt animation
                yield return new WaitForSeconds(0.33f);
                playerUnit.animator.SetTrigger("Hurt");
                yield return new WaitForSeconds(0.33f);
                playerUnit.animator.SetTrigger("Hurt");
                break;
            default:
                playerUnit.animator.SetTrigger("SetIdle");
                break;
        }
        
        playerHUD.SetHP(playerUnit.currentHP);//update player hud

        yield return new WaitForSeconds(0.5f);

        if (isDead)//check if player died
        {
            playerUnit.animator.SetTrigger("Death");
            yield return new WaitForSeconds(1);
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            playerUnit.currentEnergy = 100;
            playerHUD.SetNRG(100);
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }
    void EndBattle()//win and loss states
    {
        if(state == BattleState.WON) 
        {
            state = BattleState.END;
            dialogueText.text = "You won the battle! Continue forward.";
            enemySpaceIndex++;
            
            Debug.Log("space index "+enemySpaceIndex);
        }
        else if(state == BattleState.LOST)
        {
            state = BattleState.END;
            dialogueText.text = "You have been defeated. Game over.";
            SceneManager.LoadScene("MainMenu");
        }
    }
    void PlayerTurn()//at the start of the player's turn
    {
        int enemyActionIndex = Random.Range(0, enemyUnit.moves.Count-1);
        enemyAction = enemyUnit.moves[enemyActionIndex];
        dialogueText.text = "Knight's Turn";
    }

    public void OnAttackButton()//attack function clear checks
    {
        Debug.Log("Attack button clicked");
        if(playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if(state != BattleState.PLAYERTURN) { return; }//check if player's turn
        //future: check if player is holding a weapon 
        if (playerUnit.currentDurability <= 0) { dialogueText.text = "Broadsword is broken. Change weapon."; return; }//check if player has durability
        bool hasEnergy = playerUnit.UseEnergy(playerUnit.energyDR);//check if player has enough energy
        
        if(hasEnergy == true)
        {
            playerUnit.animator.SetTrigger("Attack1");//play animation for attacking
            int weaponHIT = Random.Range(1, 100);
            if(weaponHIT > playerUnit.weaponACC) 
            { //check if attack hits
                playerUnit.currentEnergy -= playerUnit.energyDR;
                playerHUD.SetNRG(playerUnit.currentEnergy);
                playerHUD.SetPlayerHUD(playerUnit);
                dialogueText.text = "Knight uses Broadsword and misses."; 
                return; 
            }
            StartCoroutine(PlayerAttack());
        }
        else
        {
            dialogueText.text = "You are out of energy.";
            return;
        }
        
    }
    IEnumerator PlayerBrace()
    {
        enemyScaler+= 3;
        Debug.Log(enemyScaler);
        playerUnit.currentEnergy -= 10;
        playerHUD.SetNRG(playerUnit.currentEnergy);
        brace = playerUnit.currentEnergy / 100f;
        Debug.Log("Brace "+brace);
        dialogueText.text = "Knight braces for "+ playerUnit.currentEnergy.ToString() +"% of incoming damage.";
        playerUnit.animator.SetTrigger("Block");

        yield return new WaitForSeconds(1);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public void OnBraceButton()
    {
        Debug.Log("Brace button clicked");
        if (playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }
        if (state != BattleState.PLAYERTURN) { return; }

        StartCoroutine(PlayerBrace());
    }
}
