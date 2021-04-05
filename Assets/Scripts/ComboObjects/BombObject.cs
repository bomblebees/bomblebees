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
        // increase fill value of material
        this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", val);
    }

    private void LateUpdate()
    {
        this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", val);
        val += riseRate * Time.deltaTime;
    }


    protected override IEnumerator TickDown()
    {
        if (!didEarlyEffects)
        {
            yield return new WaitForSeconds(timeTillAnimSpeedup);
            SpeedUpAnim();
        }

        yield return new WaitForSeconds(tickDuration - timeTillAnimSpeedup);
        if (!didEarlyEffects)
        {
            StartCoroutine(TickDownFinish());
        }
    }

    protected virtual void SpeedUpAnim()
    {
        // toggle "about to explode" state of material
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }

    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("ComboHitbox"))
        {
            var _root = gameObjHit.transform.root.name;
            if (_root.Equals("Bomb Object(Clone)"))
            {
                this.EarlyProc();
            }
            else if (_root.Equals("Laser Object(Clone)"))
            { 
                this.EarlyProc();
            }
            else if (_root.Equals("Blink Object(Clone)"))
            { 
                this.EarlyProc();
            }
        }
    }
}