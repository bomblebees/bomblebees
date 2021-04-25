using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SyncVar] public int player1;
    [SyncVar] public int player2;
    [SyncVar] public int player3;
    [SyncVar] public int player4;
    
    [SerializeField] public Texture2D[] characterCardList;

    public delegate void TogglePlayer1();
    public delegate void TogglePlayer2();
    public delegate void TogglePlayer3();
    public delegate void TogglePlayer4();
    
    public event TogglePlayer1 EventTogglePlayer1;
    public event TogglePlayer2 EventTogglePlayer2;
    public event TogglePlayer3 EventTogglePlayer3;
    public event TogglePlayer4 EventTogglePlayer4;

    public void OnEventTogglePlayer1()
    {
        player1 = (player1 + 1) % 4;
        EventTogglePlayer1?.Invoke();
    }
    
    public void OnEventTogglePlayer2()
    {
        player2 = (player2 + 1) % 4;
        EventTogglePlayer2?.Invoke();
    }
    
    public void OnEventTogglePlayer3()
    {
        player3 = (player3 + 1) % 4;
        EventTogglePlayer3?.Invoke();
    }
    
    public void OnEventTogglePlayer4()
    {
        player4 = (player4 + 1) % 4;
        EventTogglePlayer4?.Invoke();
    }
}
