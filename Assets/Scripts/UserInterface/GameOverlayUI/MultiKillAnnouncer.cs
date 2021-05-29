using UnityEngine;

public class MultiKillAnnouncer : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] private GameObject doubleKill;
    [SerializeField] private GameObject tripleKill;
    [SerializeField] private GameObject quadrupleKill;
    [SerializeField] private GameObject multiKill;

    private void Awake()
    {
        HideAll();
    }

    public void Show(int numberOfKills)
    {
        switch (numberOfKills)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                HideAll();
                doubleKill.SetActive(true);
                break;
            case 3:
                HideAll();
                tripleKill.SetActive(true);
                break;
            case 4:
                HideAll();
                quadrupleKill.SetActive(true);
                break;
            default:
                HideAll();
                multiKill.SetActive(true);
                break;
        }
    }

    private void HideAll()
    {
        doubleKill.SetActive(false);
        tripleKill.SetActive(false);
        quadrupleKill.SetActive(false);
        multiKill.SetActive(false);
    }
}
