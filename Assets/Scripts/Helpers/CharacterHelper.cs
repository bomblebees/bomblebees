using UnityEngine;

public class CharacterHelper : MonoBehaviour
{
    [SerializeField] public Sprite[] heartColors = new Sprite[4];
    [SerializeField] public Sprite[] characterPictures = new Sprite[4];

    public Sprite GetLivesImage(int code)
    {
        return heartColors[code];
    }

    public Sprite GetCharImage(int code)
    {
        return characterPictures[code];
    }
}
