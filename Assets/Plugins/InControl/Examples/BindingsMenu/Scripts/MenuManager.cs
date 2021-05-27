using UnityEngine;
using UnityEngine.EventSystems;
using InControl;


// For the purposes of this example, this class sets up the bindings menu
// and also houses the menu and game action sets. In a real scenario,
// these would probably be separate responsibilities, and the menu management
// would likely be somewhat more complex.
public class MenuManager : MonoBehaviour
{
    public MenuActions MenuActions { get; private set; }
    public GameActions GameActions { get; private set; }

    public BindingsMenu BindingsMenu;

    private string _gameActionsSaveData;
    private string _menuActionsSaveData;


    void Awake()
    {
        InputManager.OnSetup -= SetupInput;
        InputManager.OnSetup += SetupInput;

        InputManager.OnReset -= ResetInput;
        InputManager.OnReset += ResetInput;
    }


    void SetupInput()
    {
        // Create the action sets for our menu and game controls.
        MenuActions = MenuActions.CreateWithDefaultBindings();
        GameActions = GameActions.CreateWithDefaultBindings();

        LoadBindings();

        // Get the InControlInputModule, and attach menu navigation controls.
        var inputModule = EventSystem.current.GetComponent<InControlInputModule>();
        Debug.Assert(inputModule != null, "inputModule != null");
        if (inputModule != null)
        {
            inputModule.MoveAction = MenuActions.Move;
            inputModule.SubmitAction = MenuActions.Submit;
            inputModule.CancelAction = MenuActions.Cancel;
        }
    }

    public void ShowGameActions()
    {
        BindingsMenu.Hide();
        BindingsMenu.Show(GameActions);
    }


    void ResetInput()
    {
        if (MenuActions != null)
        {
            MenuActions.Destroy();
            MenuActions = null;
        }

        if (GameActions != null)
        {
            GameActions.Destroy();
            GameActions = null;
        }
    }


    #region Basic singleton implementation.

    static MenuManager instance;
    static bool applicationIsQuitting;
    static readonly object lockObject = new object();


    // This project uses the experimental fast enter play mode options with domain reloading disabled,
    // so we need to reinitialize our static variables.
    // https://docs.unity3d.com/Manual/DomainReloading.html
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        instance = null;
        applicationIsQuitting = false;
    }


    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
        applicationIsQuitting = true;
        instance = null;
    }

    public void SaveBindings()
    {
        _gameActionsSaveData = GameActions.Save();
        PlayerPrefs.SetString("Bindings: Game Actions", _gameActionsSaveData);

        _menuActionsSaveData = MenuActions.Save();
        PlayerPrefs.SetString("Bindings: Menu Actions", _menuActionsSaveData);
    }


    private void LoadBindings()
    {
        if (PlayerPrefs.HasKey("Bindings: Game Actions"))
        {
            _gameActionsSaveData = PlayerPrefs.GetString("Bindings: Game Actions");
            GameActions.Load(_gameActionsSaveData);
        }

        // if (PlayerPrefs.HasKey("Bindings: Menu Actions"))
        // {
        // 	_menuActionsSaveData = PlayerPrefs.GetString("Bindings: Menu Actions");
        // 	MenuActions.Load(_menuActionsSaveData);
        // }
    }

    public static MenuManager Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(MenuManager)) as MenuManager;
                }

                return instance;
            }
        }
    }

    #endregion
}