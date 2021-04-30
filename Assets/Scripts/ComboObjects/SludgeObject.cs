using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SludgeObject : TickObject
{
    [SerializeField] public float slowRate = 0.6f;
    [SerializeField] public float slowDuration = 3f;
    // Start is called before the first frame update
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }

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
            // else if (_root.Equals("Blink Object(Clone)"))
            // {
            //     this.EarlyProc();
            // }
            else if (_root.Equals("Plasma Object(Clone)"))
            {
                 this.EarlyProc();
            }
        }
    }
}
