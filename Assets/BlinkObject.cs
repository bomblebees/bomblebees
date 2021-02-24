using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkObject : TickObject
{
    // lets have this go invis when pushed, and use Push's raycast to find the next available bomb
    public float dist = 4;
    // Start is called before the first frame update
    
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        // FindTriggerDirection(edgeIndex);
        edgeIndex += 2;
        print("edgeIndex "+edgeIndex);
        base.Push(edgeIndex, triggeringPlayer);
        triggeringPlayer.transform.position = this.gameObject.transform.position;
        return true;
    }
    
    protected virtual void FindTriggerDirection(int edgeIndex)
    {
        // targetDir = HexMetrics.edgeDirections[edgeIndex];
        // this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = targetDir;
        // this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles =
        //     new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex] + 270f);
    }
}
