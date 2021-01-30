using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TickObject : ComboObject
{
    public float tickDuration = 4f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
        StartCoroutine(TickDown());
    }
    
    protected virtual IEnumerator TickDown()
    {
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
        ProcEffects();
        if (!didEarlyEffects)
        {
            yield return new WaitForSeconds(lingerDuration);
            StartCoroutine(DestroySelf());
        }
    }

    public virtual void ProcEffects()
    {
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
        didEarlyEffects = true;
        ProcEffects();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // All the client needs to do is render the sfx/vfx
        // The server should do all calculations
        StartCoroutine(TickDown());
    }


    protected override bool Push(int edgeIndex)
    {
        bool result = base.Push(edgeIndex);
        if (result)
        {
            NotifyOccupiedTile(false);
        }
        return result;
    }
}
