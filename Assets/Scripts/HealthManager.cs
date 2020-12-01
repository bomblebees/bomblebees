using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
	private GameObject Player;

	// Start is called before the first frame update
	void Start()
    {
		// 
		Player = GameObject.FindWithTag("Player");

		// for now, manually search for Health component and subscribe to its event
		// maybe the manager will search for these sorts of events iteratively later on?

		// subscribe to the health component's event
		Player.GetComponent<Health>().Died += HandleDeath;

	}

	// Update is called once per frame
	void Update()
    {
        
    }

	private void OnEnable()
	{

	}

	private void HandleDeath(GameObject thingToKill)
	{
		// cleanup code for when a thing dies; if UI stuff is listening, emit SceneSpawner's own event here that UI is listening to

		// unsubscribe the SceneSpawner from this dead thing's events before "killing"
		Player.GetComponent<Health>().Died -= HandleDeath;
		Destroy(thingToKill);

		// (maybe conditional check to see if the thing that died is a player, and then do some respawn thing if it is?
	}
}
