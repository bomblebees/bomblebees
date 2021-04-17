using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalScale;
    private bool tweenStarted = false;

    public float scaleMultipler = 1.1f;
    public LeanTweenType easeIn = LeanTweenType.easeOutExpo;
    public LeanTweenType easeOut = LeanTweenType.easeOutExpo;

    public void OnEnable()
    {
        originalScale = this.gameObject.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Prevents tween anim from running multiple times
        if (tweenStarted) return;

        tweenStarted = true;

        LeanTween.scale(this.gameObject, originalScale * scaleMultipler, 0.2f).setEase(easeIn);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.scale(this.gameObject, originalScale, 0.2f)
            .setEase(easeOut)
            .setOnComplete(OnTweenExitComplete);
    }

    public void OnTweenExitComplete()
    {
        tweenStarted = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerExit(eventData);
    }
}
