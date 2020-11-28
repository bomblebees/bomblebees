using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	// Start is called before the first frame update
	[SerializeField]
	private Text Player1HealthText;

	// store reference into property at startup, so we can listen to its events
	private GameObject Player;

	private void OnEnable()
	{

		

	}

	private void OnDisable()
	{
		// problem: results in error if we disable after the player is dead/gameobject is deleted
		Player.GetComponent<Health>().HealthChanged -= UpdateUI;
	}

	void Start()
    {

		// to find the Player object in runtime we wait until Start and then search the scene:
		Player = GameObject.FindWithTag("Player");


		// searching and finding specific object in scene with FindWithTag is slow
		// so later we can search an array of players managed by
		// some sort of manager instead of looking for it by tag? idk
		Player.GetComponent<Health>().HealthChanged += UpdateUI;
		Debug.Log(Player.name);

		Debug.Log("P1 Health: " + Player.GetComponent<Health>().CurrentHealth);
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	private void UpdateUI(int newHealthAmount)
	{
		// Runs when the change health event is invoked in our Health script
		// Update the p1 health text we dragged in through the inspector
		Debug.Log("new HP amount is " + newHealthAmount);
		Player1HealthText.text = "P1 Health: " + newHealthAmount;
	}
}
