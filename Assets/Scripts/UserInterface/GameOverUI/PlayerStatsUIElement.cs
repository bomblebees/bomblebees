using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUIElement : MonoBehaviour
{
	[SerializeField] public Image avatar;
	[SerializeField] public TMP_Text playerName;
	[SerializeField] public TMP_Text killsText;
	[SerializeField] public TMP_Text deathsText;
	[SerializeField] public TMP_Text bombComboMadeText;
	[SerializeField] public TMP_Text totalPointsText;

	// Only updated after the round ends
	public int kills;
	public int totalDeathPenalty;
	public int totalBombCombosMade;
	public int totalPoints;
}
