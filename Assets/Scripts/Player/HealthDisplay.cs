using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] public Health lives = null;
    [SerializeField] private TextMeshProUGUI localLivesText;


    [SerializeField] private string localPlayerLivesText = "Your Lives: ";

    // Subscribe to event
    private void OnEnable()
    {
        lives.EventLivesChanged += HandleLivesChanged;
    }

    // Unsubscribe to event
    private void OnDisable()
    {
        lives.EventLivesChanged -= HandleLivesChanged;
    }

    public void Start()
    {
        //GameObject test = GameObject.Find("RoundManager");
        //Debug.Log(test);
        // set initial lives
        localLivesText.text = localPlayerLivesText + lives.maxLives.ToString();
    }

    private void HandleLivesChanged(int currentLives, int maxLives)
    {
        //Debug.Log("fill amoutn changed");
        localLivesText.text = localPlayerLivesText + currentLives.ToString();
    }
}

