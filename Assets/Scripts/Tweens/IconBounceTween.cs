using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconBounceTween : MonoBehaviour
{
    private Vector3 originalScale;
    private bool tweenStarted = false;

    public float scaleMultipler = 1.1f;

    public float easeInTime = 0.1f;
    public float easeOutTime = 0.15f;

    public LeanTweenType easeIn = LeanTweenType.easeOutCubic;
    public LeanTweenType easeOut = LeanTweenType.easeOutExpo;

    public void OnEnable()
    {
        originalScale = this.gameObject.transform.localScale;
    }

    public void OnTweenStart()
    {
        // Prevents tween anim from running multiple times
        if (tweenStarted) return;

        tweenStarted = true;

        LeanTween.scale(this.gameObject, originalScale * scaleMultipler, easeInTime)
            .setEase(easeIn)
            .setOnComplete(OnTweenExit);

        //this.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
    }

    public void OnTweenExit()
    {
        LeanTween.scale(this.gameObject, originalScale, easeOutTime)
            .setEase(easeOut)
            .setOnComplete(OnTweenExitComplete);
    }

    public void OnTweenExitComplete()
    {
        tweenStarted = false;
    }
}
