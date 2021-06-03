﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Required Children:
// - VFX
// - SFX
// - Hitbox
public class ComboObject : NetworkBehaviour
{
    [Header("Required", order = 1)]
    public GameObject blockerHandler;
    public GameObject hitBox;
    public Collider collider;
    public GameObject telegraphVFX;
    [SerializeField] public GameObject SFX;
    [SerializeField] public GameObject BeepSFX;
    public Image bombRadialTimerImage;
    
    protected bool isMoving = false;  // isMoving: Whether or not the object is moving after being pushed
    [Header("Properties", order = 2)] public float travelDistanceInHexes = 4;
    protected float pushedDirAngle = 30;
    public float lerpRate = 0.15f;  // The speed at which the object is being pushed
    public float lerpRateSecondary = 0.50f;
    public Vector3 targetPosition;  // The position that the tile wants to move to after being pushed
    public float snapToCenterThreshold = 0.5f;
    
    public Vector3 nearestCenter;
    public HexCell tileUnderneath;
    public int edgeIndexCached = 0;
    public float vfxDuration = 4f;
    public float sfxDuration = 4f;
    public float hitboxDuration = 4f;
    public float lingerDuration = 8f;
    public float startupDelay = 0f;
    public float queenStartupDelay = 0f;
    public bool ownerIsQueen = false;
    public bool hasStarted = false;
    protected bool didEarlyEffects = false;

    // player who triggered the bomb
	[SyncVar]
    public GameObject triggeringPlayer;
    protected bool canHitTriggeringPlayer = true;

    // the player who last interacted with the proccing bomb that blew this bomb up
    public GameObject proccingPlayer;
    public bool wasProcced = false;
    public bool wasMovingWhenProcced = false;

    [FormerlySerializedAs("objectMat")] public GameObject model;
    [FormerlySerializedAs("timeBtwnFillFinishAndFuse")] public float fillShaderRatio = 0;  // set this in the inspector;
    
    protected float fillShaderVal = -.51f;
    protected float fillShaderRate = 0;

    // player who placed the bomb (set in Player.cs, SERVER only variable)
    [SyncVar] public GameObject ownerPlayer;

    public AnimationCurve movementCurve;
    public int moveTweenId;

    public GameObject GetOwnerPlayer() { return ownerPlayer; }

    
    /// <summary>
    /// Returns the player who is responsible for any kills that this bomb acquires
    /// </summary>
    public virtual GameObject GetKillerPlayer(GameObject playerThatWasKilled)
    {
        Debug.Log("GetKillerPlayer(): was this bomb moving? ans = " + wasMovingWhenProcced);
        return triggeringPlayer;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        IgnoreDamageHitbox();
        if (!model)
        {
            print("Error: need to assign objectMat to Bomb Object!");
        }
        hasStarted = true;

        var angles = this.model.transform.eulerAngles;
        // add the start dir
        // var startAngle = RoundAngleToHex(player.GetComponent<Player>().rotation.eulerAngles.y);
        // print("start angle is "+startAngle);
        // this.gameObject.transform.eulerAngles = new Vector3(angles.x, angles.y, angles.z);
    }

