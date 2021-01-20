using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
    protected override void Push(int edgeIndex)
    {
        wasHit = true;
        UpdateLaserDirection(edgeIndex);
    }

    protected virtual void UpdateLaserDirection(int edgeIndex)
    {
        Debug.Log("edgeIndex is "+edgeIndex);
        this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
    }
}