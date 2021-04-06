using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityObject : TickObject
{
    [SerializeField] public float pullStrength = 30f;
    [SerializeField] public float distThresh = 5f;
    // Start is called before the first frame update
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }
}
