using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundStartEnd : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timerText;

    public IEnumerator StartRoundFreezetime(int freezetime)
    {
        if (freezetime == -1)
        {
            titleText.text = "";
        } else
        {
            titleText.text = "All players loaded!";
            yield return new WaitForSeconds(1);
            FindObjectOfType<AudioManager>().PlaySound("startCountdown");
            titleText.text = "Game Starting in";
            timerText.gameObject.SetActive(true);

            for (int i = freezetime; i > 0; i--)
            {
                timerText.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            timerText.gameObject.SetActive(false);

            titleText.text = "Fight!";
            yield return new WaitForSeconds(1);
            titleText.text = "";
        }
    }

    public IEnumerator EndRoundFreezetime(int freezetime)
    {
        titleText.text = "<size=400%>Game!</size>";
        //titleText.gameObject.transform.localPosition = new Vector3(0, 60, 0);
        //titleText.color = new Color32(255, 255, 255, 220);

        yield return new WaitForSeconds(freezetime);

        titleText.text = "";
    }

    public void UpdateRoundWaitUI(int connPlayers, int totalPlayers)
    {
        // ex. Waiting for players... (2/4)
        titleText.text = "Waiting for players... (" +
            connPlayers +
            "/" +
            totalPlayers +
            ")";
    }
}
