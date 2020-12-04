// Referencing Jason Weimann's YouTube video "Unity Architecture - Composition or Inheritance?"
// https://www.youtube.com/watch?v=8TIkManpEu4

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	// it's a serializefield rn, but later on when we start prototyping it might get annoying editing other values in code
	[SerializeField]
	protected int startingHealth = 3;

	private int currentHealth;

	// read only (is this the right way to do this lol?)
	public int CurrentHealth
	{
		get { return currentHealth; }
		set { currentHealth = value; }
	}

	// Start is called before the first frame update
	void Awake()
    {
		currentHealth = startingHealth; 
    }

	void Start()
	{

	}

	public void DealDamage(int amount)
	{
		if (currentHealth > 0)
		{
			// change currentHealth by amount (and emit damage dealt event)
			currentHealth -= amount;
			EventManager.TriggerEvent("healthChanged", this, new CustomEventArgs { Amount = currentHealth });
		}

		if (currentHealth <= 0)
		{
			// call method for when currentHealth hits 0 (and emit death event)
			OnHealthZero();
		}
	}

	private void OnHealthZero()
	{
		// when health hits 0, call death event thru EventManager singleton which listeners can react to and use custom args
		EventManager.TriggerEvent("healthObjectDied", this, new CustomEventArgs { EventObject = gameObject });
	}
}
