using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LivesUIElement : MonoBehaviour
{
    [SerializeField] public GameObject livesObject;
    [SerializeField] public Image avatar;
    [SerializeField] public RawImage background;
    [SerializeField] public GameObject heartsObject;
    [SerializeField] public GameObject[] hearts;
    [SerializeField] public TMP_Text playerName;
    [SerializeField] public TMP_Text livesCounter;
    [SerializeField] public GameObject eliminationsObject;
    [SerializeField] public TMP_Text elimsText;
    [SerializeField] public GameObject combosObject;
    [SerializeField] public TMP_Text combosText;
    public GameObject player; // the player gameobject that owns this lives elem
}
