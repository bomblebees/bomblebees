using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TickObject : ComboObject
{
    protected float tickDuration = 4.0f;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // All the client needs to do is render the sfx/vfx
        // The server should do all calculations
        // StartCoroutine(TickDown());
    }

    // public override void SetCreator(Player player)
    // {
    //     StartCoroutine(TickDown());
    //     base.SetCreator(player);
    // }

    protected virtual IEnumerator TickDown()
    {
        print("tick tickduration is "+tickDuration);
        yield return new WaitForSeconds(tickDuration);
        if (!didEarlyEffects)
        {
            StartCoroutine(TickDownFinish());
        }
    }

    protected virtual IEnumerator TickDownFinish()
    {
        FindCenter();
        GoToCenter();
        StartCoroutine((ProcEffects()));
        if (!didEarlyEffects)
        {
            yield return new WaitForSeconds(lingerDuration);
            StartCoroutine(DestroySelf());
        }
    }

    public virtual IEnumerator ProcEffects()
    {
        yield return new WaitForSeconds(startupDelay);
        StopVelocity();
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }


    public virtual void EarlyProc()
    {
        if (isLocalPlayer) FindObjectOfType<AudioManager>().StopPlaying("bombBeep"); // TODO do something like "StopPlaying(objectSound)"
        didEarlyEffects = true;
        FindCenter();
        GoToCenter();
        StartCoroutine(ProcEffects());
    }

    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        bool result = base.Push(edgeIndex, triggeringPlayer);
        if (result)
        {
            NotifyOccupiedTile(false);
        }

        this.blockerHandler.SetActive(false); // in order to stop blocking players while moving
        return result;
    }
    
}