using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityObject : TickObject
{
    [SerializeField] public float pullStrength = 30f;
    [SerializeField] public float distThresh = 5f;
    // Start is called before the first frame update
   
    // public override void SetCreator(Player player)
    // {
    //     base.SetCreator(player);
    //     queenStartupDelay = 2.0f;
    // }
}
