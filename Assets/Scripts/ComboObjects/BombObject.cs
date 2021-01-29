using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BombObject : TickObject
{
    public float timeTillAnimSpeedup;
    public GameObject model;
    private Material flashingMaterial;

    private void Start()
    {
        flashingMaterial = model.GetComponent<MeshRenderer>().material;
    }

    protected override IEnumerator TickDown()
    {
        yield return new WaitForSeconds(timeTillAnimSpeedup);
        SpeedUpAnim();
        yield return new WaitForSeconds(tickDuration - timeTillAnimSpeedup);
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

    protected virtual void SpeedUpAnim()
    {
        Debug.Log("speeding up anim");
        flashingMaterial.SetFloat("Boolean_7185963F", 1f);
    }
}
