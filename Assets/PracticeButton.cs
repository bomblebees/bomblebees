using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PracticeButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<Matchmaking>().CreateLobby();
        InvokeRepeating(nameof(TryToStartGame), Single.Epsilon, 0.1f);
    }

    private void TryToStartGame()
    {
        if (FindObjectOfType<NetworkRoomPlayerExt>() is null) return;
        
        if (FindObjectOfType<LobbySettings>() is null || FindObjectOfType<NetworkRoomManagerExt>() is null)
        {
            Debug.LogWarning("This should never be called");
            return;
        }
        
        CancelInvoke(nameof(TryToStartGame));
        StartGame();
    }

    private void StartGame()
    {
        var lobbySettings = FindObjectOfType<LobbySettings>();
        lobbySettings.roundDuration = 0f;
        lobbySettings.playerLives = 0;
        
        var networkRoomManagerExt = FindObjectOfType<NetworkRoomManagerExt>();
        networkRoomManagerExt.ServerChangeScene(networkRoomManagerExt.GameplayScene);
    }
}
