using UnityEngine;

public class FadeCanvas : MonoBehaviour
{

    private CanvasRenderer[] canvasRenderers;

    public float delay;
    public float duration;

    private void Awake()
    {
        canvasRenderers = this.GetComponentsInChildren<CanvasRenderer>();
    }

    private void Start()
    {
        SetCanvasAlpha(0);
    }

    public void SetCanvasAlpha(float alpha)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(alpha);
        }
    }

    public void StartFadeIn()
    {
        LeanTween.value(this.gameObject, SetCanvasAlpha, 0f, 1f, duration)
            .setDelay(delay);
            //.setEase(LeanTweenType.linear);
    }
}
