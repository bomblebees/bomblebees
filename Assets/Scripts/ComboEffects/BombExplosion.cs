using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BombExplosion : ComboEffect
{
    private float lifeDuration = 0.5f;
    private float sizeScalar = 2f;
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
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }
    }
}
