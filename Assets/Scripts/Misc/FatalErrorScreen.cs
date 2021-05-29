using TMPro;
using UnityEngine;

public class FatalErrorScreen : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    public void Show(string errorTitle, string errorDescription)
    {
        title.text = errorTitle;
        description.text = errorDescription;
        popup.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
