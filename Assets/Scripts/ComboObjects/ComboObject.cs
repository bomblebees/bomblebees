using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] public GameObject SFX;
    
    protected bool isMoving = false;  // isMoving: Whether or not the object is moving after being pushed
    [Header("Properties", order = 2)] public float travelDistanceInHexes = 4;
    protected float pushedDirAngle = 30;
    public float lerpRate = 0.9f;  // The speed at which the object is being pushed
    public Vector3 targetPosition;  // The position that the tile wants to move to after being pushed
    public float snapToCenterThreshold = 0.5f;
    
    public Vector3 nearestCenter;
    public HexCell tileUnderneath;

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
    protected GameObject triggeringPlayer;
    protected bool canHitTriggeringPlayer = true;
    
    [FormerlySerializedAs("objectMat")] public GameObject model;
    [FormerlySerializedAs("timeBtwnFillFinishAndFuse")] public float fillShaderRatio = 0;  // set this in the inspector;
    
    protected float fillShaderVal = -.51f;
    protected float fillShaderRate = 0;

    // player who placed the bomb (set in Player.cs, SERVER only variable)
    protected GameObject ownerPlayer;
    [Server] public void SetOwnerPlayer(GameObject p) { 
        ownerPlayer = p;
        ownerIsQueen = p.GetComponent<Player>().isQueen; 
    }
    [Server] public GameObject GetOwnerPlayer() { return ownerPlayer; }

    public virtual void _Start(GameObject player)
    {
        SetOwnerPlayer(player);
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
        ListenForMoving();
    }

    protected void ListenForMoving()
    {
        if (this.isMoving)
        {
            this.gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, lerpRate);  // move object
            if (GetDistanceFrom(targetPosition) < snapToCenterThreshold)
            {
                if (isServer)
                {
                    // The server should calculate these values to make sure
                    // all clients have the synced values as dictated by the server
                    FindCenter();
                    GoToCenter();
                    NotifyOccupiedTile(true);
                    isMoving = false;
                    RpcStopMoving();
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

    [Server]
    protected virtual void FindCenter() 
    {
        var objectRay = new Ray(this.gameObject.transform.position, Vector3.down);
        RaycastHit tileUnderneathHit;
        if (Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            tileUnderneath = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
            var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position;
            // var result = GetComponent<Transform>().position;
            nearestCenter = result;
            RpcFindCenter(result, this.gameObject.transform.position);
        }
    }
    
    // Tell all clients the nearest center as calculated by the server
    [ClientRpc]
    protected virtual void RpcFindCenter(Vector3 centerPos, Vector3 pos)
    {
        var objectRay = new Ray(pos, Vector3.down);
            RaycastHit tileUnderneathHit;
            if (Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
            {
                tileUnderneath = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
                var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position;
                nearestCenter = result;
            }
        // nearestCenter = centerPos;
    }
    
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
    
    protected virtual Vector3 FindCenterBelowOtherInclusive(GameObject origin) 
        {
            var objectRay = new Ray(origin.transform.position, Vector3.down);
            RaycastHit tileUnderneathHit;
            var status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("BaseTiles"));
            if (!status)
            {
                status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("SlowHex"));
            }
            // if (!status)
            // {
            //     status = Physics.Raycast(objectRay, out tileUnderneathHit, 1000f, 1 << LayerMask.NameToLayer("DangerHex"));
            // }
            if (status) {
                // var tileBelowOrigin = tileUnderneathHit.transform.gameObject.GetComponentInParent<HexCell>();
                var result = tileUnderneathHit.transform.gameObject.GetComponent<Transform>().position ;
                return result;
            }
            else return origin.transform.position;
        }


    protected virtual float GetDistanceFrom(Vector3 targetPos)
    {
        var dist = Mathf.Sqrt(
            Mathf.Pow(this.gameObject.transform.position.x - targetPos.x, 2) +
            Mathf.Pow(this.gameObject.transform.position.z - targetPos.z, 2)
        );
        return dist;
    }

    [Server]
    protected virtual void GoToCenter()
    {
        this.gameObject.transform.position = nearestCenter;
        RpcGoToCenter(nearestCenter);
    }

    // Tell all clients to move their bomb to the center
    [ClientRpc]
    protected virtual void RpcGoToCenter(Vector3 centerPos)
    {
        this.gameObject.transform.position = centerPos;
    }

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
        if (SFX) {
            SFX.SetActive(true);
            yield return new WaitForSeconds(sfxDuration);
            SFX.SetActive(false);
        }
    }

    protected virtual IEnumerator EnableHitbox()
    {
        hitBox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitBox.SetActive(false);
    }

    protected virtual IEnumerator DisableObjectModel()
    {
        GameObject _model = this.gameObject.transform.Find("Model").gameObject;
        if (!_model)
        {
            print("ERROR: Model not found for a Bomb! Make sure the Model's name is 'Model'");
        }
        _model.SetActive(false);
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
        this.triggeringPlayer = triggeringPlayer;  // Testing for client assignment
        Push(edgeIndex, triggeringPlayer);
    }

    protected virtual bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
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
            for (var tileOffset = 1; tileOffset < travelDistanceInHexes; tileOffset++)
            {
                var possiblePosition = gameObject.transform.position +
                                       HexMetrics.edgeDirections[edgeIndex] * HexMetrics.hexSize * tileOffset; 
                // if works then change targetPosition
                var checkForEmptyRay = new Ray(possiblePosition, Vector3.down);
                var checkForObjectRay = new Ray(possiblePosition + new Vector3(0f, 20f, 0f), Vector3.down);
                // Debug.DrawRay(possiblePosition + new Vector3(0f, 20f, 0f), Vector3.down * 20, Color.cyan, 2, true);
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

            triggeringPlayer = other.transform.parent.gameObject;

            // Adjust travel distance based on spin power
            int power = triggeringPlayer.GetComponent<Player>().spinPower;
            Debug.Log("power: " + power);

            int newTravelDistanceInHexes = power + 1;

            NotifyOccupiedTile(false); // Update occupation status of tile
            // Push(edgeIndex, triggeringPlayer); // Push for server too
            RpcPush(edgeIndex, triggeringPlayer, newTravelDistanceInHexes);

            // Set spin hit on player, for event logger
            triggeringPlayer.GetComponent<Player>().spinHit = this.gameObject;
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
        Debug.Log("Extending for "+extensionTime);
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
        this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", fillShaderVal);
    }

    protected virtual void StepFillShader()
    {
        this.model.GetComponent<Renderer>().material.SetFloat("_FillRate", fillShaderVal);  // Fill shader
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
    
            return angle;
        }
}