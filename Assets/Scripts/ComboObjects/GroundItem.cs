using System.Collections;
using System.Collections.Generic;
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
		thisOffset = Random.Range(0f, sinTimeOffsetRange);
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
			// play sound if local player
			if (other.transform.parent.GetComponent<Player>().isLocalPlayer)
			{
				FindObjectOfType<AudioManager>().PlaySound("inventorypop");
			}

			// then, add this ground item's type to the collided player's inventory:
			AddToInventory(other);
		}
		
	}

	[Server]
	private void AddToInventory(Collider collider)
	{
		// if the parent of the collider (the Player object) exists:
		if (collider.transform.parent)
		{
			// grab the inventory
			PlayerInventory collidedInventory = collider.transform.parent.GetComponent<PlayerInventory>();

			// and then add this grounditem's bomb type to the inventory, then destroy
			collidedInventory.AddInventoryBomb(bombType, 1);

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
