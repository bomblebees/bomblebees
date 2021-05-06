using TMPro;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas keyBindingsCanvas;
    
    [Header("Others")]
    [SerializeField] private TMP_Text textFullscreen;
    
    private int _fullScreenModeIndex;
    private readonly FullScreenMode[] _fullScreenModes = {
        FullScreenMode.Windowed, 
        FullScreenMode.MaximizedWindow, 
        FullScreenMode.ExclusiveFullScreen, 
        FullScreenMode.FullScreenWindow};
    
    private void Start()
    {
        // Start with windowed mode
        _fullScreenModeIndex = 0;
        Screen.fullScreenMode = _fullScreenModes[_fullScreenModeIndex];
        UpdateFullscreenText();
    }

    public void ToggleFullScreenMode()
    {
        _fullScreenModeIndex = (_fullScreenModeIndex + 1) % 4;
        Screen.fullScreenMode = _fullScreenModes[_fullScreenModeIndex];
        UpdateFullscreenText();
    }

    private void UpdateFullscreenText()
    {
        switch (_fullScreenModeIndex)
        {
            case 0:
                textFullscreen.text = "Fullscreen: Windowed";
                break;
            case 1:
                textFullscreen.text = "Fullscreen: Maximized Window";
                break;
            case 2:
                textFullscreen.text = "Fullscreen: Exclusive Fullscreen";
                break;
            case 3:
                textFullscreen.text = "Fullscreen: Fullscreen Window";
                break;
        }
    }

    public void ToggleKeyBindingsCanvas()
    {
        mainCanvas.enabled = !mainCanvas.enabled;
        keyBindingsCanvas.enabled = !keyBindingsCanvas.enabled;
    }

    public void OnClickQuitGameButton()
    {
        Application.Quit();
    }
}

