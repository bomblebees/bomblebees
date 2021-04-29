using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;
    
    [Header("Character Availability Info")]
    [SerializeField] private GameObject characterAvailabilityInfo;

    public delegate void ChangeCharacterDelegate();
    public event ChangeCharacterDelegate EventCharacterChanged;

    private void Start()
    {
        InstantiateCharacterAvailabilityInfo();
    }

    [Server]
    private void InstantiateCharacterAvailabilityInfo()
    {
        NetworkServer.Spawn(Instantiate(characterAvailabilityInfo));
    }
    
    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }
}
