using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSpawner : MonoBehaviour
{
	// Use this global object currently to hold player(s) references and data and also events, but maybe 
	// use dedicated managers/systems later?

	// Start is called before the first frame update

	// is it right to store a reference to the player like this as an object?
	// or should we make an explicit Player class?
	[SerializeField]
	GameObject PlayerPrefab;

	private GameObject Player;

	void Awake()
    {
		// store references on gameobject awake:
		// https://docs.unity3d.com/Manual/ExecutionOrder.html

		Instantiate(Resources.Load("Prefabs/Hex Grid"));

		// attach returned object to Player property

		/* I have this weird problem where if I want to get the instantiated
		 * player's Health component, instead of instantiating directly thru code,
		 * I make a property for inserting the Player prefab in the Inspector, and
		 * then I make a new clone instance of that prefab in game, and then initialize it to 
		 * the Player property here
		 * 
		 * 
		 * in the future, if we have a dedicated class for a Player we could do something like
		 * "Player1 = new Player(instantiation params);" returns object of type Player which 
		 * has all the references and components attached and stuff, but also along with player data
		 */

		Player = Instantiate(PlayerPrefab);


        Instantiate(Resources.Load("Prefabs/Development UI"));

        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Main Camera"));
        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Directional Light"));
    }

	private void OnEnable()
	{
		// for now, manually search for Health component and subscribe to its event
		// maybe the manager will search for these sorts of events iteratively later on?

		// subscribe to the health component's event
		Player.GetComponent<Health>().Died += HandleDeath;
	}

	private void HandleDeath(GameObject thingToKill)
	{
		// cleanup code for when a thing dies; if UI stuff is listening, emit SceneSpawner's own event here that UI is listening to

		Debug.Log("THING DIED!");

		// unsubscribe the SceneSpawner from this dead thing's events before "killing"
		Player.GetComponent<Health>().Died -= HandleDeath;
		Destroy(thingToKill);

		// (maybe conditional check to see if the thing that died is a player, and then do some respawn thing if it is?
	}
}
