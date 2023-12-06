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

    [Header("Character Prefabs")]
    public GameObject playerPrefab;
    public SpriteRenderer playerSprite;
    public SpriteRenderer enemySprite;
    public SpriteRenderer enemyIntent;
    public List<GameObject> enemyPool = new List<GameObject>();
    public List<Transform> enemySpaces = new List<Transform>();
    public GameObject werewolf;
    public GameObject demon;
    private GameObject enemyTransform;

    [Header("Intent Sprites")]
    public Sprite redIntent;
    public Sprite medRedIntent;
    public Sprite lgRedIntent;
    public Sprite greenIntent;
    public Sprite blueIntent;
    public Sprite nothingIntent;

    [Header("Loot Drops")]
    public GameObject wood;
    public GameObject stone;
    public GameObject scrap;
    public GameObject firethorn;
    public GameObject mageflower;
    public GameObject sickleaf;

    [Header("Audio Sources")]
    public AudioSource playerHitAudio;
    public AudioSource enemyHitAudio;
    public AudioSource pummelHitAudio;
    public AudioSource armorHitAudio;
    public AudioSource braceAudio;
    public AudioSource itemThrowAudio;
    public AudioSource itemThrowArmorAudio;
    public AudioSource missAudio;
    public AudioSource errorAudio;
    

    private int enemySpaceIndex = 0;
    private int enemyAction;
    private int enemyCooldown;

    private float enemyATK;
    private float enemyScaler;
    private bool playerWeakend;
    private bool retaliate;
    private bool drained;
    private bool plated;
    private bool exposed;
    private bool hexxed;
    private float playerPoison;
    private float enemyArmor;
    private bool ghostTransform;

    private bool isDead;

    [Header("Text Objects")]
    public TMP_Text enemyNameText;
    public TMP_Text dialogueText;
    public TMP_Text enemyArmorText;

    Enemy enemyUnit;
    Player playerUnit;
    AgentWeapon agentWeapon;

    [Header("HUDs")]
    public HUD playerHUD;
    public EnemyHUD enemyHUD;

    private float brace;
    public IEnumerator SetupBattle()//loads enemy and player info, resets variables
    {
        int enemyIndex = Random.Range(0, enemyPool.Count);//pick enemy from pool
        GameObject enemyGO = Instantiate(enemyPool[enemyIndex], enemySpaces[enemySpaceIndex]);//spawn enemy prefab
        enemySprite = enemyGO.GetComponent<SpriteRenderer>();
        enemyTransform = enemyGO;
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
        plated = false;
        hexxed = false;
        ghostTransform = false;
        playerPoison = 0;
        enemyArmor = 0;

        dialogueText.text = "A "+ enemyUnit.enemyName +" approaches...";

        playerUnit = playerPrefab.GetComponent<Player>();
        playerSprite = playerPrefab.GetComponent<SpriteRenderer>();
        agentWeapon = playerPrefab.GetComponent<AgentWeapon>();
        playerUnit.currentEnergy = 100;
        playerHUD.SetNRG(100);
        playerHUD.SetPlayerHUD(playerUnit);

        yield return new WaitForSeconds(1);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }
    IEnumerator PlayerAttack()//main player attack function that passes after checks have been cleared
    {
        if(enemyUnit.enemyName == "Demonic Spirit")
            ghostTransform = true;
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
            armorHitAudio.Play();
        }
        else
        {
            isDead = enemyUnit.TakeDamage(playerATK);//damage the enemy
            playerHitAudio.Play();
        }

        if (enemyArmor <= 0 && plated == true)
        {
            plated = false;
            LootDrop("scrap", 2);
        }
        if (enemyArmor < 0)
        {
            enemyUnit.currentHP += enemyArmor;
            enemyArmor = 0;
            isDead = enemyUnit.TakeDamage(-enemyArmor);//damage the enemy
        }
        enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();

        playerUnit.currentEnergy -= playerUnit.energyDR;//player loses energy

        playerUnit.currentDurability--;
        //modify SO durability in inventory
        agentWeapon.ModifyDurability();

        playerHUD.SetNRG(playerUnit.currentEnergy);//update player hud
        playerHUD.SetDurability(playerUnit.currentDurability);
        playerHUD.SetPlayerHUD(playerUnit);

        enemyHUD.SetHP(enemyUnit.currentHP);//update enemy hud
        dialogueText.text = "Knight uses "+ agentWeapon.weapon.Name +" for "+ playerATK +" damage.";
        playerUnit.animator.SetTrigger("Attack1");//play animation for attacking
        
        if(enemyUnit.enemyName == "Wolf")
        {
            if(enemyUnit.currentHP < enemyUnit.maxHP / 2)
            {
                float reminder = enemyUnit.currentHP;
                enemySprite.sprite = werewolf.GetComponent<SpriteRenderer>().sprite;
                enemyTransform.transform.localScale = werewolf.transform.localScale;
                enemyUnit = werewolf.GetComponent<Enemy>();
                enemyNameText.text = enemyUnit.enemyName + " HP";
                enemyUnit.currentHP = reminder;
                enemyHUD.SetHP(enemyUnit.currentHP);
            }
        }

        yield return new WaitForSeconds(1);
        if (isDead)//check if enemy died
        {
            if(state == BattleState.END)
            {
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
            case 1://red intent (attack)
            case 13:
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
            case 11:
                enemyIntent.sprite = lgRedIntent;
                break;
            case 3://blue intent (buff)
            case 4:
            case 5:
            case 6:
                enemyIntent.sprite = blueIntent;
                break;
            case 7://green intent (debuff)
            case 8:
            case 10:
            case 12:
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

    void IsAttacking(int action)//keeps track of if the enemy has attacked. if anemy has not attacked in the last the turn, force attack.
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
        if(ghostTransform == true)
        {
            float reminder = enemyUnit.currentHP;
            enemySprite.sprite = demon.GetComponent<SpriteRenderer>().sprite;
            enemyTransform.transform.localScale = demon.transform.localScale;
            enemyUnit = demon.GetComponent<Enemy>();
            enemyNameText.text = enemyUnit.enemyName + " HP";
            enemyUnit.currentHP = reminder;
            enemyHUD.SetHP(enemyUnit.currentHP);
        }
        switch (enemyAction)
        {
            case 1://basic attack
                enemyATKcalc();
                enemyHitAudio.Play();
                dialogueText.text = enemyUnit.enemyName + " uses " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 2://pummel
                enemyATKcalc();
                pummelHitAudio.Play();
                dialogueText.text = enemyUnit.enemyName + " uses Pummel " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 3://armor
                braceAudio.Play();
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
                braceAudio.Play();
                enemyArmor += 40;
                plated = true;
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
                    enemyHitAudio.Play();
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
            case 11://life steal
                enemyATKcalc();
                enemyHitAudio.Play();
                enemyUnit.currentHP += enemyATK;
                if(enemyUnit.currentHP > enemyUnit.maxHP)
                    enemyUnit.currentHP = enemyUnit.maxHP;
                enemyHUD.SetHP(enemyUnit.currentHP);
                dialogueText.text = enemyUnit.enemyName + " uses Life Steal " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                break;
            case 12://hex
                if (hexxed == true)
                {
                    enemyATKcalc();
                    enemyHitAudio.Play();
                    dialogueText.text = "Knight is Hexxed." + enemyUnit.enemyName + " uses " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
                }
                else
                {
                    hexxed = true;
                    dialogueText.text = "Knight is hexxed.";
                }
                break;
            case 13://poison attack
                playerPoison += 5;
                enemyATKcalc();
                enemyHitAudio.Play();
                dialogueText.text = enemyUnit.enemyName + " uses Venom " + enemyUnit.mainATK + " for " + enemyATK + " damage.";
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
            case 7://weaken applied animation
                playerSprite.color = Color.yellow;
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
                break;
            case 8://drain applied animation
                playerSprite.color = Color.gray;
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
                break;
            case 10://poison applied animation
                playerSprite.color = Color.green;
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
                break;
            case 11://life steal animation
                playerSprite.color = Color.red;
                playerUnit.animator.SetTrigger("Hurt");
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
                break;
            case 12://hex animation
                playerSprite.color = Color.magenta;
                yield return new WaitForSeconds(0.5f);
                playerSprite.color = Color.white;
                playerUnit.animator.SetTrigger("SetIdle");
                break;
            case 13://poison attack animation
                playerSprite.color = Color.green;
                playerUnit.animator.SetTrigger("Hurt");
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

    private void LootDrop(string loot, int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            switch (loot)
            {
                case "wood":
                    Instantiate(wood, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                case "stone":
                    Instantiate(stone, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                case "scrap":
                    Instantiate(scrap, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                case "firethorn":
                    Instantiate(firethorn, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                case "mageflower":
                    Instantiate(mageflower, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                case "sickleaf":
                    Instantiate(sickleaf, new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
                default:
                    if (enemyUnit.loot.Count == 0 || enemyUnit.loot == null)
                    {
                        Debug.Log("Loot table is missing");
                        break;
                    }
                    int index = Random.Range(0, enemyUnit.loot.Count-1);
                    Instantiate(enemyUnit.loot[index], new Vector3(enemyUnit.transform.position.x + i, enemyUnit.transform.position.y, enemyUnit.transform.position.z), Quaternion.identity);
                    break;
            }
        }
        
    }

    void EndBattle()//win and loss states
    {
        if(state == BattleState.WON) 
        {
            LootDrop("table", 2);
            state = BattleState.END;
            dialogueText.text = "You won the battle! Continue forward.";
            enemySpaceIndex++;
            
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
        if( agentWeapon.weapon == null) { errorAudio.Play(); dialogueText.text = "Your hand is empty."; return; }//check if player has a weapon equipped
        if (enemyUnit == null) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if enemy is empty
        if (playerUnit.gameIsActive == false) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if(state != BattleState.PLAYERTURN) { errorAudio.Play(); return; }//check if player's turn
        if (playerUnit.currentDurability <= 0) { errorAudio.Play(); dialogueText.text = agentWeapon.weapon.Name +" is broken. Change weapon."; return; }//check if player has durability
        bool hasEnergy = playerUnit.UseEnergy(playerUnit.energyDR);//check if player has enough energy
        
        if(hasEnergy == true)
        {
            if(hexxed == true)
            {
                playerUnit.currentEnergy = 0;
                playerHUD.SetNRG(playerUnit.currentEnergy);
                hexxed = false;
            }
            playerUnit.animator.SetTrigger("Attack1");//play animation for attacking
            int weaponHIT = Random.Range(1, 100);
            if(weaponHIT > playerUnit.weaponACC)
            { //check if attack hits
                missAudio.Play();
                playerUnit.currentEnergy -= playerUnit.energyDR;
                playerHUD.SetNRG(playerUnit.currentEnergy);
                playerHUD.SetPlayerHUD(playerUnit);
                dialogueText.text = "Knight uses "+ agentWeapon.weapon.Name +" and misses."; 
                return; 
            }
            StartCoroutine(PlayerAttack());
        }
        else
        {
            errorAudio.Play();
            dialogueText.text = "You are out of energy.";
            return;
        }
        
    }
    IEnumerator PlayerBrace()
    {
        state = BattleState.ENEMYTURN;
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
        plated = false;

        yield return new WaitForSeconds(1);

        
        StartCoroutine(EnemyTurn());
    }

    public void OnBraceButton()
    {
        if (enemyUnit == null) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if enemy is empty
        if (playerUnit.gameIsActive == false) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if (state != BattleState.PLAYERTURN) { errorAudio.Play(); return; }//check if player's turn
        braceAudio.Play();
        StartCoroutine(PlayerBrace());
    }

    public void OnThrowButton()//throw from hand
    {
        if (playerUnit.gameIsActive == false) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if (enemyUnit == null) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if enemy is empty
        if (state != BattleState.PLAYERTURN) { errorAudio.Play(); return; }//check if player's turn
        //throw button logic will go here. for now it just sets durability to 0.
        if(agentWeapon.weapon != null)
        {
            if (enemyArmor > 0)
            {
                enemyArmor = Mathf.Round(enemyArmor - 15);
                itemThrowArmorAudio.Play();
            }
            else
            {
                isDead = enemyUnit.TakeDamage(15);//damage the enemy
                itemThrowAudio.Play();
            }

            if (enemyArmor <= 0 && plated == true)
            {
                plated = false;
                LootDrop("scrap", 2);
            }
            if (enemyArmor < 0)
            {
                enemyUnit.currentHP += enemyArmor;
                enemyArmor = 0;
                isDead = enemyUnit.TakeDamage(-enemyArmor);//damage the enemy
            }
            enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();
            
            enemyHUD.SetHP(enemyUnit.currentHP);
            playerUnit.currentDurability = 0;
            playerHUD.SetDurability(playerUnit.currentDurability);
            dialogueText.text = "Knight throws " + agentWeapon.weapon.Name + " for 15 damage.";
            agentWeapon.weapon = null;
            if (isDead)//check if enemy died
            {
                if (state == BattleState.END)
                {
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
            errorAudio.Play();
            dialogueText.text = "Your hand is empty.";
        }
    }

    public void ThrowFromBag()
    {
        if (enemyUnit == null) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if enemy is empty
        if (playerUnit.gameIsActive == false) { errorAudio.Play(); dialogueText.text = "You are out of battle."; return; }//check if player is in battle
        if (state != BattleState.PLAYERTURN) { errorAudio.Play(); return; }//check if player's turn
        //throw button logic will go here. for now it just sets durability to 0.
        if (enemyArmor > 0)
        {
            enemyArmor = Mathf.Round(enemyArmor - 15);
            itemThrowArmorAudio.Play();
        }
        else
        {
            isDead = enemyUnit.TakeDamage(15);//damage the enemy
            itemThrowAudio.Play();
        }

        if (enemyArmor <= 0 && plated == true)
        {
            plated = false;
            LootDrop("scrap", 2);
        }
        if (enemyArmor < 0)
        {
            enemyUnit.currentHP += enemyArmor;
            enemyArmor = 0;
            isDead = enemyUnit.TakeDamage(-enemyArmor);//damage the enemy
        }
        enemyArmorText.text = "Enemy Armor: " + enemyArmor.ToString();

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "Knight throws an item for 15 damage.";
        if (isDead)//check if enemy died
        {
            if (state == BattleState.END)
            {
                return;
            }
            enemySprite.enabled = false;
            enemyIntent.enabled = false;
            playerUnit.gameIsActive = false;//disables combat buttons and enables movement
            state = BattleState.WON;
            EndBattle();
        }
        
    }
}
