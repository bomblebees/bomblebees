using System;
using System.Collections;
using System.Collections.Generic;
//using System.Net.Configuration;
using Mirror;
using UnityEngine;

public class HandleEntry : MonoBehaviour
{
    // so this trigger that handles if the player is in the object should be smaller than the invis blocker
    public Collider invisibleBlocker;
    private bool allowingEntry = true;

    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (allowingEntry)
        {
            var gameObjHit = other.gameObject;
            if (gameObjHit.CompareTag("Player"))
            {
                SetPlayerEntry(other, true); // This needs to run before the invisibleBlocker scans for collision
            }

            invisibleBlocker.gameObject.SetActive(true); // only enable after collision has been disabled for anyone who needs it
            allowingEntry = false;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        var gameObjHit = other.gameObject;
        // Debug.Log(gameObjHit.name);
        if (gameObjHit.CompareTag("Player"))
        {
            SetPlayerEntry(other, false);
            Physics.IgnoreCollision(gameObjHit.GetComponent<CapsuleCollider>(), this.GetComponent<SphereCollider>(),
                true);
        }
    }

    protected virtual void SetPlayerEntry(Collider playerCollider, bool val)
    {
        
        if (!playerCollider) Debug.LogError("HandleEntry.cs: Need to update collider type of plaer");
        Physics.IgnoreCollision(playerCollider, invisibleBlocker, val);
    }

    public virtual void Restart()
    {
        allowingEntry = true;
    }
}