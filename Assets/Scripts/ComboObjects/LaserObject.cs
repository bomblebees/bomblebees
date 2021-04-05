using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
    
    public float breakdownDuration = 3f;
    
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        UpdateLaserDirection(edgeIndex);
        return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }

    protected virtual void UpdateLaserDirection(int edgeIndex)
    {
        this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
        this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles = new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex]+270f);
    }
    
    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("ComboHitbox"))
        {
            var _root = gameObjHit.transform.root.name;
            if (_root.Equals("Bomb Object(Clone)"))
            {
                StartCoroutine(Breakdown());
            }
        }
    }
    
    public IEnumerator Breakdown()
    {
        // if (isLocalPlayer) FindObjectOfType<AudioManager>().StopPlaying("bombBeep");
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        // play breakdown anim here
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(breakdownDuration);
        DestroySelf();
    }
}