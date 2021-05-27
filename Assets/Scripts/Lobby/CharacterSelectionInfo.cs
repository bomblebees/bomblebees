using Mirror;
using UnityEngine;

public class CharacterSelectionInfo : NetworkBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;

    public delegate void ChangeCharacterDelegate();
    public event ChangeCharacterDelegate EventCharacterChanged;

    public bool[] characterAvailable = { true, true, true, true };

    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }

    public void ResetAll()
    {
        for (int i = 0; i < characterAvailable.Length; i++)
        {
            characterAvailable[i] = true;
        }
    }
}
