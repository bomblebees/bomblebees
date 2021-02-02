using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] public Health lives = null;
    [SerializeField] public Image livesBarImage = null;

    // Subscribe to event
    private void OnEnable()
    {
        // this.gameObject.transform.localPosition = new Vector3(100);
        lives.EventLivesChanged += HandleLivesChanged;
    }

    // Unsubscribe to event
    private void OnDisable()
    {
        lives.EventLivesChanged -= HandleLivesChanged;
    }

    private void HandleLivesChanged(int currentLives, int maxLives)
    {
        Debug.Log("fill amoutn changed");
        livesBarImage.fillAmount = (float) currentLives / maxLives;
    }
}

