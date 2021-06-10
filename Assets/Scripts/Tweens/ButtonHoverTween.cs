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

    private int cur = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Prevents tween anim from running multiple times
        if (tweenStarted) return;

        tweenStarted = true;

        cur = LeanTween.scale(this.gameObject, originalScale * scaleMultipler, 0.2f).setEase(easeIn).id;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cur = LeanTween.scale(this.gameObject, originalScale, 0.2f)
            .setEase(easeOut)
            .setOnComplete(OnTweenExitComplete).id;
    }

    public void OnDisable()
    {
        LeanTween.cancel(cur);

        this.gameObject.transform.localScale = originalScale;
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
