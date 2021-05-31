using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private GameObject[] livesAnchors = new GameObject[4];

    [SerializeField] private GameObject livesUIElementPrefab;

    [SerializeField] GameUIManager gameUIManager;

    [SerializeField] TMP_Text conditionText;

    private LivesUIElement[] livesUIs = new LivesUIElement[4];
    private List<GameObject> playerList = new List<GameObject>();

    private LobbySettings _lobbySettings;
    private Gamemode selectedGamemode;

    private void Awake()
    {
        _lobbySettings = FindObjectOfType<LobbySettings>();
        if (_lobbySettings == null) Debug.LogError("LobbySettings not found!");

        selectedGamemode = _lobbySettings.GetGamemode();

        if (selectedGamemode is StandardGamemode)
            conditionText.text = "Last bee standing wins!";
        else if (selectedGamemode is TeamsGamemode)
            conditionText.text = "Last team standing wins!";
        else if (selectedGamemode is KillsGamemode)
            conditionText.text = "First to " + (selectedGamemode as KillsGamemode).eliminations + " kills win!";
        else if (selectedGamemode is ComboGamemode)
            conditionText.text = "First to " + (selectedGamemode as ComboGamemode).winPoints + " points win!";
    }

    public void EnableLivesUI(Player p)
    {
        // create the player card
        GameObject obj = Instantiate(
            livesUIElementPrefab,
            livesAnchors[p.playerRoomIndex].transform.position,
            livesAnchors[p.playerRoomIndex].transform.rotation,
            gameObject.transform);

        // resacle to anchor size
        obj.transform.localScale = livesAnchors[p.playerRoomIndex].transform.localScale;

        LivesUIElement elem = obj.GetComponent<LivesUIElement>();

        // add to list
        livesUIs[p.playerRoomIndex] = elem;

        // enable ui for player
        elem.livesObject.SetActive(true);

        // set the avatar
        elem.avatar.sprite = gameUIManager.GetComponent<CharacterHelper>().GetCharImage(p.characterCode);

        // set the player
        elem.player = p.gameObject;

        // if it is the local player
        if (p.transform.root.name == "LocalPlayer")
        {
            // set background to white
            elem.background.color = Color.white;

            // the flash color on the tween returns to white
            elem.background.GetComponent<ColorTween>().endColor = Color.white;
        }

        // enable the ranking text
        livesAnchors[p.playerRoomIndex].transform.Find("RankText").gameObject.SetActive(true);

        // add player to player list
        playerList.Add(p.gameObject);



        // Set the lives (if applicable)
        if (_lobbySettings.GetGamemode() is StandardGamemode
            || _lobbySettings.GetGamemode() is TeamsGamemode)
        {
            elem.heartsObject.SetActive(true);

            
            for (int j = 0; j < elem.hearts.Length; j++)
            {
                if (j < p.GetComponent<Health>().maxLives)
                {
                    elem.hearts[j].SetActive(true);
                    elem.hearts[j].GetComponent<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetLivesImage(p.characterCode);
                } else
                {
                    elem.hearts[j].SetActive(false);
                }
            }
        }
        
        if (_lobbySettings.GetGamemode() is KillsGamemode) elem.eliminationsObject.SetActive(true);
        if (_lobbySettings.GetGamemode() is ComboGamemode) elem.combosObject.SetActive(true);
    }

    public void UpdateLives(int currentLives, Player player)
    {
        int i = Array.FindIndex(livesUIs, e => e.player.GetComponent<Player>().playerRoomIndex == player.playerRoomIndex);

        Debug.Log("update lives: " + currentLives);

        switch (currentLives)
        {
            case 2: { livesUIs[i].hearts[2].SetActive(false); break; }
            case 1: {
                    livesUIs[i].hearts[1].SetActive(false);
                    livesUIs[i].background.GetComponent<ColorTween>().LoopTween(); 
                    break;
                }
            case 0: {
                    livesUIs[i].hearts[0].SetActive(false);
                    livesUIs[i].background.GetComponent<ColorTween>().EndLoopTween();
                    livesUIs[i].background.color = new Vector4(.1f, .1f, .1f, 1f); // not working for some reason
                    livesUIs[i].avatar.color = new Vector4(.5f, .5f, .5f, 5f);
                    break;
            }
        }
    }

    public void UpdateCombos(int combos, Player player)
    {
        int i = Array.FindIndex(livesUIs, e => e.player.GetComponent<Player>().playerRoomIndex == player.playerRoomIndex);

        livesUIs[i].combosText.text = combos.ToString();
    }

    public void UpdateEliminations(int killAmt, Player player)
    {
        int i = Array.FindIndex(livesUIs, e => e.player.GetComponent<Player>().playerRoomIndex == player.playerRoomIndex);

        livesUIs[i].elimsText.text = killAmt.ToString();
    }

    public void UpdateOrdering()
    {
        // Recalculate the winning order
        GameObject[] orderedList = _lobbySettings.GetGamemode().GetWinningOrder(playerList.ToArray());

        // Enable crown for top player
        // orderedList[0].transform.Find("")

        // Reorder livesUI based on new list 
        livesUIs = livesUIs.OrderBy(e => e == null ? 4 : Array.IndexOf(orderedList, e.player)).ToArray();


        for (int i = 0; i < playerList.Count; i++)
        {
            // Move lives ui to the new positions
            LeanTween.moveLocal(livesUIs[i].gameObject, livesAnchors[i].transform.localPosition, 1f)
                .setEase(LeanTweenType.easeOutExpo);

            // Enable/disable player crown model
            GameObject defaultCrown = orderedList[i].GetComponent<Player>().crownModelDefault.gameObject;
            if (i == 0 && !defaultCrown.activeSelf)
                defaultCrown.GetComponent<CrownAnimator>().EnableCrown();
            if (i > 0 && defaultCrown.activeSelf)
                defaultCrown.GetComponent<CrownAnimator>().DisableCrown();
            defaultCrown.transform.localScale = Vector3.one;

            // Enable/disable ghost crown model
            GameObject ghostCrown = orderedList[i].GetComponent<Player>().crownModelGhost.gameObject;
            if (i == 0 && !ghostCrown.activeSelf)
                ghostCrown.GetComponent<CrownAnimator>().EnableCrown();
            if (i > 0 && ghostCrown.activeSelf)
                ghostCrown.GetComponent<CrownAnimator>().DisableCrown();
            ghostCrown.transform.localScale = Vector3.one;
        }
    }
}
