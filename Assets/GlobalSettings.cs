using TMPro;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas keyBindingsCanvas;
    
    [Header("Others")]
    [SerializeField] private TMP_Text textFullscreen;
    
    private void Start()
    {
        UpdateFullscreenText();
    }

    public void ToggleFullScreenMode()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.Windowed:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                break;
            case FullScreenMode.MaximizedWindow:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case FullScreenMode.ExclusiveFullScreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case FullScreenMode.FullScreenWindow:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
        UpdateFullscreenText();
    }

    private void UpdateFullscreenText()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.Windowed:
                textFullscreen.text = "Fullscreen: Windowed";
                break;
            case FullScreenMode.MaximizedWindow:
                textFullscreen.text = "Fullscreen: Maximized Window";
                break;
            case FullScreenMode.ExclusiveFullScreen:
                textFullscreen.text = "Fullscreen: Exclusive Fullscreen";
                break;
            case FullScreenMode.FullScreenWindow:
                textFullscreen.text = "Fullscreen: Fullscreen Window";
                break;
            default:
                textFullscreen.text = "Fullscreen: Error";
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

