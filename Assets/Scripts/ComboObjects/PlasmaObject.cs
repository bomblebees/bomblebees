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
    public float projectileSpeed = 3f;
    public float breakdownDuration = 3f;
    public float startOffset = 3.5f;
    [SerializeField] public GameObject plasmaSphereModel;
    [SerializeField] public ParticleSystem particleSystem;
    public float ignoreTriggererDuration = 1f;
    
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }
    
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        if (wasHit) return true;
        wasHit = true;
        FindTriggerDirection(edgeIndex);
        ////
        var nextPos = FindCenterBelowOtherInclusive(plasmaSphereModel.transform.position + targetDir * startOffset);
        this.plasmaSphereModel.transform.position = nextPos;
        this.hitBox.transform.position = nextPos;
        ////
        StartCoroutine(SpawnBall(edgeIndex, triggeringPlayer));
        return true;
    }

    // note: TriggerObject.Proc() runs in parallel
    public IEnumerator SpawnBall(int edgeIndex, GameObject triggeringPlayer)
    {
        if (ownerIsQueen)
        {
            yield return new WaitForSeconds(queenStartupDelay);
        }
        else
        {
            yield return new WaitForSeconds(startupDelay);
        }
        StartCoroutine(IgnoreTriggeringPlayer(ignoreTriggererDuration));
        this.hitboxEnabled = true;
        this.hitBox.SetActive(true); 
        this.plasmaSphereModel.SetActive(true);
        base.Push(edgeIndex, triggeringPlayer);  // This shoulda been at the top lol
    }

    public Vector3 lastPosition;
    private void LateUpdate()
    {
        if (this.hitboxEnabled == true)
        {
            plasmaSphereModel.transform.localPosition += targetDir * projectileSpeed * Time.deltaTime;
            var nextPos = FindCenterBelowOtherInclusive(this.plasmaSphereModel.transform.position);
            if (nextPos == this.plasmaSphereModel.transform.position)  // Since FindCenter... returns own position on fail
            {
                // this.hitBox.SetActive(false); 
            }
            else if (lastPosition != nextPos)
            {
                particleSystem.Play();
                this.hitBox.transform.position = nextPos;
                lastPosition = nextPos;
                this.hitBox.SetActive(true); 
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
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(breakdownDuration);
        // play breakdown anim here
        DestroySelf();
    }
}