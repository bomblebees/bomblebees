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
    [SerializeField] public ParticleSystem particleSystem;
    public float ignoreTriggererDuration = 1f;
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        StartCoroutine(IgnoreTriggeringPlayer(ignoreTriggererDuration));
        FindTriggerDirection(edgeIndex);
        plasmaSphereModel.transform.position = plasmaSphereModel.transform.position + targetDir * 5;
        this.hitboxEnabled = true;
        this.plasmaSphereModel.SetActive(true);
        return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }
    
    private IEnumerator IgnoreTriggeringPlayer(float seconds)
    {
        this.canHitTriggeringPlayer = false; // see Health.cs' OnTriggerEnter()
        yield return new WaitForSeconds(seconds);
        this.canHitTriggeringPlayer = true;
    }

    public Vector3 lastPosition;
    private void LateUpdate()
    {
        if (this.hitboxEnabled == true)
        {
            plasmaSphereModel.transform.localPosition += targetDir * projectileSpeed * Time.deltaTime;
            var nextPos = FindCenterBelowOther(this.plasmaSphereModel);
            if (lastPosition != nextPos)
            {
                /// PUT .PLAY() HERE
                this.hitBox.transform.position = nextPos;
                lastPosition = nextPos;
            }
        }
    }

    protected virtual void FindTriggerDirection(int edgeIndex)
    {
        targetDir = HexMetrics.edgeDirections[edgeIndex];
        // this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = targetDir;
        // this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles =
        //     new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex] + 270f);
    }
}