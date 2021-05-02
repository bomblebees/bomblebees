using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class RoundTimer : MonoBehaviour
{
    [Header("Start Round")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerRing;
    [SerializeField] private float warnUnderSeconds = 30f;
    [SerializeField] private float countdownUnderSeconds = 10f;

    [Header("LowTimeFeedback")]
    [SerializeField] private TMP_Text remainingText;
    [SerializeField] private TMP_Text countdownText;

    // Whether the timer has started or not
    private bool timerStarted = false;

    // The time when the round ends (pre calculated on start)
    private float endTime;

    // The time right when the round begins
    private float startTime;

    // The current time
    private float currentTime;

    private bool warnStarted = false;
    private bool countdownStarted = false;

    private void Update()
    {
        if (timerStarted)
        {
            currentTime = (float)NetworkTime.time;

            float remainingTimeInSeconds = endTime - currentTime;
            int minutesLeft = Mathf.FloorToInt(remainingTimeInSeconds / 60F);
            int secondsLeft = Mathf.FloorToInt(remainingTimeInSeconds - minutesLeft * 60);

            // Conditionals
            if (remainingTimeInSeconds <= 0)
            {
                // When the round ends
                timerStarted = false;

                // End flashing
                timerText.gameObject.GetComponent<ColorTween>().EndLoopTween();
            } else if (remainingTimeInSeconds <= warnUnderSeconds + 1 && !warnStarted)
            {
                // Warn the player that the timer is ticking down
                timerText.gameObject.GetComponent<ColorTween>().LoopTween();
                remainingText.gameObject.SetActive(true);
                warnStarted = true;
            } else if (remainingTimeInSeconds <= countdownUnderSeconds + 1)
            {
                // Start the countdown after set time
                countdownText.gameObject.SetActive(true);

                if (!countdownStarted) countdownText.text = secondsLeft.ToString();

                if (countdownText.text != secondsLeft.ToString())
                {
                    countdownText.text = secondsLeft.ToString();
                    countdownText.GetComponent<SimpleAnimation.TweenSequence>().StopAllCoroutines();
                    countdownText.GetComponent<SimpleAnimation.TweenSequence>().PlayScaleAnimations();
                }

                countdownStarted = true;
            }


            // Update the timer
            if (remainingTimeInSeconds > 0)
            {
                timerText.text = string.Format("{0:0}:{1:00}", minutesLeft, secondsLeft);
            } else
            {
                timerText.text = "0:00";
            }

            timerRing.fillAmount = (currentTime - startTime) / (endTime - startTime);
        }
    }

    public void StartTimer(float duration)
    {
        // Set start and end times
        startTime = (float)NetworkTime.time;
        endTime = (float)NetworkTime.time + duration;
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
