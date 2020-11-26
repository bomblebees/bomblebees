using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : MonoBehaviour
{
    public float tickDuration = 2f;

    private void Awake()
    {
        StartCoroutine(TickDown());
    }

    IEnumerator TickDown()
    {
        Debug.Log(String.Concat("Bomb is ticking for ", tickDuration));
        yield return new WaitForSeconds(tickDuration);

        this.Explode();
        this.DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    private void Explode()
    {
        Debug.Log("Creating Bomb Explosion");
        new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        if (!Instantiate(Resources.Load("Prefabs/ComboObjects/Bomb/Bomb Explosion"),
            new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z),
            Quaternion.identity))
        {
            Debug.LogError("Could not instantiate Bomb Explosion");
        }
    }
}
