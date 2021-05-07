using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimHelper : MonoBehaviour
{
    public GameObject radialArrow;

    float prevIndex = 0;

    float[] angles = { 60f, 0f, -60f, -120, 180f, 120f};

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        if (!gameObject.transform.root.gameObject.GetComponent<ComboObject>().model.activeSelf)
        {
            radialArrow.SetActive(false);
        }

        var playerPosition = other.gameObject.transform.position;
        // note: Don't use Vector3.Angle
        var angleInRad = Mathf.Atan2(playerPosition.x - transform.position.x,
            playerPosition.z - transform.position.z);

        var dirFromPlayerToThis =
            (Mathf.Rad2Deg * angleInRad) + 180; // "+ 180" because Unity ranges it from [-180, 180]

        int edgeIndex;

        for (edgeIndex = 0; edgeIndex < HexMetrics.edgeAngles.Length; edgeIndex++)
        {
            if (Mathf.Abs(dirFromPlayerToThis - HexMetrics.edgeAngles[edgeIndex]) <= 30)
            {
                break;
            }
        }

        //Debug.Log("pushedDirAngle= " + (HexMetrics.edgeAngles[edgeIndex] + 270f));
        //Debug.Log("edgeIndex= " + edgeIndex);

        if (prevIndex != edgeIndex)
        {
            prevIndex = edgeIndex;

            Quaternion ang = Quaternion.Euler(0, 0, angles[edgeIndex]);

            LeanTween.rotateLocal(radialArrow, ang.eulerAngles, 0.15f)
                .setEase(LeanTweenType.easeOutExpo);
                
                //LeanRotateZ(newPushedDirAngle, 0.1f);
        }

        // Set spin hit on player, for event logger
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().StartTween();

        //radialArrow.SetActive(true);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().EndTween();

        //radialArrow.SetActive(false);
    }
}