    protected virtual void IgnoreDamageHitbox()
    {
        if (hitBox)
        {
            foreach (Collider c in hitBox.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, c, true);
            }
        }
    }

    protected virtual void Update()
    {
        //Debug.Log("is this bomb moving? - " + isMoving);
        //ListenForMoving();
    }

    float MapDist(float start, float end, float point)
    {
        // 
        // float percent = end - start 
        return 0f;
    }

    protected void ListenForMoving()
    {
        snapToCenterThreshold = float.Epsilon;

        if (this.isMoving)
        {
            Debug.Log("distance to target: " + GetDistanceFrom(targetPosition) + " | threshold = " + snapToCenterThreshold);
            //Debug.Log("distance to target: " + snapToCenterThreshold);

            // // decrease start speed, increase at end
            // float deltaLerp = lerpRate * 0.1f;
            // float mapDist = Math.
            // lerpRate = Mathf.Clamp()

            if (GetDistanceFrom(targetPosition) < snapToCenterThreshold)
            {
                Debug.Log("center threshold exceeded");
                if (isServer)
                {
                    // The server should calculate these values to make sure
                    // all clients have the synced values as dictated by the server
                    FindCenter();
                    GoToCenter();
                    NotifyOccupiedTile(true);
                    //isMoving = false;
                    RpcStopMoving();
                }
                Debug.Log("snapped to center, moving should be false: " + isMoving);
            }
            else  // check if next tile is empty
            {
                if (FindCenter())  // defer check to whenever entering a new tile
                {
                    var possiblePosition = nearestCenter + HexMetrics.edgeDirections[edgeIndexCached] * HexMetrics.hexSize * 1;
                    var checkForObjectRay = new Ray(possiblePosition + new Vector3(0f, 40f, 0f), Vector3.down);
                    RaycastHit otherObjectHit;
                    if (Physics.Raycast(checkForObjectRay, out otherObjectHit, 100f,
                            1 << LayerMask.NameToLayer("ComboObjects"))) // found bomb, now stop
                    {
                        lerpRate = lerpRateSecondary;
                        targetPosition = nearestCenter;
                    }
                }
                
            }
        }
    }

    [ClientRpc]
    void RpcStopMoving()
    {
        blockerHandler.SetActive(true);
        blockerHandler.GetComponent<HandleEntry>().Restart();
        isMoving = false;
    }

    protected virtual bool FindCenter()
    {
        bool foundNew = false;
        var objectRay = new Ray(this.gameObject.transform.position, Vector3.down);
        RaycastHit tileUnderneathHit;
        if (Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            tileUnderneath = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
            var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position;
            // var result = GetComponent<Transform>().position;
            if (nearestCenter != result) foundNew = true;
            nearestCenter = result;
            //RpcFindCenter(result, this.gameObject.transform.position);
        }
        return foundNew;
    }
    
    //// Tell all clients the nearest center as calculated by the server
    //[ClientRpc]
    //protected virtual void RpcFindCenter(Vector3 centerPos, Vector3 pos)
    //{
    //    var objectRay = new Ray(pos, Vector3.down);
    //        RaycastHit tileUnderneathHit;
    //        if (Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
    //        {
    //            tileUnderneath = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
    //            var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position;
    //            nearestCenter = result;
    //        }
    //    // nearestCenter = centerPos;
    //}
    
    protected virtual Vector3 FindCenterBelowOther(GameObject origin) 
    {
        var objectRay = new Ray(origin.transform.position, Vector3.down);
        RaycastHit tileUnderneathHit;
        if (Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            // var tileBelowOrigin = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
            var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position ;
            return result;
        }
        else return origin.transform.position;
    }
    
    protected virtual Vector3 FindCenterBelowOtherInclusive(Vector3 origin) 
        {
            var objectRay = new Ray(origin, Vector3.down);
            RaycastHit tileUnderneathHit;
            var status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles"));
			
            if (!status)
            {
				// status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("SlowHex"));
				
				// Status false means collider hit wasn't a base tile
				// return origin;
            }
            // if (!status)
            // {
            //     status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("DangerHex"));
            // }
            if (status) {
                // var tileBelowOrigin = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
                var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position;
                return result;
            }
            else return origin;
        }


    protected virtual float GetDistanceFrom(Vector3 targetPos)
    {
        var dist = Mathf.Sqrt(
            Mathf.Pow(this.gameObject.transform.position.x - targetPos.x, 2) +
            Mathf.Pow(this.gameObject.transform.position.z - targetPos.z, 2)
        );
        return dist;
    }

    //[Server]
    protected virtual void GoToCenter()
    {
        isMoving = false;
        this.gameObject.transform.position = nearestCenter;
        //RpcGoToCenter(nearestCenter);
    }

    //// Tell all clients to move their bomb to the center
    //[ClientRpc]
    //protected virtual void RpcGoToCenter(Vector3 centerPos)
    //{
    //    isMoving = false;
    //    this.gameObject.transform.position = centerPos;
    //}

    protected virtual void NotifyOccupiedTile(bool val)
    {
        if (isServer)
        {
            if (tileUnderneath) tileUnderneath.SetOccupiedByComboObject(val);
        }
    }
    protected virtual void StopVelocity()
    {
        var rigidBody = this.GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
        this.isMoving = false;
    }

    protected virtual IEnumerator EnableVFX()
    {
        var vfx = this.gameObject.transform.Find("VFX").gameObject;
        vfx.SetActive(true);
        yield return new WaitForSeconds(vfxDuration);
        vfx.SetActive(false);
    }

    protected virtual IEnumerator EnableSFX()
    {
		Debug.Log("starting sound");
        if (SFX) {
            SFX.SetActive(true);
            yield return new WaitForSeconds(sfxDuration);
            SFX.SetActive(false);
        }
    }

    protected virtual void EnableBeepSFX()
    {
        if (BeepSFX)
        {
            BeepSFX.SetActive(true);
        }
    }

    protected virtual IEnumerator EnableHitbox()
    {
        // Play shake anim
        FindObjectOfType<CameraShake>().InduceStress(0.22f);

        hitBox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitBox.SetActive(false);
    }

    protected virtual IEnumerator DisableObjectModel()
    {
        //GameObject _model = this.gameObject.transform.Find("Model").gameObject;
        if (!model)
        {
            print("ERROR: Model not found for a Bomb! Make sure the Model's name is 'Model'");
        }
        model.SetActive(false);
        yield return new WaitForSeconds(lingerDuration);
    }

    protected virtual IEnumerator DisableObjectCollider()
    {
        collider.enabled = false;
        blockerHandler.SetActive(false);
        yield return new WaitForSeconds(lingerDuration);
    }

    [ClientRpc]
    protected virtual void RpcPush(int edgeIndex, GameObject triggeringPlayer, int newTravelDistance)
    {
        this.travelDistanceInHexes = newTravelDistance;
        this.triggeringPlayer = triggeringPlayer;
        Push(edgeIndex, triggeringPlayer);
    }

    protected virtual bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        LeanTween.cancel(moveTweenId);
        FindCenter();
        GoToCenter();
        bool result = false;
        var rigidBody = this.GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            return false;
            Debug.LogError("ComboObject.cs: ComboObject has no RigidBody component.");
        }
        else
        {
            targetPosition = this.gameObject.transform.position;  // Safety, in the event that no possible tiles are found.
            // float lerpScaleRate = 1/travelDistanceInHexes;
            edgeIndexCached = edgeIndex;
            for (var tileOffset = 1; tileOffset < travelDistanceInHexes; tileOffset++)
            {
                var possiblePosition = gameObject.transform.position +
                                       HexMetrics.edgeDirections[edgeIndex] * HexMetrics.hexSize * tileOffset; 
;
                
                // if works then change targetPosition
                var checkForEmptyRay = new Ray(possiblePosition, Vector3.down);
                var checkForObjectRay = new Ray(possiblePosition + new Vector3(0f, 20f, 0f), Vector3.down);
                RaycastHit tileUnderneathHit;
                RaycastHit otherObjectHit;
                if (Physics.Raycast(checkForEmptyRay, out tileUnderneathHit, 10f, 1 << LayerMask.NameToLayer("BaseTiles"))  // if raycast hit basetile, break
                    && !Physics.Raycast(checkForObjectRay, out otherObjectHit, 20f, 1 << LayerMask.NameToLayer("ComboObjects"))) // And there shouldn't be a bomb on this tile
                {
                    targetPosition = possiblePosition;
                    result = true;
                }
                else
                {
                    break;
                }
            }

            if (result == true)
            {

                moveTweenId = LeanTween.move(this.gameObject, targetPosition, 0.25f)
                    .setEase(movementCurve)
                    .setOnComplete(() => { isMoving = false; FindCenter(); NotifyOccupiedTile(true); })
                    .id;

                this.isMoving = true; 
            }
            return result;
        }
    }

    [ServerCallback]
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // The calculations should be validated on server side, return if not server

        if (other.gameObject.CompareTag("Spin"))
        {
            // "Toggle on" radial timer
            // bombRadialTimerImage.transform.localScale = new Vector3(0.26f,0.26f,0.26f);
            // bombRadialTimerImage.transform.localScale *= 1.25f;
            // bombRadialTimerImage.color = new Vector4(1,1,1,1);

            var playerPosition = other.transform.parent.gameObject.transform.position;
            // note: Don't use Vector3.Angle
            var angleInRad = Mathf.Atan2(playerPosition.x - transform.position.x,
                playerPosition.z - transform.position.z);

            var dirFromPlayerToThis =
                (Mathf.Rad2Deg * angleInRad) + 180; // "+ 180" because Unity ranges it from [-180, 180]

            pushedDirAngle = 30f;
            int edgeIndex;

            for (edgeIndex = 0; edgeIndex < HexMetrics.edgeAngles.Length; edgeIndex++)
            {
                if (Mathf.Abs(dirFromPlayerToThis - HexMetrics.edgeAngles[edgeIndex]) <= 30)
                {
                    pushedDirAngle = HexMetrics.edgeAngles[edgeIndex];
                    break;
                }
            }



            // if this is a bomb, or if this is a deployable that was not already activated
            if (this is TickObject || (this is TriggerObject && !this.GetComponent<TriggerObject>().wasHit))
            {
                // Update "owner" of bomb (the player who kicked it last)
                triggeringPlayer = other.transform.parent.gameObject;

                // Adjust travel distance based on spin power
                int power = triggeringPlayer.GetComponent<PlayerSpin>().spinPower;
                //Debug.Log("power: " + power);

                int newTravelDistanceInHexes = power + 1;

                RpcPush(edgeIndex, triggeringPlayer, newTravelDistanceInHexes);
            }

            // Update occupation status of tile
            // Push(edgeIndex, triggeringPlayer); // Push for server too
            

            // Set spin hit on player, for event logger
            //triggeringPlayer.GetComponent<Player>().spinHit = this.gameObject;
        }
    }

    protected virtual IEnumerator DestroySelf()
    {
		yield return new WaitForSeconds(0);
        NotifyOccupiedTile(false);
        NetworkServer.Destroy(this.gameObject);
    }
    
    // For extension time
    protected virtual IEnumerator DestroySelf(float extensionTime)
    {
        yield return new WaitForSeconds(extensionTime);
        NotifyOccupiedTile(false);
        NetworkServer.Destroy(this.gameObject);
    }

    // this isn't being sent to every player
    // [Command(ignoreAuthority = true)] 
    public bool CanHitThisPlayer(GameObject target)
    {
        print(triggeringPlayer);
        if (
            (canHitTriggeringPlayer 
            && 
            target == triggeringPlayer)
            ||
            (target != triggeringPlayer)
        )
        {
         return true;
        }
        else return false;
    }
    
    protected IEnumerator IgnoreTriggeringPlayer(float seconds)
    {
        this.canHitTriggeringPlayer = false; // see Health.cs' OnTriggerEnter()
        yield return new WaitForSeconds(seconds);
        this.canHitTriggeringPlayer = true;
    }

    // To be called in TickObject and TriggerObject individually
    protected virtual void ReadyFillShader()
    {
        if (ownerIsQueen)
            fillShaderRate = 1 / (queenStartupDelay * fillShaderRatio);
        else
            fillShaderRate = 1 / (startupDelay * fillShaderRatio);
        // this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", fillShaderVal);
        // "Toggle on" radial timer
            // bombRadialTimerImage.transform.localScale = new Vector3(0.26f,0.26f,0.26f);
            // bombRadialTimerImage.transform.localScale *= 1.25f;
        // bombRadialTimerImage.color = new Vector4(1,1,1,1);
        if (telegraphVFX) telegraphVFX.SetActive(true);
    }

    protected virtual void StepFillShader()
    {
        this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", fillShaderVal);  // Fill shader
        
        bombRadialTimerImage.fillAmount = 1 - ((fillShaderVal + 0.51f) / (1 + (1 - fillShaderRatio)));

        fillShaderVal += fillShaderRate * Time.deltaTime;
    }

    protected virtual float RoundAngleToHex(float angle)
        {
            float remainder = angle % 60;
            if (remainder < 30f)  // round down
            {
                angle -= remainder;
            }
            else
            {
                angle += (60 - remainder);
            }

            if (angle < 0) angle = 300f;  // for edge case where it rounds to -60, which is out of bounds
            return angle;
        }
}