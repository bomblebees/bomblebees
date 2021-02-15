using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class PlasmaObject : TriggerObject
{
    private bool hitboxEnabled = false;
    private Vector3 targetDir;
    private float projectileSpeed = 3f;
    [SerializeField] public GameObject plasmaSphereModel;
    private Collider triggeringPlayerCollider;
    private CharacterController triggeringPlayerCC;
    public float ignoreTriggererDuration = 1f;
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        Debug.Log("Triggerer is " + triggeringPlayer.transform.name);

        // TODO get this working
        foreach (MeshCollider coll in this.hitBox.GetComponentsInChildren<MeshCollider>())
        {
            
            Debug.Log("rbc "+coll.gameObject.name);
            Debug.Log("player is "+triggeringPlayer.gameObject.name);
            var ply = triggeringPlayer.GetComponent<Player>();
            ply.IgnoreThese(coll, true);
        }

        this.triggeringPlayerCollider = triggeringPlayer.GetComponent<Collider>();
        this.triggeringPlayerCC = triggeringPlayer.GetComponent<CharacterController>();
        StartCoroutine(IgnoreTriggeringPlayer(ignoreTriggererDuration));
        
        FindTriggerDirection(edgeIndex);
        this.hitboxEnabled = true;
        this.plasmaSphereModel.SetActive(true);
        return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }

    
    private IEnumerator IgnoreTriggeringPlayer(float seconds)
    {
        foreach (MeshCollider coll in this.hitBox.GetComponentsInChildren<MeshCollider>())
        {
            Physics.IgnoreCollision(triggeringPlayerCC, coll, true);
            Physics.IgnoreCollision(triggeringPlayerCollider, coll, true);
        }
        yield return new WaitForSeconds(seconds);
        foreach (MeshCollider coll in this.hitBox.GetComponentsInChildren<MeshCollider>())
        {
            Physics.IgnoreCollision(triggeringPlayerCC, coll, false);
            Physics.IgnoreCollision(triggeringPlayerCollider, coll, false);
        }
    }

    private void LateUpdate()
    {
        if (this.hitboxEnabled == true)
        {
            Debug.Log("Runnng");
            plasmaSphereModel.transform.localPosition += targetDir * projectileSpeed * Time.deltaTime;
            this.hitBox.transform.position = FindCenterBelowOther(this.plasmaSphereModel);

        }
        
    }

    protected virtual void FindTriggerDirection(int edgeIndex)
    {
        Debug.Log("edgeIndex is "+edgeIndex);
        targetDir = HexMetrics.edgeDirections[edgeIndex];
        Debug.Log("targetDir is "+targetDir);
        // this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = targetDir;
        // this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles =
        //     new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex] + 270f);
    }
}