using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;

    public delegate void ChangeCharacterDelegate();

    public event ChangeCharacterDelegate EventCharacterChanged;

    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }
}
