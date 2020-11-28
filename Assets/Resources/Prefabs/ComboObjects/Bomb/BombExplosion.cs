using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    private float lifeDuration = 0.5f;
    private float sizeScalar = 2f;

	private int defaultDamage = 50;

    // Start is called before the first frame update
    private void Awake()
    {
        ChangeScale();
        StartCoroutine(TickDown());
    }

    private void ChangeScale()
    {
        this.transform.localScale *= sizeScalar;
    }

    IEnumerator TickDown()
    {
        yield return new WaitForSeconds(lifeDuration);
        DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

		if (other.gameObject.GetComponent<Health>() != null)
		{
			other.gameObject.GetComponent<Health>().DealDamage(defaultDamage);
		}
	}
}
