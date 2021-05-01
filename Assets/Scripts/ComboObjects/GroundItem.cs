using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class GroundItem : NetworkBehaviour
{
	[SyncVar (hook = nameof(OnBombTypeChanged))] public char bombType = ' ';
	[SyncVar] public Color color = Color.white;

	[SerializeField] private float sinTimeOffsetRange = 0f;
	[SerializeField] private float bobFrequency = 7f;
	[SerializeField] private float amplitude = .2f;

	private float thisOffset = 0f;

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
				AddToInventory(other, inventoryComponent);
			}
		}

	}

	[Server]
	private void AddToInventory(Collider collider, PlayerInventory inventory)
	{
		// if the parent of the collider (the Player object) exists:
		if (collider.transform.parent.GetComponent<Player>().isLocalPlayer)
		{
			FindObjectOfType<AudioManager>().PlaySound("inventorypop");
		}
		// if inventory of player it collided with is already full, don't add
		if (collider.transform.parent)
		{
			// grab the inventory
			

			// and then add this grounditem's bomb type to the inventory, then destroy
			inventory.AddInventoryBomb(bombType, 1);
			
			NetworkServer.Destroy(this.gameObject);
			
		}
	}

	private void OnBombTypeChanged(char oldValue, char newValue)
	{
		// to-do: how do i not hard code these bomb type values? help
		switch (newValue)
		{
			case 'r':
				GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.red);
				break;
			case 'p':
				GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.magenta);
				break;
			case 'y':
				this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.yellow);
				break;
			case 'g':
				this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.green);
				break;
		}
		
	}
}
