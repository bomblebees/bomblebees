using UnityEngine;

public class DebugCheats : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetKey(KeyCode.C)) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FindObjectOfType<MultiKillAnnouncer>().Show(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FindObjectOfType<MultiKillAnnouncer>().Show(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FindObjectOfType<MultiKillAnnouncer>().Show(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            FindObjectOfType<MultiKillAnnouncer>().Show(5);
        }
    }
}