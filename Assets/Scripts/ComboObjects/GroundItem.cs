using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;

public class GroundItem : NetworkBehaviour
{
	public char bombType = ' ';
	public Color color = Color.white;
	public GameObject model;
	[SerializeField] private float sinTimeOffsetRange = 0f;
	[SerializeField] private float bobFrequency = 7f;
	[SerializeField] private float amplitude = .2f;
	[SerializeField] private float lifeTime = 5.0f;

	private float thisOffset = 0f;

	// when destroying, flag and destroy in next 
	private bool flagToDestroy = false;
	private IEnumerator DestroyAfterLifetimeRoutine;

	// Start is called before the first frame update
	public override void OnStartClient()
    {
		// offset by random float for better look
		thisOffset = UnityEngine.Random.Range(0f, sinTimeOffsetRange);
	}

	public override void OnStartServer()
	{
		DestroyAfterLifetimeRoutine = DestroyAfterLifetime();
		StartCoroutine(DestroyAfterLifetimeRoutine);
	}

	// Update is called once per frame
	[Client]
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

	// Coroutine is defined and started in server, so server will set destroy flag to true, destroying it on server and for all players
	[Server]
	private IEnumerator DestroyAfterLifetime()
	{
		yield return new WaitForSeconds(lifeTime);
		flagToDestroy = true;
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

	private void DestroyItem()
	{
		NetworkServer.Destroy(gameObject);
	}
}
