using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class TriggerObject : ComboObject
{
    protected bool wasHit = false;
    protected bool canBeTriggered = true; // to make sure it's only hit once.
    protected float timeAlive = 0f; // Seconds since object was instantiated
    protected float timeTriggered = 0f;
    public float minimumLifeDuration = 4f; // Minimum time the object needs to use effect for

    private bool canBeExtended = true;
    // note: lingerDuration is the time spent until object despawns without being hit
    
    protected virtual void Start()
    {
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
    }

    protected override void Update()
    {
        ListenForTrigger();
        ElapseTimeAlive();
        ListenForDespawn();
    }

    protected virtual void ListenForTrigger()
    {
        if (canBeTriggered && wasHit)
        {
            Debug.Log("triggered");
            canBeTriggered = false;  // To stop it from being triggered twice
            timeTriggered = timeAlive;
            StartCoroutine(EnableSFX());
            StartCoroutine(EnableVFX());
            StartCoroutine(EnableHitbox());
            StartCoroutine(DisableObjectCollider());
            StartCoroutine(DisableObjectModel());
        }
    }

    protected virtual void ListenForDespawn()
    {
        if (canBeExtended && timeAlive > lingerDuration)
        {
            Debug.Log("entered despawn");
            canBeExtended = false;
            if (wasHit)
            {
                if (lingerDuration - timeTriggered < minimumLifeDuration) // If hit and going into "overtime" with trigger
                {
                    float extensionTime = minimumLifeDuration - (lingerDuration - timeTriggered);
                    StartCoroutine(DestroySelf(extensionTime));
                }
                else // If was hit and not going into "overtime" with trigger
                {
                    StartCoroutine(DestroySelf());
                }
            }
            else
            {
                StartCoroutine(DestroySelf());
            }
        }
    }

    protected virtual void ElapseTimeAlive()
    {
        timeAlive += Time.deltaTime;
    }

    protected override IEnumerator EnableHitbox()
    {
        var hitbox = this.gameObject.transform.Find("Hitbox").gameObject;
        hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
    }
    
    protected override void Push(int edgeIndex)
    {
        NotifyOccupiedTile(false);
        wasHit = true;
    }
}