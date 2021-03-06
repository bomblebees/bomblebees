using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconBounceTween : MonoBehaviour
{
    private Vector3 originalScale;

    public float scaleMultipler = 1.1f;
    public LeanTweenType easeIn = LeanTweenType.easeOutCubic;
    public LeanTweenType easeOut = LeanTweenType.easeOutExpo;

    public void OnEnable()
    {
        originalScale = this.gameObject.transform.localScale;
    }

    public void OnTweenStart()
    {
        LeanTween.scale(this.gameObject, originalScale * scaleMultipler, 0.1f)
            .setEase(easeIn)
            .setOnComplete(OnTweenExit);

        //this.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
    }

    public void OnTweenExit()
    {
        LeanTween.scale(this.gameObject, originalScale, 0.15f).setEase(easeOut);
    }
}
