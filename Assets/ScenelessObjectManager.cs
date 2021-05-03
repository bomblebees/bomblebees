using UnityEngine;

public class ScenelessObjectManager : MonoBehaviour
{
    [Header("Configure")] 
    [SerializeField] private float settingButtonOpacity = 1f;
    [Header("Others")]
    [SerializeField] private GlobalSettings globalSettings;
    [SerializeField] private GameObject settingButton;
    private CanvasRenderer[] _canvasRenderers;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _canvasRenderers = settingButton.GetComponentsInChildren<CanvasRenderer>();

        foreach (var canvasRenderer in _canvasRenderers)
        {
            canvasRenderer.SetAlpha(settingButtonOpacity);
        }
    }

    private void Update()
    {
        if (KeyBindingManager.GetKeyUp(KeyAction.ToggleSettings))
        {
            ToggleSettingsScreen();
        }
    }

    public void ToggleSettingsScreen()
    {
        globalSettings.gameObject.SetActive(!globalSettings.gameObject.activeSelf);
    }
}
