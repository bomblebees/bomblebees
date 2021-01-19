using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : ComboObject
{
    
    protected override void Push(int edgeIndex)
    {
        var rigidBody = this.GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            Debug.LogError("ComboObject.cs: ComboObject has no RigidBody component.");
        }
        else
        {
            // Update occupation status of tile
            NotifyOccupiedTile(false);
            
            Vector3 dir = HexMetrics.edgeDirections[edgeIndex];
            rigidBody.AddForce(HexMetrics.edgeDirections[edgeIndex] * pushedSpeed);
        }
    }
}
