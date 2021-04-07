using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute.Core.SequenceChecking;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PulseObject : TickObject
{

    public float[] delays;  // the first should be 0
    private int itr = 0;
    private List<HexCell> closed;
    private List<HexCell> readyToPulse;
    public Transform hitboxPrefab;
    
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }

    public IEnumerator PulseStep(float[] delays)
    {
        if (itr >= delays.Length) { 
            yield return new WaitForSeconds(0);  // Do nothing
        }
        else
        {
            while (readyToPulse.Count > 0)  // Pulse each ready cell
            {
                HexCell pulser = readyToPulse[0];
                readyToPulse.RemoveAt(0);
                closed.Add(pulser);
                StartCoroutine(PulseCell(pulser, hitboxDuration));
            }
            yield return new WaitForSeconds(delays[itr]);
            itr++;
            StartCoroutine(PulseStep(delays));
        }
    }
    
    public override IEnumerator ProcEffects()
    {
        print("Proccing Pulse");
        yield return new WaitForSeconds(0);
        StopVelocity();
        PulseFindReady(tileUnderneath);
        PulseStep(delays);
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }

    public IEnumerator PulseCell(HexCell cell, float hitboxDuration)
    {
        // play anim here
        // spawn hitbox here?
        var hitbox = Instantiate(hitboxPrefab, cell.transform.position, Quaternion.identity);
        hitbox.GetComponent<MeshRenderer>().enabled = true; // for debug
        yield return new WaitForSeconds(hitboxDuration);
        Destroy(hitbox);
        // turn off hitbox and anims
    }
    

    public void PulseFindReady(HexCell cell)
    {
        // get each neighbor. if not in closed, put it in ready
        for (var direction = 0; direction < 3; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexDirection oppositeDirection = hexDirection.Opposite();
            var n1 = cell.GetNeighbor(hexDirection);
            var n2 = cell.GetNeighbor(oppositeDirection);
            if (!(closed.Contains(n1)))
            {
                readyToPulse.Add(n1);
            }
            if (!(closed.Contains(n2)))
            {
                readyToPulse.Add(n2);
            }
        }
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
                this.EarlyProc();
            }
            else if (_root.Equals("Laser Object(Clone)"))
            { 
                this.EarlyProc();
            }
            else if (_root.Equals("Blink Object(Clone)"))
            { 
                this.EarlyProc();
            }
        }
    }
}