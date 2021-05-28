using UnityEngine;
using UnityEngine.EventSystems;

public class PracticeButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        //
        //FindObjectOfType<Matchmaking>().CreateLobby();
        //InvokeRepeating(nameof(TryToStartGame), float.Epsilon, 0.1f);

        StartGame();
    }

    //private void TryToStartGame()
    //{
    //    if (FindObjectOfType<NetworkRoomPlayerExt>() is null) return;
        
    //    if (FindObjectOfType<LobbySettings>() is null || FindObjectOfType<NetworkRoomManagerExt>() is null)
    //    {
    //        Debug.LogWarning("This should never be called");
    //        return;
    //    }
        
    //    CancelInvoke(nameof(TryToStartGame));
    //    StartGame();
    //}

    private void StartGame()
    {
        // Turn on loading screen
        FindObjectOfType<GlobalLoadingScreen>().gameObject.GetComponent<Canvas>().enabled = true;

        // Start host
        NetworkRoomManagerExt networkManager = FindObjectOfType<NetworkRoomManagerExt>();
        networkManager.StartHost();

        // Turn practice mode on
        LobbySettings lobbySettings = FindObjectOfType<LobbySettings>();
        lobbySettings.practiceMode = true;
    }
}
