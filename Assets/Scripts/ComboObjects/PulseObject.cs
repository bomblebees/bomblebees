using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
//using NSubstitute.Core.SequenceChecking;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PulseObject : TickObject
{

    public float[] delays;  // the first should be 0
    public GameObject hitboxPrefab;
    private int itr = 0;
    private List<HexCell> closed = new List<HexCell>();
    private List<HexCell> readyToPulse = new List<HexCell>();
    
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }

    public IEnumerator PulseStep(float[] delays, int i)
    {
        while (i <= delays.Length)
        {
            List<HexCell> allNewReady = new List<HexCell>();
            print("gotta create "+readyToPulse.Count+" on itr "+itr);
            while (readyToPulse.Count > 0) 
                // Pulse each ready cell.
            {
                HexCell pulser = readyToPulse[0];
                readyToPulse.RemoveAt(0);
                allNewReady = allNewReady.Concat(PulseFindReady(pulser)).ToList();
                StartCoroutine(PulseCell(pulser, hitboxDuration));
            }
            print("ready before"+readyToPulse.Count);
            readyToPulse = allNewReady;
            print("ready after"+readyToPulse.Count);
            if (itr != delays.Length) yield return new WaitForSeconds(delays[itr]);
            else yield return new WaitForSeconds(0);
            i += 1;
        }
    }
    
    public override IEnumerator ProcEffects()
    {
        yield return new WaitForSeconds(0);
        StopVelocity();
        closed.Add(tileUnderneath);
        readyToPulse = PulseFindReady(tileUnderneath);
        StartCoroutine(PulseStep(delays,0 ));
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }

    public IEnumerator PulseCell(HexCell cell, float hitboxDuration)
    {
        GameObject hitbox = (GameObject) Instantiate(hitboxPrefab,cell.transform.position+new Vector3(0f,7f,0f), Quaternion.Euler(0f,90f,0f));
        hitbox.transform.localScale = new Vector3(1000f, 200f, 1000f);
        yield return new WaitForSeconds(hitboxDuration);
        Destroy(hitbox);
    }

    public List<HexCell> PulseFindReady(HexCell cell)
    {
        // print(cell.neighbors.Length);
        List<HexCell> newReady = new List<HexCell>();
        // get each neighbor. if not in closed, put it in ready
        // print("before"+newReady.Count);
        if (!cell) return newReady;
        foreach (HexCell neighbor in cell.neighbors)
        {
            if (!(closed.Contains(neighbor)))
            {
                newReady.Add(neighbor);
                closed.Add(neighbor);
            }
            
        }
        // print("after"+newReady.Count);
        return newReady;
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