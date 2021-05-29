using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUIElement : MonoBehaviour
{
	[SerializeField] public Image avatar;
	[SerializeField] public TMP_Text playerName;
	[SerializeField] public TMP_Text killsText;
	[SerializeField] public TMP_Text deathsText;
	[SerializeField] public TMP_Text comboMadeText;
	[SerializeField] public TMP_Text totalPointsText;
}
