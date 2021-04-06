using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PulseObject : TickObject
{

    public float[] delays;  // the first should be 0
    private int itr = 0;
    private List<HexCell> closed;
    private List<HexCell> readyToPulse;
    
    // called by TickDown()
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }


    public IEnumerator PulseStep(float[] delays)
    {
        if (itr < delays.Length)
        {
            yield return new WaitForSeconds(delays[itr]);
            StartCoroutine(PulseStep(delays));
        }
    }

    public void PulseFind(HexCell cell)
    {
        // get each neighbor. if not in closed, put it in ready
        for (var direction = 0; direction < 3; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexDirection oppositeDirection = hexDirection.Opposite();
            neigh = cell.GetNeighbor(hexDirection);
            cell.GetNeighbor(oppositeDirection), oppositeDirection, checkList;
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