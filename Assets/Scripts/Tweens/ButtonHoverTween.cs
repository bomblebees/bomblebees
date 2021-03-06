using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalScale;

    public float scaleMultipler = 1.1f;
    public LeanTweenType easeIn = LeanTweenType.easeOutExpo;
    public LeanTweenType easeOut = LeanTweenType.easeOutExpo;

    public void OnEnable()
    {
        originalScale = this.gameObject.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.scale(this.gameObject, originalScale * scaleMultipler, 0.2f).setEase(easeIn);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.scale(this.gameObject, originalScale, 0.2f).setEase(easeOut);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerExit(eventData);
    }
}
