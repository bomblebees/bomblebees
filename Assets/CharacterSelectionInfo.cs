using System;
using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;
    
    [Header("Character Availability Info")]
    [SerializeField] private GameObject characterAvailabilityInfo;
    public CharacterAvailabilityInfo CharacterAvailabilityInfo;

    private void Start()
    {
        NetworkServer.Spawn(Instantiate(characterAvailabilityInfo));
        
        CharacterAvailabilityInfo = FindObjectOfType<CharacterAvailabilityInfo>();
    }

    public delegate void ChangeCharacterDelegate();

    public event ChangeCharacterDelegate EventCharacterChanged;

    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }
}
