// Referencing Jason Weimann's YouTube video "Unity Architecture - Composition or Inheritance?"
// https://www.youtube.com/watch?v=8TIkManpEu4

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	[SerializeField]
	protected int startingHealth = 100;

	private int health;

    // Start is called before the first frame update
    void Awake()
    {
		health = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void DealDamage(int amount)
	{
		Debug.Log(health);
		if (health > 0)
		{
			health -= amount;
		}
		if (health <= 0)
		{
			OnHealthZero();
		}
	}

	private void OnHealthZero()
	{
		Debug.Log("health under or at 0");
		Debug.Log(health);

		// For now, just delete the gameobject this script is stuck to
		// Later we might use events for game state stuff, so that functionality can be split into more intuitive ways
		Destroy(gameObject);
	}
}
