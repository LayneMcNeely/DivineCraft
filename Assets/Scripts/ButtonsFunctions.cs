using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonsFunctions : MonoBehaviour
{
    [SerializeField]
    private InventoryController inventoryData;
    public void Play(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetFloat("playerHealth", 100);
        PlayerPrefs.SetString("currentLevel", "StrongholdOverworld");
        PlayerPrefs.Save();
        SceneManager.LoadScene("StrongholdOverworld");
    }

    public void Continue()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("currentLevel"));
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
    }
}
