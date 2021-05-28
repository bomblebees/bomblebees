using TMPro;
using UnityEngine;

public class FatalErrorScreen : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text reason;

    public void Show(string titleText, string reasonText)
    {
        title.text = titleText;
        reason.text = reasonText;
        popup.SetActive(true);
    }
    
    public void Show(string reasonText)
    {
        title.text = "Fatal Error";
        reason.text = reasonText;
        popup.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
