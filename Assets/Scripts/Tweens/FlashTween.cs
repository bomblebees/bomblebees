using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashTween : MonoBehaviour
{
    public float flashAlpha = 0.7f;
    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    public void StartFlash()
    {
        LeanTween.alpha(this.gameObject.GetComponent<RectTransform>(), flashAlpha, 0.05f)
            .setEase(easeIn)
            .setOnComplete(EndFlash);
    }

    public void EndFlash()
    {
        LeanTween.alpha(this.gameObject.GetComponent<RectTransform>(), 0f, 0.4f).setEase(easeOut);
    }
}
