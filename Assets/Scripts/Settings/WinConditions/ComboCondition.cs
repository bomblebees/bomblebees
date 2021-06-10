using UnityEngine;
using Mirror;

public class ComboCondition : WinCondition
{
    private int toCombos;

    private Player[] players;

    private LobbySettings settings;

    #region Virtuals

    [Server]
    public override void InitWinCondition()
    {
        toCombos = FindObjectOfType<ComboGamemode>().winPoints;
        settings = FindObjectOfType<LobbySettings>();
    }

    [Server]
    public override void StartWinCondition()
    {
        players = FindObjectsOfType<Player>();

        // Subscribe to the swap event
        FindObjectOfType<EventManager>().EventPlayerSwap += OnSwapEvent;
        FindObjectOfType<EventManager>().EventPlayerTookDamage += OnLivesChanged;
    }

    #endregion

    private void OnSwapEvent(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        // If a combo was not made, return;
        if (!combo) return;

        // Everytime a player swapped, check if total combos is reached
        CheckWin();
    }

    private void OnLivesChanged(int newLives, GameObject bomb, GameObject player)
    {
        // Everytime a player is eliminated, check if total combos is reached
        CheckWin();
    }

    private Player leader;

    private bool thirtyPlayed = false;
    private bool tenPlayed = false;

    private void CheckWin()
    {
        if (settings && settings.practiceMode) return; // Return if practice mode

        if (leader == null) leader = players[0]; // Initialize a leader if needed

        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];

            int playerCombos = p.GetComponent<PlayerStatTracker>().totalPoints;

            // If leader has been overtaken
            if (p != leader && playerCombos > leader.GetComponent<PlayerStatTracker>().totalPoints)
            {
                Debug.Log(p.playerRoomIndex + " overtook " + leader.playerRoomIndex);

                leader = p; // leader is now that top player
                string coloredName = GetPlayerText(p);

                FindObjectOfType<GameUIManager>().Announce(coloredName + " has taken the lead!");
            }

            // Check if player has reached the combos
            if (playerCombos >= toCombos)
            {
                base.InvokeWinConditionSatisfied();
            } else if (playerCombos >= toCombos - 10 && !tenPlayed)
            {
                tenPlayed = true;

                string coloredName = GetPlayerText(p);

                FindObjectOfType<GameUIManager>().Announce(coloredName + " has <size=150%>"
                    + (toCombos - playerCombos) + "</size>  points remaining!");
            } else if (playerCombos >= toCombos - 30 && !thirtyPlayed)
            {
                thirtyPlayed = true;

                string coloredName = GetPlayerText(p);

                FindObjectOfType<GameUIManager>().Announce(coloredName + " has <size=150%>"
                    + (toCombos - playerCombos) + "</size>  points remaining!");
            }
        }
    }

    // Copypasta'd from message feed
    private string GetPlayerText(Player player)
    {
        Player p = player.GetComponent<Player>();
        return "<b><color=#" + ColorUtility.ToHtmlStringRGB(p.playerColor) + ">" + p.steamName + "</color></b>";
    }
}
