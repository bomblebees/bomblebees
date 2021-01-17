using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : ComboObject
{
    public float tickDuration = 2f;
    public string effectPath = "Prefabs/ComboEffects/Bomb Explosion";
    public float animDuration = 4f;
    public float hitboxDuration = 4f;

    private void Start()
    {
        StartCoroutine(TickDown());
    }

    protected override IEnumerator TickDown()
    {
        
        yield return new WaitForSeconds(tickDuration);
        StartCoroutine(EnableAnim());
        StartCoroutine(EnableFX);
        this.Explode();
    }

    private void Explode()
    {
        new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        if (!Instantiate(Resources.Load(effectPath),
            new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z),
            Quaternion.identity))
        {
            Debug.LogError("Could not instantiate Bomb Explosion");
        }
    }
}
