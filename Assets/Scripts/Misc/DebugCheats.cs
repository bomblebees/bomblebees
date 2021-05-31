using UnityEngine;

public class DebugCheats : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.B)) BCheats();
        if (Input.GetKey(KeyCode.C)) CCheats();
    }

    private void BCheats()
    {
        // Cheat for increasing inventory size
        if (Input.GetKeyDown(KeyCode.Alpha1)) FindObjectOfType<RoundManager>().IncreaseGlobalLivesAmt();
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
    }
}