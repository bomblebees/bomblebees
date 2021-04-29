using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaTween : MonoBehaviour
{
    public float targetAlpha = 0.7f;
    public float endAlpha = 0f;

    public float easeInTime = 0.05f;
    public float easeOutTime = 0.4f;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    public void StartTween()
    {
        LeanTween.alpha(this.gameObject.GetComponent<RectTransform>(), targetAlpha, 0.05f)
            .setEase(easeIn)
            .setOnComplete(EndTween);
    }

    public void EndTween()
    {
        LeanTween.alpha(this.gameObject.GetComponent<RectTransform>(), endAlpha, easeOutTime).setEase(easeOut);
    }
}
