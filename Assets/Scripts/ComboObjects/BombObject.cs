using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : TickObject
{
    public GameObject model;
    public float timeTillAnimSpeedup;
    public Material flashingMaterial;
    

    protected override void Start()
    {
        base.Start();
        flashingMaterial = model.GetComponent<MeshRenderer>().material;
        if(!flashingMaterial) Debug.Log("Flashing Material not found");
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
            //here make the other explode too
            this.EarlyProc();
        }
    }

    
}
