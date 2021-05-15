using UnityEngine;

public class CharacterSelectionInfo : MonoBehaviour
{
    [SerializeField] public Texture2D[] characterPortraitList;

    public delegate void ChangeCharacterDelegate();
    public event ChangeCharacterDelegate EventCharacterChanged;

    public bool[] characterAvailable = { true, true, true, true };
    private bool[] _defaultCharacterAvailable;

    private void Start()
    {
        _defaultCharacterAvailable = characterAvailable;
    }

    public void OnChangeCharacter()
    {
        EventCharacterChanged?.Invoke();
    }

    public void ResetAvailability()
    {
        Debug.Log("Character availability has been reset.");
        characterAvailable = _defaultCharacterAvailable;
    }
}
