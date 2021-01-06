using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LaserBeam : ComboEffect
{
    private float lifeDuration = 0.5f;
    private float sizeScalar = 2f;

	private int defaultDamage = 1;
	private int testCounter = 0;

    // Start is called before the first frame update
    private void Rotate(HexDirection dir)
    {
        Vector3 newDirVec;
        // set transforms rotation here.
        switch (dir)
        {
            case HexDirection.NW:
                newDirVec = new Vector3(90f, 0f, 30f);
                break;
            case HexDirection.SE:
                newDirVec = new Vector3(90f, 0f, 30f);
                break;
            case HexDirection.W:
                newDirVec = new Vector3(90f, 0f, 90f);
                break;
            case HexDirection.E:
                newDirVec = new Vector3(90f, 0f, 90f);
                break;
            case HexDirection.NE:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
            default:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
        }
        if (dir == HexDirection.NW)
            
        this.gameObject.transform.Rotate(newDirVec); 
    }

    public void SpawnInDirection(HexDirection direction)
    {
        this.Rotate(direction);
        StartCoroutine(TickDown());
    }

   protected override IEnumerator TickDown()
    {
        yield return new WaitForSeconds(lifeDuration);
        DestroySelf();
    }

    private void OnTriggerEnter(Collider other)
    {
		testCounter++;
		if (other.gameObject.GetComponent<Health>() != null && !other.gameObject.GetComponent<Health>().Invulnerable)
		{
			other.gameObject.GetComponent<Health>().DealDamage(defaultDamage);
		}
    }
}