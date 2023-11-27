using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string scene;
    public Player player;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerPrefs.SetFloat("playerHealth", player.currentHP);
        PlayerPrefs.SetString("currentLevel", scene);
        PlayerPrefs.Save();
        SceneManager.LoadScene(scene);
    }
}
