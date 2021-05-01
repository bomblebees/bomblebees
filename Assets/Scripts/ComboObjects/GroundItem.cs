using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class GroundItem : NetworkBehaviour
{
	[SyncVar (hook = nameof(OnBombTypeChanged))] public char bombType = ' ';
	[SyncVar] public Color color = Color.white;
	public GameObject r_model;
	public GameObject g_model;
	public GameObject p_model;
	public GameObject y_model;
	[SerializeField] private float sinTimeOffsetRange = 0f;
	[SerializeField] private float bobFrequency = 7f;
	[SerializeField] private float amplitude = .2f;

	private float thisOffset = 0f;

	// when destroying, flag and destroy in next 
	private bool flagToDestroy = false;

	// Start is called before the first frame update
	public override void OnStartClient()
    {
		// offset by random float for better look
		thisOffset = UnityEngine.Random.Range(0f, sinTimeOffsetRange);
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		// Gives a sin-based animation based on editable parameters
		transform.position = transform.position + new Vector3(0.0f, Mathf.Sin(Time.time * bobFrequency + thisOffset) * amplitude, 0.0f);
    }

	private void LateUpdate()
	{
		if (flagToDestroy)
		{
			DestroyItem();
		}
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		// to-do: if collision is detected while inventory is already full, it will still destroy the grounditem objects it comes into contact with, fix later
		if (other.gameObject.layer == 18)
		{
			// cache the collided player gameobject here, because we know player has a ground item pickup hitbox
			PlayerInventory inventoryComponent = other.transform.parent.GetComponent<PlayerInventory>();

			// if inventory of player it collided with is already full, don't add
			int indexOfBombType = Array.IndexOf(inventoryComponent.GetBombTypes(), bombType);
			if (!(inventoryComponent.inventoryList[indexOfBombType] >= inventoryComponent.GetMaxInvSizes()[indexOfBombType]))
			{
				RpcPlayBombPickupSound(other.transform.parent.gameObject);
				AddToInventory(other, inventoryComponent);
			}
		}

	}

	[Server]
	private void AddToInventory(Collider collider, PlayerInventory inventory)
	{
		// if the parent of the collider (the Player object) exists:
		Debug.Log("GroundItem calls addtoinventory on server");
		

		// if inventory of player it collided with is already full, don't add
		if (collider.transform.parent)
		{
			// grab the inventory
			

			// and then add this grounditem's bomb type to the inventory, then destroy
			inventory.AddInventoryBomb(bombType, 1);

			flagToDestroy = true;
			
		}
	}
	[ClientRpc]
	private void RpcPlayBombPickupSound(GameObject player)
	{
		Debug.Log(player.name);
		if (player.name == "LocalPlayer")
		{
			FindObjectOfType<AudioManager>().PlaySound("inventorypop");
		}
	}

	private void OnBombTypeChanged(char oldValue, char newValue)
	{
		// to-do: how do i not hard code these bomb type values? help
		switch (newValue)
		{
			case 'r':
				r_model.active = true;
				// GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.red);
				break;
			case 'p':
				p_model.active = true;
				// GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.magenta);
				break;
			case 'y':
				y_model.active = true;
				// this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.yellow);
				break;
			case 'g':
				g_model.active = true;
				// this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.green);
				break;
		}
		
	}

	private void DestroyItem()
	{
		NetworkServer.Destroy(gameObject);
	}
}
