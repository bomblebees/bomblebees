using UnityEngine;
using UnityEngine.UI;

public class ButtonDisable : MonoBehaviour
{
    [Header("Button Opacity Settings")]
    [Range(0f, 1f)] [SerializeField] private float deactivatedOpacity = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float activatedOpacity = 1f;

    private AudioEvents sfx;
    private Button button;
    private ButtonHoverTween tween;
    private CanvasRenderer[] canvasRenderers;

    private void Awake()
    {
        button = this.GetComponent<Button>();
        if (!button) Debug.LogError("Could not find component: Button");

        sfx = this.GetComponent<AudioEvents>();
        if (!sfx) Debug.LogError("Could not find component: AudioEvents");

        tween = this.GetComponent<ButtonHoverTween>();
        if (!tween) Debug.LogError("Could not find component: ButtonHoverTween");

        canvasRenderers = this.GetComponentsInChildren<CanvasRenderer>();
        if (canvasRenderers == null) Debug.LogError("Could not find component: CanvasRenderer");
    }

    public void EnableButton()
    {
        // Make button clickable
        button.interactable = true;

        // Enable sfx
        sfx.enabled = true;

        // Enable hover tween
        tween.enabled = true;

        // Set alpha of all canvas renderers back to normal
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(activatedOpacity);
        }
    }

    public void DisableButton()
    {
        // Make button unclickable
        button.interactable = false;

        // Disable sfx
        sfx.enabled = false;

        // Disable hover tween
        tween.enabled = false;

        // Set alpha of all canvas renderers to transparent
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(deactivatedOpacity);
        }
    }
}
