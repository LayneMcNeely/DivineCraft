using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

public class Player : MonoBehaviour
{
    public float speed;

    public float maxHP;
    public float currentHP;
    public float maxDMG;
    public float minDMG;
    public int maxEnergy;
    public int currentEnergy;
    public int energyDR;
    public int maxDurability;
    public int currentDurability;
    public int weaponACC;

    [HideInInspector]
    public bool gameIsActive = false;

    private float hInput;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    public Animator animator;

    public HUD playerHUD;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("Hey you need a rigidbody!");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        currentHP = PlayerPrefs.GetFloat("playerHealth");
        if(playerHUD != null)
        {
            playerHUD.SetPlayerHUD(this);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Cancel") == 1)
        {
            PlayerPrefs.Save();
            SceneManager.LoadScene("MainMenu");
        }
        hInput = Input.GetAxis("Horizontal");

    }
    private void FixedUpdate()
    {
        if (!gameIsActive)//if event has been triggered, disable movement controls
        {
            rb.velocity = new Vector2(hInput * speed, 0);

            
        }
        if (rb.velocity.magnitude > 0.05f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if (rb.velocity.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (rb.velocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }

    }

    public bool TakeDamage(float dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
            return true;
        else return false;
    }
    public bool UseEnergy(int energyCost)
    {
        if (currentEnergy >= energyCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
