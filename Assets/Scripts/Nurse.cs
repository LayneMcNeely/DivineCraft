using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse : MonoBehaviour
{
    [SerializeField]
    private GameObject nurseMenu;
    private void Start()
    {
        nurseMenu.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        nurseMenu.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        nurseMenu.SetActive(false);
    }
}
