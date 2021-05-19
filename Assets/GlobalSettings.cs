using TMPro;
using UnityEngine;
using Mirror;

public class GlobalSettings : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas keyBindings;
    [SerializeField] private Canvas keyBindings2;
    
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
        CheckFullScreenMode();
    }

    #region FullScreenMode

    private void CheckFullScreenMode()
    {
        _fullScreenModeIndex = (int) Screen.fullScreenMode;
        UpdateFullScreenText();
    }

    public void ToggleFullScreenMode()
    {
        _fullScreenModeIndex = (_fullScreenModeIndex + 1) % 4;
        Screen.fullScreenMode = _fullScreenModes[_fullScreenModeIndex];
        UpdateFullScreenText();
    }

    private void UpdateFullScreenText()
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
    
    #endregion

    public void ToggleKeyBindingsCanvas()
    {
        mainCanvas.enabled = !mainCanvas.enabled;
        keyBindings.enabled = !keyBindings.enabled;
    }
    
    public void ToggleKeyBindings2Canvas()
    {
        mainCanvas.enabled = !mainCanvas.enabled;
        keyBindings2.enabled = !keyBindings2.enabled;
    }

    public void OnClickQuitToMenu()
    {
        NetworkRoomManagerExt networkManager = NetworkManager.singleton as NetworkRoomManagerExt;

        if (networkManager)
        {
            if (NetworkServer.active) networkManager.StopHost();
            else networkManager.StopClient();
        }

    }

    public void OnClickQuitToDesktop()
    {
        Application.Quit();
    }
}

