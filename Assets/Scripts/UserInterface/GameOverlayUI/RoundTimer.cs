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
    [SerializeField] private float flashUnderSeconds = 10f;

    private bool timerStarted = false;
    private float roundDuration = 0f;
    private float currentTime = 0f;

    private bool timerFlashing = false;

    private void Update()
    {
        if (timerStarted)
        {
            if (currentTime > roundDuration)
            {
                Debug.Log("round finished");
                timerStarted = false;
                timerText.gameObject.GetComponent<FlashTextTween>().StopFlashContinuous();
            } else if (roundDuration - currentTime < flashUnderSeconds && !timerFlashing)
            {
                timerText.gameObject.GetComponent<FlashTextTween>().FlashContinuous();
                timerFlashing = true;
            }

            float remainingTimeInSeconds = roundDuration - currentTime;

            if (remainingTimeInSeconds > 0)
            {
                int minutes = Mathf.FloorToInt(remainingTimeInSeconds / 60F);
                int seconds = Mathf.FloorToInt(remainingTimeInSeconds - minutes * 60);

                timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            } else
            {
                timerText.text = "0:00";
            }

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
        int minutes = Mathf.FloorToInt(duration / 60F);
        int seconds = Mathf.FloorToInt(duration - minutes * 60);

        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        yield return new WaitForSeconds(freezeTime + 1);
        StartTimer(duration);
    }
}
