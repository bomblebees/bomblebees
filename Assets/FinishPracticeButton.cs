using UnityEngine;

public class FinishPracticeButton : MonoBehaviour
{
    private void Awake()
    { 
        GetComponentInChildren<Canvas>().enabled = FindObjectOfType<PingDisplay>().myPingDisplay.Equals("Practice Mode");
    }

    public void FinishPractice()
    {
        FindObjectOfType<GlobalSettings>().OnClickQuitToMenu();
    }
}
