using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    //public GameObject playerModelsAndVfx;

    [Header("User Interface")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject gameOverUI;

    [Header("Settings")]
    [SerializeField] private int deathUItime = 3;

    public IEnumerator EnableDeathUI()
    {
        // WARN: lazy way to disable player after death, may still be able to place bombs
        //playerModelsAndVfx.SetActive(false); 

        deathUI.SetActive(true);
        yield return new WaitForSeconds(deathUItime);
        deathUI.SetActive(false);
    }

    public void EnableGameOverUI()
    {
        deathUI.SetActive(false);
        gameOverUI.SetActive(true);
    }
}