using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEngine;

// Required Children:
// - VFX
// - SFX
// - Hitbox
public class ComboObject : MonoBehaviour
{
    private bool isMoving = false;  // isMoving: Whether or not the object is moving after being pushed
    private float travelDistanceInHexes = 2;
    private float pushedDirAngle = 30;
    public float lerpRate = 0.15f;  // The speed at which the object is being pushed
    public Vector3 targetPosition;  // The position that the tile wants to move to after being pushed
    public float snapToCenterThreshold = 0.5f;
    
    public Vector3 nearestCenter;
    public HexCell tileUnderneath;

    public float tickDuration = 4f;
    public float vfxDuration = 4f;
    public float sfxDuration = 4f;
    public float hitboxDuration = 4f;
    public float lifeDuration = 8f;
    public bool useUniversalVal = false; // Replaces the other durations (except lifeDuration)
    public float universalVal = 4f;
    

    protected virtual void Start()
    {
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
        StartCoroutine(TickDown());
    }
    
    
    protected void Update()
    {
        ListenForMoving();
    }

    protected void ListenForMoving()
    {
        if (this.isMoving)
        {
            this.gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, lerpRate);
            if (GetDistanceFrom(targetPosition) < snapToCenterThreshold)
            {
                Debug.Log("snapping to center");
                FindCenter();
                GoToCenter();
                isMoving = false;
            }
        }
    }

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
        }
    }

    protected virtual float GetDistanceFrom(Vector3 targetPos)
    {
        var dist = Mathf.Sqrt(
            Mathf.Pow(this.gameObject.transform.position.x - targetPos.x, 2) +
            Mathf.Pow(this.gameObject.transform.position.z - targetPos.z, 2)
        );
        return dist;
    }

    protected virtual void GoToCenter()
    {
        this.gameObject.transform.position = nearestCenter;
    }

    protected virtual void NotifyOccupiedTile(bool val)
    {
        Debug.Log("notified");
        tileUnderneath.SetOccupiedByComboObject(val);
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
        yield return new WaitForSeconds(lifeDuration);
        DestroySelf();
    }

    protected virtual void RecursiveWait()
    {
    }

    protected virtual void StopVelocity()
    {
        // wait if not at center
        var rigidBody = this.GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;
    }

    protected virtual IEnumerator EnableVFX()
    {
        this.gameObject.transform.Find("VFX").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(vfxDuration);
        else yield return new WaitForSeconds(universalVal);
    }

    protected virtual IEnumerator EnableSFX()
    {
        this.gameObject.transform.Find("SFX").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(sfxDuration);
        else yield return new WaitForSeconds(universalVal);
    }

    protected virtual IEnumerator EnableHitbox()
    {
        this.gameObject.transform.Find("Hitbox").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(hitboxDuration);
        else yield return new WaitForSeconds(universalVal);
    }

    protected virtual IEnumerator DisableObjectModel()
    {
        this.gameObject.transform.Find("Model").gameObject.SetActive(false);
        yield return new WaitForSeconds(lifeDuration);
    }

    protected virtual IEnumerator DisableObjectCollider()
    {
        if (!this.gameObject.GetComponent<SphereCollider>())
        {
            Debug.Log(
                "ComboObject.cs: ComboObject either looking for incorrect Collider " +
                "type or you need to overwrite in ComboObject subclass");
        }

        this.gameObject.GetComponent<SphereCollider>().enabled = false;
        yield return new WaitForSeconds(lifeDuration);
    }

    protected virtual void PunchedAction(int edgeIndex)
    {
        this.Push(edgeIndex);
    }

    protected virtual void Push(int edgeIndex)
    {
        var rigidBody = this.GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            Debug.LogError("ComboObject.cs: ComboObject has no RigidBody component.");
        }
        else
        {
            // Update occupation status of tile
            NotifyOccupiedTile(false);
            
            targetPosition = this.gameObject.transform.position + HexMetrics.edgeDirections[edgeIndex] * HexMetrics.hexSize * travelDistanceInHexes;
            this.isMoving = true;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
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

            Debug.Log("target dir is " + pushedDirAngle);
            this.PunchedAction(edgeIndex);
        }
    }

    protected virtual void DestroySelf()
    {
        NotifyOccupiedTile(false);
        Destroy(this.gameObject);
    }
}