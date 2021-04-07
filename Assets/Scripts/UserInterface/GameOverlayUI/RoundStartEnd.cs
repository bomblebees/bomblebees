using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundStartEnd : MonoBehaviour
{
    [Header("Start Round")]
    [SerializeField] private GameObject startGameUI;
    [SerializeField] private TMP_Text waitText;
    [SerializeField] private TMP_Text timerText;

    [Header("End Round")]
    [SerializeField] private GameObject endGameUI;

    public IEnumerator StartRoundFreezetime(int freezetime)
    {
        waitText.text = "All players loaded!";
        yield return new WaitForSeconds(1);
        waitText.text = "Game Starting in";
        timerText.gameObject.SetActive(true);

        for (int i = freezetime; i > 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        waitText.gameObject.SetActive(false);
        timerText.text = "Begin!";

        yield return new WaitForSeconds(1);
        startGameUI.SetActive(false);
    }

    public IEnumerator EndRoundFreezetime(int freezetime)
    {
        endGameUI.SetActive(true);
        yield return new WaitForSeconds(freezetime);
        endGameUI.SetActive(false);
    }

    public void UpdateRoundWaitUI(int connPlayers, int totalPlayers)
    {
        // ex. Waiting for players... (2/4)
        waitText.text = "Waiting for players... (" +
            connPlayers +
            "/" +
            totalPlayers +
            ")";
    }
}
