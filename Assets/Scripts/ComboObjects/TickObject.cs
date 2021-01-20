using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickObject : ComboObject
{
    public float tickDuration = 4f;
    
    protected virtual void Start()
    {
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
        StartCoroutine(TickDown());
    }
    
    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(tickDuration);
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        StopVelocity();
        yield return new WaitForSeconds(lingerDuration);
        StartCoroutine(DestroySelf());
    }
}
