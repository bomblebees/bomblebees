using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    [Header("Start Round")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerRing;

    private bool timerStarted = false;
    private float roundDuration = 0f;
    private float currentTime = 0f;

    private void Update()
    {
        if (timerStarted)
        {
            if (currentTime > roundDuration)
            {
                Debug.Log("round finished");
                timerStarted = false;
            }

            timerText.text = (roundDuration - currentTime).ToString("F1");
            timerRing.fillAmount = 1 - (currentTime / roundDuration);

            currentTime += Time.deltaTime;
            //Debug.Log(currentTime);
        }
    }

    public void StartTimer(float duration)
    {
        roundDuration = duration;
        timerStarted = true;
    }

    public IEnumerator InitTimer(float duration, float freezeTime)
    {
        timerText.text = duration.ToString("F1");
        yield return new WaitForSeconds(freezeTime + 1);
        StartTimer(duration);
    }
}
