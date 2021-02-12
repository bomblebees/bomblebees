using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : TickObject
{
    public GameObject model;
    public float timeTillAnimSpeedup;
    public Material riseShader;
    public Material pulseShader;
    public float riseRate;
    private float val = -.51f;

    protected override void Start()
    {
        riseRate = 1 / timeTillAnimSpeedup;
        riseShader.SetFloat("Vector1_9422D918", val);
    }

    private void LateUpdate()
    {
        riseShader.SetFloat("Vector1_9422D918", val);
        val += riseRate * Time.deltaTime;
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
        riseShader.SetFloat("Boolean_A83C6489", 1f);
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
