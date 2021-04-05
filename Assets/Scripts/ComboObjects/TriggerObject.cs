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

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
        ReadyFillShader();
    }

    protected override void Update()
    {
        ListenForTrigger();
        ElapseTimeAlive();
        ListenForDespawn();
        if (wasHit)
        {
            StepFillShader();
        }
    }

    protected virtual void ListenForTrigger()
    {
        if (canBeTriggered && wasHit)
        {
            StartCoroutine(Proc());
        }
    }
    
    
    protected virtual void StartDangerAnim()
    {
        // TO OVERRIDE IN CHILDREN
    }

    protected virtual IEnumerator Proc()
    {
        var timeBtwnFillAndTrigger = startupDelay - fillShaderDuration;
        yield return new WaitForSeconds(fillShaderDuration);
        StartDangerAnim();
        yield return new WaitForSeconds(timeBtwnFillAndTrigger);
        canBeTriggered = false;  // To stop it from being triggered twice
        timeTriggered = timeAlive;
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
    }

    protected virtual void ListenForDespawn()
    {
        if (canBeExtended && timeAlive > lingerDuration)
        {
            canBeExtended = false;
            if (wasHit)
            {
                if (lingerDuration - timeTriggered < minimumLifeDuration + startupDelay) // If hit and going into "overtime" with trigger
                {
                    float extensionTime = minimumLifeDuration + startupDelay - (lingerDuration - timeTriggered);
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
    
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        NotifyOccupiedTile(false); // prolly move this later
        wasHit = true;
        return true;
    }

}