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

        // V this is kinda busted, 4/6 angles are wrong
        this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles += new Vector3(0f, 0f, HexMetrics.edgeAngles[edgeIndex]+270f);

    }
}