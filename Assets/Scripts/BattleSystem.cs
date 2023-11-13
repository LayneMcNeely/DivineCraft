using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, END }
public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public SpriteRenderer playerSprite;
    public SpriteRenderer enemySprite;
    public SpriteRenderer enemyIntent;
    public List<GameObject> enemyPool = new List<GameObject>();
    public List<Transform> enemySpaces = new List<Transform>();

    [Header("Intent Sprites")]
    public Sprite redIntent;
    public Sprite medRedIntent;
    public Sprite lgRedIntent;
    public Sprite greenIntent;
    public Sprite blueIntent;
    public Sprite nothingIntent;

    
    private int enemySpaceIndex = 0;
    private int enemyAction;
    private int enemyCooldown;

    private float enemyATK;
    private float enemyScaler;
    private bool playerWeakend;
    private bool retaliate;
    private bool drained;
    //private bool plated;
    private bool exposed;
    private float playerPoison;
    private float enemyArmor;

    private bool isDead;

    [Header("Text Objects")]
    public TMP_Text enemyNameText;
    public TMP_Text dialogueText;
    public TMP_Text enemyArmorText;

    Enemy enemyUnit;
    Player playerUnit;

    [Header("HUDs")]
    public HUD playerHUD;
    public EnemyHUD enemyHUD;

    private float brace;
    public IEnumerator SetupBattle()//loads enemy and player info, resets variables
    {
        int enemyIndex = Random.Range(0, enemyPool.Count);//pick enemy from pool
        GameObject enemyGO = Instantiate(enemyPool[enemyIndex], enemySpaces[enemySpaceIndex]);//spawn enemy prefab
        enemySprite = enemyGO.GetComponent<SpriteRenderer>();
        
        enemyUnit = enemyGO.GetComponent<Enemy>();
        enemyNameText.text = enemyUnit.enemyName + " HP";
        enemyHUD.SetEnemyHUD(enemyUnit);

        SpriteRenderer[] sprites = enemyGO.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite.name != "intent")
            {
                continue;
            }
            else
            {
                enemyIntent = sprite;
            }
            
        }

        enemyCooldown = 0;
        enemyScaler = -3;
        playerWeakend = false;
        exposed = false;
        retaliate = false;
        drained = false;
        //plated = false;
        playerPoison = 0;
        enemyArmor = 0;

        dialogueText.text = "A "+ enemyUnit.enemyName +" approaches...";

        playerUnit = playerPrefab.GetComponent<Player>();
        playerSprite = playerPrefab.GetComponent<SpriteRenderer>();
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

        if (retaliate)
        {
            isDead = playerUnit.TakeDamage(5);//damage the player
            if (isDead)//check if player died
            {
                playerUnit.animator.SetTrigger("Death");
                yield return new WaitForSeconds(1);
                state = BattleState.LOST;
                EndBattle();
                yield break;
            }

        }
        if(playerWeakend)
        {
            playerATK -= Mathf.Round(playerATK / 4);
        }

        playerATK = Mathf.Round(playerATK);

        if (enemyArmor > 0)
        {
            enemyArmor = Mathf.Round(enemyArmor - playerATK);
        }
        else
        {
            isDead = enemyUnit.TakeDamage(playerATK);//damage the enemy
        }

        //if (armorValue <= 0 && plated === true)
        //{
        //    plated = false;
        //    lootDrop("scrap");
        //    lootDrop("scrap");
        //    populateBag(inventory);
        //}
        if (enemyArmor < 0)
        {
            enemyUnit.currentHP += enemyArmor;
            enemyArmor = 0;
        }
        enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();

        //isDead = enemyUnit.TakeDamage(playerATK);//damage the enemy

        playerUnit.currentEnergy -= playerUnit.energyDR;//player loses energy

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
            enemyIntent.enabled = false;
            playerUnit.gameIsActive = false;//disables combat buttons and enables movement
            state = BattleState.WON;
            EndBattle();
        }

    }
    void enemyATKcalc()
    {
        enemyATK = Random.Range(enemyUnit.minDMG+enemyScaler, enemyUnit.maxDMG+enemyScaler);//roll for enemy base damage
        

        if (enemyAction == 2)
        {
            enemyATK *= 2;
        }
        if (exposed == true)
        {
            exposed = false;
            enemyATK += enemyATK / 2;
        }

        enemyATK = enemyATK - enemyATK * brace;//calculate damage after brace

        enemyATK = Mathf.Round(enemyATK);
        isDead = playerUnit.TakeDamage(((int)enemyATK));//damage the player
    }

    void showIntent(int action)
    {
        switch(action)
        {
            case 1:
                if (exposed == true)
                {
                    enemyIntent.sprite = medRedIntent;
                }
                else
                {
                    enemyIntent.sprite = redIntent;
                }
                break;
            case 2:
                enemyIntent.sprite = lgRedIntent;
                break;
            case 3:
            case 4:
            case 5:
            case 6:
                enemyIntent.sprite = blueIntent;
                break;
            case 7:
            case 8:
            case 10:
                enemyIntent.sprite = greenIntent;
                break;
            case 9:
                if (exposed == true)
                {
                    enemyIntent.sprite = medRedIntent;
                }
                else
                {
                    enemyIntent.sprite = greenIntent;
                }
                break;
            default:
                enemyIntent.sprite = nothingIntent;
                break;
        }
    }

    void IsAttacking(int action)
    {
        switch(action)
        {
            case 1:
            case 2:
            case 5:
            case 10:
                enemyCooldown = 0;
                break;
            case 9:
                break;
            default:
                enemyCooldown++;
                if (enemyCooldown > 1)
                {
                    enemyAction = 1;
                    enemyCooldown = 0;
                }
                break;
        }
    }

    //Enemy Actions: 0 nothing, 1 basic attack, 2 pummel, 3 armor, 4 strength, 5 retaliation, 6 plated, 7 weaken, 8 drain, 9 expose, 10 poison, 11 life steal, 12 hex, 13 poison attack
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
                dialogueText.text = enemyUnit.enemyName + " uses Pummel " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 3://armor
                enemyArmor += 20;
                enemyArmorText.text = "Enemy Armor: "+ enemyArmor.ToString();
                dialogueText.text = enemyUnit.enemyName + " gains armor.";
                break;
            case 4://strengh
                enemyScaler += 3;
                dialogueText.text = enemyUnit.enemyName + " gains strength.";
                break;
            case 5://retaliation
                retaliate = true;
                dialogueText.text = enemyUnit.enemyName + " is retaliating.";
                break;
            case 6://plated armor
                enemyArmor += 40;
                //plated = true;
                enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();
                dialogueText.text = enemyUnit.enemyName + " gains plated armor.";
                break;
            case 7://weaken
                playerWeakend = true;
                dialogueText.text = "Knight is weakened.";
                break;
            case 8://drain
                drained = true;
                dialogueText.text = "Knight is drained.";
                break;
            case 9://expose
                if (exposed == true)
                {
                    enemyATKcalc();
                    dialogueText.text = enemyUnit.enemyName + " uses Super " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                }
                else
                {
                    dialogueText.text = "Knight is exposed.";
                    exposed = true;
                }
                break;
            case 10://poison
                playerPoison += 5;
                dialogueText.text = "Knight is poisoned";
                break;
            default://does nothing
                dialogueText.text = enemyUnit.enemyName + " does nothing.";
                break;
        }
        
        
        
        yield return new WaitForSeconds(0.5f);

        
        switch (enemyAction)//switch for handling player animations
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
            case 10://poison applied animation
                playerSprite.color = Color.green;
                //playerUnit.animator.SetTrigger("Hurt");
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
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
            if(drained == true)
            {
                drained = false;
                playerUnit.currentEnergy = 50;
                playerHUD.SetNRG(playerUnit.currentEnergy);
            }
            else
            {
                playerUnit.currentEnergy = 100;
                playerHUD.SetNRG(playerUnit.currentEnergy);
            }
            
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
        IsAttacking(enemyAction);
        showIntent(enemyAction);

        dialogueText.text = "Knight's Turn";
        enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();
    }

    public void OnAttackButton()//attack function clear checks
    {
        Debug.Log("Attack button clicked");
        if(playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if(state != BattleState.PLAYERTURN) { return; }//check if player's turn
        //future check if player is holding a weapon here
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
        bool isDeadbyPoison = false;
        if (playerPoison > 0)//handle poison damage
        {
            playerSprite.color = Color.green;
            playerUnit.animator.SetTrigger("Hurt");
            isDeadbyPoison = playerUnit.TakeDamage(playerPoison);
            playerHUD.SetHP(playerUnit.currentHP);
            yield return new WaitForSeconds(0.5f);
            playerSprite.color = Color.white;
            playerPoison -= 1;
        }
        if (isDeadbyPoison)//check if player died
        {
            playerUnit.animator.SetTrigger("Death");
            yield return new WaitForSeconds(1);
            state = BattleState.LOST;
            EndBattle();
            yield break;
        }
        enemyScaler += 3;
        if(playerUnit.currentEnergy == 100)
        {
            playerUnit.currentEnergy -= 15;
        }
        else
        {
            playerUnit.currentEnergy -= 10;
        }
        
        if(playerUnit.currentEnergy < 0)
            playerUnit.currentEnergy = 0;
        playerHUD.SetNRG(playerUnit.currentEnergy);
        brace = playerUnit.currentEnergy / 100f;

        dialogueText.text = "Knight braces for "+ playerUnit.currentEnergy.ToString() +"% of incoming damage.";
        playerUnit.animator.SetTrigger("Block");

        //reset one-turn effects
        retaliate = false;
        playerWeakend = false;
        enemyArmor = 0;
        //plated = false;

        yield return new WaitForSeconds(1);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    public void OnBraceButton()
    {
        if (playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        Debug.Log("Brace button clicked");
        if (playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }
        if (state != BattleState.PLAYERTURN) { return; }

        StartCoroutine(PlayerBrace());
    }

    public void OnThrowButton()
    {
        //temporary function
        if (playerUnit.gameIsActive == false) { dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        //throw button logic will go here. for now it just sets durability to 0.
        if (playerUnit.currentDurability > 0)
        {
            enemyUnit.TakeDamage(15);
            enemyHUD.SetHP(enemyUnit.currentHP);
            playerUnit.currentDurability = 0;
            playerHUD.SetDurability(playerUnit.currentDurability);
            dialogueText.text = "Knight throws Broadsword for 15 damage.";
            if (isDead)//check if enemy died
            {
                if (state == BattleState.END)
                {
                    Debug.Log("stop");
                    return;
                }
                enemySprite.enabled = false;
                enemyIntent.enabled = false;
                playerUnit.gameIsActive = false;//disables combat buttons and enables movement
                state = BattleState.WON;
                EndBattle();
            }
        }
        else
        {
            dialogueText.text = "Broadsword is brocken.";
        }
        
    }
}
