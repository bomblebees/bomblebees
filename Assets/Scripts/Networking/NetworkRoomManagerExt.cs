using System.Linq;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkRoomManagerExt : NetworkRoomManager
{
    [SerializeField] private GameObject hexGrid;
    [SerializeField] private GameObject roundManager;
    [SerializeField] private GameObject audioManager;
    /// <summary>
    /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
    /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
    /// into the GamePlayer object as it is about to enter the Online scene.
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="gamePlayer"></param>
    /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        //PlayerScore playerScore = gamePlayer.GetComponent<PlayerScore>();
        //playerScore.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;
        return true;
    }

    public override void OnRoomStopClient()
    {
        // Enable lobby list
        Matchmaking mm = Matchmaking.singleton;
        mm.uiLeaveLobby();

        base.OnRoomStopClient();
    }

    public override void OnRoomStopServer()
    {
        base.OnRoomStopServer();
    }

    /*
        This code below is to demonstrate how to do a Start button that only appears for the Host player
        showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
        all players are ready, but if a player cancels their ready state there's no callback to set it back to false
        Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
        Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
        is set as DontDestroyOnLoad = true.
    */

    public bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
#if UNITY_SERVER
        base.OnRoomServerPlayersReady();
#else
        showStartButton = true;
#endif
    }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        if (sceneName == GameplayScene)
        {
            Debug.Log("OnRoomServerSceneChanged");
            NetworkServer.Spawn(Instantiate(hexGrid));
            NetworkServer.Spawn(Instantiate(roundManager));
            NetworkServer.Spawn(Instantiate(audioManager));
        }
    }
        
    public override void OnStartServer()
    {
        base.OnStartServer();
 
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
    }
        
    public override void OnStartClient()
    {
        base.OnStartClient();
            
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Prefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnGUI()
    {
        Matchmaking mm = Matchmaking.singleton;
        if (mm) return;

        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }
}
