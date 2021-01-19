using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : ComboObject
{
    private bool isMoving = false;
    public float lerpRate = 0.15f;
    public Vector3 targetPosition;
    
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
            
            // Vector3 dir = HexMetrics.edgeDirections[edgeIndex];
            // rigidBody.AddForce(HexMetrics.edgeDirections[edgeIndex] * pushedSpeed);
            // this.gameObject.transform.position += HexMetrics.edgeDirections[edgeIndex] * HexMetrics.hexSize * 2;
            targetPosition = this.gameObject.transform.position + HexMetrics.edgeDirections[edgeIndex] * HexMetrics.hexSize * 2;
            // lerp here
            this.isMoving = true;

            // dont forget to find new center after, and gotocenter
        }
    }

    private void Update()
    {
        if (this.isMoving)
        {
            // lerp
            this.gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, lerpRate);
            // threshold here
        }
    }
}
