using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GroundItem : NetworkBehaviour
{
	[SyncVar (hook = nameof(OnBombTypeChanged))] public char bombType = ' ';
	[SyncVar] public Color color = Color.white;

    // Start is called before the first frame update
    public override void OnStartClient()
    {
		// this.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", color);
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Ground Item collided with: " + other.name);
		// NetworkServer.Destroy(this.gameObject);
	}

	private void OnBombTypeChanged(char oldValue, char newValue)
	{
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
