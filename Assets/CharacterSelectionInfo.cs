using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;

    public delegate void CharacterChange();

    public event CharacterChange EventCharacterChanged;

    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }
}
