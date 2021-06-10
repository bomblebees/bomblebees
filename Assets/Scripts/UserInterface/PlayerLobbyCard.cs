using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyCard : MonoBehaviour
{
    public GameObject playerCard;
    public RawImage avatar;
    public TMP_Text username;
    public GameObject readyStatus;
    public RawImage characterPortrait;
    public Button changeCharacterButton;
    public ButtonHoverTween changeCharacterButtonHoverTween;
    public Image[] colorFrames;
    public TMP_Text pingDisplay;
    public GameObject crown;
    public TMP_Text disabledText;
    public Button changeTeamButton;
}
