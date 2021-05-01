using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorTween : MonoBehaviour
{
    public Color targetColor;
    public Color endColor;

    public float easeInTime;
    public float easeOutTime;

    public float timeBetweenLoop;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    public bool startOnEnabled = false;

    private void OnEnable()
    {
        StartTween();
    }

    public void StartTween()
    {
        LeanTween.value(gameObject, updateColorCallback, endColor, targetColor, easeInTime)
            .setEase(easeIn)
            .setOnComplete(EndTween);
    }

    public void EndTween()
    {
        LeanTween.value(gameObject, updateColorCallback, targetColor, endColor, easeOutTime).setEase(easeOut);
    }

    private bool loopStarted = false;

    public void LoopTween()
    {
        if (loopStarted) return;

        loopStarted = true;

        LeanTween.delayedCall(gameObject, timeBetweenLoop, () => {

            LeanTween.value(gameObject, updateColorCallback, endColor, targetColor, easeInTime)
                .setEase(easeIn)
                .setLoopPingPong(1);

        }).setRepeat(-1);

    }

    void updateColorCallback(Color val)
    {
        if (GetComponent<Image>()) GetComponent<Image>().color = val;
        if (GetComponent<RawImage>()) GetComponent<RawImage>().color = val;
        if (GetComponent<TMP_Text>()) gameObject.GetComponent<TMP_Text>().color = val;
    }
}
