using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        UpdateLaserDirection(edgeIndex);
        return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }

    protected virtual void UpdateLaserDirection(int edgeIndex)
    {
        Debug.Log("edgeIndex is " + edgeIndex);
        this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
        this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles = new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex]+270f);
    }
}