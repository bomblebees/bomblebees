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
		// this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", color);
		transform.position = transform.position + new Vector3(0.0f, UnityEngine.Random.Range(-.3f, .3f), 0.0f);
		thisOffset = Random.Range(0f, sinTimeOffsetRange);
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		transform.position = transform.position + new Vector3(0.0f, Mathf.Sin(Time.time * bobFrequency + thisOffset) * amplitude, 0.0f);
    }

	private void OnTriggerEnter(Collider other)
	{
		// Debug.Log("Ground Item collided with: " + other.name);
		// NetworkServer.Destroy(this.gameObject);
		if (other.gameObject.layer == 18)
		{
			Debug.Log(other.transform.parent);
			if (other.transform.parent.GetComponent<Player>().isLocalPlayer)
			{
				FindObjectOfType<AudioManager>().PlaySound("inventorypop");
			}
			AddToInventory(other);

			// other.transform.parent

		}
		
	}

	[Server]
	private void AddToInventory(Collider collider)
	{
		if (collider.transform.parent)
		{
			PlayerInventory collidedInventory = collider.transform.parent.GetComponent<PlayerInventory>();
			collidedInventory.AddInventoryBomb(bombType, 1);

			NetworkServer.Destroy(this.gameObject);
		}
	}

	private void OnBombTypeChanged(char oldValue, char newValue)
	{
		// how do i not hard code these bomb type values? help
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
