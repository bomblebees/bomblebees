using UnityEngine;

public class BombHelper : MonoBehaviour
{
    [SerializeField] public Sprite defaultBombIcon;
    [SerializeField] public Sprite blueBombIcon;
    [SerializeField] public Sprite greenBombIcon;
    [SerializeField] public Sprite yellowBombIcon;
    [SerializeField] public Sprite redBombIcon;
    [SerializeField] public Sprite purpleBombIcon;

    public Sprite GetKeySprite(char key)
    {
        switch (key)
        {
            case 'b': return blueBombIcon;
            case 'g': return greenBombIcon;
            case 'y': return yellowBombIcon;
            case 'r': return redBombIcon;
            case 'p': return purpleBombIcon;
            default: return defaultBombIcon;
        }
    }

    public static Color GetKeyColor(char key)
    {
        switch (key)
        {
            case 'g': return new Color32(101, 231, 0, 255);
            case 'y': return new Color32(97, 189, 255, 255); // ** for blue hex tile in game
            case 'r': return new Color32(255, 88, 88, 255);  
            case 'p': return new Color32(255, 216, 88, 255); // ** for yellow hex tile in game
            case 'w': return new Color32(178, 178, 178, 255);
            case 'e': return Color.white;
            default: return Color.white;
        }
    }

    public static string GetBombTextByKey(char key)
    {
        switch (key)
        {
            case 'b': return "Blink";
            case 'g': return "Plasma";
            case 'y': return "Laser";
            case 'r': return "Bomble";
            case 'p': return "Honey";
            case 'w': return "Queen Bee";
            default: return "None";
        }
    }
}
