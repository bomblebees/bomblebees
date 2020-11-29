using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchHitbox : MonoBehaviour
{
    private int hitTimer = 100;

    private void Start()
    {
        // Global position of the punch
        // Debug.Log(
        //     this.GetComponentInParent<Transform>().transform.position -
        //     this.transform.localPosition);
        // Debug.Log(this.GetComponentInParent<Transform>().transform.position);
    }

    private void Update()
    {
        hitTimer++;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered on Punch's end");
        if (other.gameObject.CompareTag("ComboObject") && hitTimer > 100)
        {
            // Deprecated - Old calculations
            var angleBetweenParentAndObj = Vector3.Angle(
                this.gameObject.GetComponentInParent<Transform>().transform.position, 
                other.gameObject.transform.position
            );
            
            
            
            // var punchGlobalPosition = 
            //     this.GetComponentInParent<Transform>().transform.position
            //     - this.transform.localPosition;
            // var angleBetweenPunchAndObj = Vector3.Angle(
            //     punchGlobalPosition,
            //     other.gameObject.transform.position
            // );
            
            hitTimer = 0;
        }
    }
}
