using UnityEngine;

public class DontDestroyMatchmaking : MonoBehaviour
{
    // 現在存在しているオブジェクト実体の記憶領域
    static DontDestroyMatchmaking _instance = null;

    // オブジェクト実体の参照（初期参照時、実体の登録も行う）
    static DontDestroyMatchmaking instance
    {
        get { return _instance ?? (_instance = FindObjectOfType<DontDestroyMatchmaking>()); }
    }

    void Awake()
    {

        // ※オブジェクトが重複していたらここで破棄される

        // 自身がインスタンスでなければ自滅
        if (this != instance)
        {
            Destroy(gameObject);
            return;
        }

        // 以降破棄しない
        DontDestroyOnLoad(gameObject);

    }

    public void OnDestroy()
    {

        // ※破棄時に、登録した実体の解除を行なっている

        // 自身がインスタンスなら登録を解除
        if (this == instance) _instance = null;

        Matchmaking matchmaking = GetComponent<Matchmaking>();
        /*
        matchmaking.lobbyVersion = null;

        //matchmaking.currentLobby = null;
        //matchmaking.lobbyOwner = null;
        //matchmaking.isLobbyHost = null;
        matchmaking.lobbyPlayers = null;
        //matchmaking.lobbyPanels = null;
        matchmaking.myPlayer = null;

        matchmaking.LobbyCanvas =null;

        matchmaking.lobbies = null;
        matchmaking.lobbyRowPrefab =null;
        matchmaking.scrollViewContent = null;
        matchmaking.lobbyListPanel = null;
        matchmaking.createLobbyWindow = null;
        matchmaking.inputLobbyName = null;
        matchmaking.txtMaxLobbyPlayers = null;
        matchmaking.sliderMaxPlayers = null;
        matchmaking.lobbyPanel = null;
        //matchmaking.device = null;

        //matchmaking.portX = 7777;
        //matchmaking.isStartgame = null;

        //matchmaking.NManager = null;
        */
        Destroy(matchmaking);

    Destroy(gameObject);
    }
}
