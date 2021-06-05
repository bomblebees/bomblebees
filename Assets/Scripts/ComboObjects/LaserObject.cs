using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
	private bool isActivated = false;

    public GameObject arrowUI;
	public GameObject chargeSFX;

    protected override void StartDangerAnim()
    {
        // this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        // this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }
    
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {		


		if (isSpinnable && !isActivated) // boolean defined in TriggerObject; set to false when hitbox is activated
		{
			GetSpunDirection(edgeIndex, triggeringPlayer, true);
		}
		if (!isActivated)
		{
			isActivated = true;
			chargeSFX.SetActive(true);
		}
		return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }

    // protected virtual void UpdateLaserDirection(int edgeIndex, GameObject triggeringPlayer)
    // {
    //     // get angle from player
    //     Vector3 dir = triggeringPlayer.transform.position - transform.position;
    //     dir = triggeringPlayer.transform.InverseTransformDirection(dir);
    //     
    //     float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    //     angle += 90f;
    //     targetAngle = RoundAngleToHex(angle);
    //
    //     startAngle = model.transform.eulerAngles.y;
    //     if (Math.Abs(startAngle - targetAngle) > 180)
    //     {
    //         if (startAngle > targetAngle) targetAngle += 360f;
    //         startAngle += 360f;
    //     }
    //
    //     // model.transform.eulerAngles = new Vector3(0f, angle, 0f);
    //     // targetAngle = angle;
    //     isRotating = true;
    //     this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
    //     this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles = new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex]+270f);
    // }

    protected virtual float RoundAngleToHex(float angle)
    {
        float newAngle = angle;
        if (angle < 0) {angle += 360;}
        float remainder = angle % 60;
        if (remainder < 30f)  // round down
        {
            angle -= remainder;
        }
        else
        {
            angle += (60 - remainder);
        }
        return angle;
    }
    
    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
		// Debug.Log("Collision occurred in laserobject");
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("InterObjectHitbox") && this.tag == "ComboObject")
        {
            var _root = gameObjHit.transform.root.name;
            if (_root.Equals("Bomb Object(Clone)"))
            {
                StartCoroutine(Breakdown());
            }
			if (_root.Equals("Laser Object(Clone)"))
			{
				StartCoroutine(Breakdown());
			}
		}
    }
    
    protected virtual void Update()
    {
        if (hasStarted == true)
        {
            ListenForTrigger();
            ElapseTimeAlive();
            ListenForDespawn();
            if (wasHit)
            {
                StepFillShader();
            }

            if (isRotating)
            {

                float newAngle = Mathf.Lerp(startAngle, targetAngle, rotateLerpRate);
                var r = model.transform.eulerAngles;
                this.model.transform.eulerAngles = new Vector3(r.x, newAngle, r.z);
                startAngle = newAngle;
                // this.model.transform.eulerAngles = Vector3.Lerp(this.model.transform.eulerAngles,
                //         new Vector3(0f, targetAngle, 0f), rotateLerpRate); // move object               
            }
        }
    }
    

}