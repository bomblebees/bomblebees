using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlphaTextTween : MonoBehaviour
{
    public float targetAlpha = 0.7f;
    public float endAlpha = 0.7f;

    public float initialDelay = 0f;

    public float easeInTime = 0.1f;
    public float easeOutTime = 0.15f;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    private Color originalColor;
    TMP_Text text;

    public void OnEnable()
    {
        text = gameObject.GetComponent<TMP_Text>();
        originalColor = this.gameObject.GetComponent<TMP_Text>().color;
    }

    public void StartTween()
    {
        LeanTween.value(gameObject, updateColorCallback, endAlpha, targetAlpha, easeInTime)
            .setEase(easeIn)
            .setDelay(initialDelay)
            .setOnComplete(EndTween);
    }

    public void EndTween()
    {
        LeanTween.value(gameObject, updateColorCallback, targetAlpha, endAlpha, easeOutTime).setEase(easeOut);
    }

    void updateColorCallback(float val)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, val);
    }
}
