using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SyncVar] public int player1;
    [SyncVar] public int player2;
    [SyncVar] public int player3;
    [SyncVar] public int player4;
    
    [SerializeField] public Texture2D[] characterCardList;

    public delegate void CharacterChanged();

    public event CharacterChanged EventCharacterChanged;

    public void ChangePlayer1()
    {
        player1 = (player1 + 1) % 4;
        EventCharacterChanged?.Invoke();
    }
    
    public void ChangePlayer2()
    {
        player2 = (player2 + 1) % 4;
        EventCharacterChanged?.Invoke();
    }
    
    public void ChangePlayer3()
    {
        player3 = (player3 + 1) % 4;
        EventCharacterChanged?.Invoke();
    }
    
    public void ChangePlayer4()
    {
        player4 = (player4 + 1) % 4;
        EventCharacterChanged?.Invoke();
    }
}
