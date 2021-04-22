using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : TickObject
{
    
    // called by TickDown()
    protected override void StartDangerAnim()
    {
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
            else if (_root.Equals("Plasma Object(Clone)"))
            { 
                this.EarlyProc();
            }
        }
    }
}