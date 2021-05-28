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
        HideAll();
        
        switch (numberOfKills)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                doubleKill.SetActive(true);
                break;
            case 3:
                tripleKill.SetActive(true);
                break;
            case 4:
                quadrupleKill.SetActive(true);
                break;
            default:
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
