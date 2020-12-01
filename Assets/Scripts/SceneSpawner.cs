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


		Player = Instantiate(PlayerPrefab);


        Instantiate(Resources.Load("Prefabs/Development UI"));

        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Main Camera"));
        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Directional Light"));
    }

}
