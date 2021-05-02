using UnityEngine;

public class ScenelessObjectManager : MonoBehaviour
{
    [SerializeField] private GlobalSettings globalSettings;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
