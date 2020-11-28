// Referencing Jason Weimann's YouTube video "Unity Architecture - Composition or Inheritance?"
// https://www.youtube.com/watch?v=8TIkManpEu4

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	[SerializeField]
	protected int startingHealth = 100;

	private int currentHealth;

	// read only (is this the right way to do this lol?)
	public int CurrentHealth
	{
		get { return currentHealth; }
		set { }
	}

	// https://learn.unity.com/tutorial/c-actions
	public event Action<GameObject> Died;
	public event Action<int> HealthChanged;

    // Start is called before the first frame update
    void Awake()
    {
		currentHealth = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void DealDamage(int amount)
	{
		Debug.Log(currentHealth);
		if (currentHealth > 0)
		{
			// change currentHealth by amount (and emit damage dealt event)
			currentHealth -= amount;
			HealthChanged?.Invoke(currentHealth);
		}

		if (currentHealth <= 0)
		{
			// call method for when currentHealth hits 0 (and emit death event)
			OnHealthZero();
		}
	}

	private void OnHealthZero()
	{
		Died?.Invoke(gameObject);
		
		// For now, just delete the gameobject this script is stuck to
		// Later we might use events for game state stuff, so that functionality can be split into more intuitive ways
		
		
		//Destroy(gameObject);
	}
}
