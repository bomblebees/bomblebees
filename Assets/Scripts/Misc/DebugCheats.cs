using UnityEngine;

public class DebugCheats : MonoBehaviour
{
    [SerializeField] private bool enableCheats;

    private bool uiToggled = true;
    private bool playerUIToggled = true;

    private void Start()
    {
        if (enableCheats.Equals(false)) Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.B)) BCheats();
        if (Input.GetKey(KeyCode.C)) CCheats();
    }

    private void BCheats()
    {
        // Cheat for increasing inventory size
        if (Input.GetKeyDown(KeyCode.Alpha1)) FindObjectOfType<RoundManager>().IncreaseGlobalLivesAmt();
        // Player cheats
        if (Input.GetKeyDown(KeyCode.Alpha6))
            GameObject.Find("LocalPlayer").GetComponent<PlayerBombPlace>().CmdSpawnBomb('r');
        if (Input.GetKeyDown(KeyCode.Alpha7))
            GameObject.Find("LocalPlayer").GetComponent<PlayerBombPlace>().CmdSpawnBomb('p');
        if (Input.GetKeyDown(KeyCode.Alpha8))
            GameObject.Find("LocalPlayer").GetComponent<PlayerBombPlace>().CmdSpawnBomb('y');
        if (Input.GetKeyDown(KeyCode.Alpha9))
            GameObject.Find("LocalPlayer").GetComponent<PlayerBombPlace>().CmdSpawnBomb('g');
        if (Input.GetKeyDown(KeyCode.Alpha0)) GameObject.Find("LocalPlayer").GetComponent<Health>().CmdDropItems();
    }

    private void CCheats()
    {
        // Multi kill announcer cheats
        if (Input.GetKeyDown(KeyCode.Alpha1)) FindObjectOfType<MultiKillAnnouncer>().Show(2);
        if (Input.GetKeyDown(KeyCode.Alpha2)) FindObjectOfType<MultiKillAnnouncer>().Show(3);
        if (Input.GetKeyDown(KeyCode.Alpha3)) FindObjectOfType<MultiKillAnnouncer>().Show(4);
        if (Input.GetKeyDown(KeyCode.Alpha4)) FindObjectOfType<MultiKillAnnouncer>().Show(5);
        // Round manager cheats
        if (Input.GetKeyDown(KeyCode.Alpha5)) FindObjectOfType<RoundManager>().ChooseRematch();
        if (Input.GetKeyDown(KeyCode.Alpha6)) FindObjectOfType<RoundManager>().ChooseReturnToLobby();
        if (Input.GetKeyDown(KeyCode.Alpha7))
            FindObjectOfType<RoundManager>().StartCoroutine(FindObjectOfType<RoundManager>().ServerEndRound());
        if (Input.GetKeyDown(KeyCode.U))
        {
            uiToggled = !uiToggled;
            FindObjectOfType<GameUIManager>().ToggleUI(uiToggled);
            FindObjectOfType<PingDisplay>().GetComponent<Canvas>().enabled = uiToggled;
            GameObject.Find("Global Settings Button").GetComponent<Canvas>().enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            playerUIToggled = !playerUIToggled;
            PlayerInterface[] interfaces = FindObjectsOfType<PlayerInterface>();
            foreach (PlayerInterface i in interfaces)
            {
                i.hudCanvas.enabled = playerUIToggled;
            }
        }
    }
}