using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private List<Player> playerList;
	// Start is called before the first frame update
	private void Awake()
	{
		playerList = new List<Player>();
		playerList.Add(Instantiate(Resources.Load<Player>("Prefabs/Player1")));
	}
	void Start()
    {
        
    }
	
    // Update is called once per frame
    void Update()
    {
        
    }
}
