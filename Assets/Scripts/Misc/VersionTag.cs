using TMPro;
using UnityEngine;

public class VersionTag : MonoBehaviour
{
    [SerializeField] private TMP_Text versionText;
    // Start is called before the first frame update
    void Start()
    {
        versionText.text = "v" + Application.version;
    }
}
