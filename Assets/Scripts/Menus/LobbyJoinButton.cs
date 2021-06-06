using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyJoinButton : MonoBehaviour
{
    private GlobalButtonSettings _globalButtonSettings;
    private Button _button;
    private List<CanvasRenderer> _canvasRenderers;
    private ButtonHoverTween _buttonHoverTween;

    private void Awake()
    {
        _globalButtonSettings = FindObjectOfType<GlobalButtonSettings>();
        _button = GetComponentInChildren<Button>();
        _canvasRenderers = new List<CanvasRenderer>(GetComponentsInChildren<CanvasRenderer>());
        _buttonHoverTween = GetComponentInChildren<ButtonHoverTween>();
        
        DeactivateButton();
    }

    public void DeactivateButton()
    {
        if (_button == null) return;
        _button.interactable = false;
        _globalButtonSettings.DeactivateButtonOpacity(_canvasRenderers);
        if (!(_buttonHoverTween is null))
        {
            _buttonHoverTween.enabled = false;
            _buttonHoverTween.gameObject.transform.localScale.Set(1f, 1f, 1f);
        }
    }

    public void ActivateButton()
    {
        if (_button == null) return;
        _button.interactable = true;
        _globalButtonSettings.ActivateButtonOpacity(_canvasRenderers);
        if (!(_buttonHoverTween is null))
        {
            _buttonHoverTween.enabled = true;
            _buttonHoverTween.gameObject.transform.localScale.Set(1f, 1f, 1f);
        }
    }
}
