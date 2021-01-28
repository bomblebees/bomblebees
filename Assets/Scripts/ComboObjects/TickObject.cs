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

    public override void OnStartClient()
    {
        base.OnStartClient();
        // All the client needs to do is render the sfx/vfx
        // The server should do all calculations
        StartCoroutine(TickDown());
    }

    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(tickDuration);
        StopVelocity();
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(lingerDuration);
        StartCoroutine(DestroySelf());
    }
}
