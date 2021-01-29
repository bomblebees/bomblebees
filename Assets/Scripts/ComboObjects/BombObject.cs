using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : TickObject
{
    public float timeTillAnimSpeedup;
    public GameObject model;
    private Material flashingMaterial;

    private void Start()
    {
        flashingMaterial = model.GetComponent<MeshRenderer>().material;
    }

    protected override IEnumerator TickDown()
    {
        if (!didEarlyEffects) {
            yield return new WaitForSeconds(timeTillAnimSpeedup);
            SpeedUpAnim();
        }
        yield return new WaitForSeconds(tickDuration-timeTillAnimSpeedup);
        if (!didEarlyEffects)
        {
            StartCoroutine(TickDownFinish());
        }
    }

    protected virtual void SpeedUpAnim()
    {
        Debug.Log("speeding up anim");
        flashingMaterial.SetFloat("Boolean_7185963F", 0f);
    }
    

    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("ComboHitbox")
            &&
        gameObjHit.transform.root.name.Equals("Bomb Object(Clone)")
            )
        {
            Debug.Log("HAI");
            //here make the other explode too
            this.EarlyProc();
        }
    }
}
