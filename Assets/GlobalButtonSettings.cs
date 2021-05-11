using System.Collections.Generic;
using UnityEngine;

public class GlobalButtonSettings : MonoBehaviour
{
    [Header("Button Opacity Settings")] 
    [Range(0f, 1f)] [SerializeField] private float deactivatedOpacity = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float activatedOpacity = 1f;

    public void DeactivateButton(List<CanvasRenderer> canvasRenderers)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(deactivatedOpacity);
        }
    }

    public void DeactivateButton(CanvasRenderer[] canvasRenderers)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(deactivatedOpacity);
        }
    }
    
    public void ActivateButton(List<CanvasRenderer> canvasRenderers)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(activatedOpacity);
        }
    }
    
    public void ActivateButton(CanvasRenderer[] canvasRenderers)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(activatedOpacity);
        }
    }
}
