using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimHelper : MonoBehaviour
{
    public GameObject radialArrow;
    public GameObject target = null;
    public GameObject targetQuad = null;

    float prevIndex = 0;

    // hard coded values
    private float[] angles = { 60f, 0f, -60f, -120, 180f, 120f};
    private float[] distances = { 130f, 258.7f, 388.5f, 518.4f};


    /// <summary>
    /// The scale offset for how far apart the target scale is from the target
    /// Higher offset = closer to the target
    /// </summary>
    private float quadScaleOffset = 90f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        // If the bomb model is disabled, then the arrow is also disabled (for lasers/plasmas)
        if (!gameObject.transform.root.gameObject.GetComponent<ComboObject>().model.activeSelf)
        {
            radialArrow.SetActive(false);
        }


        // Arrow Rotation
        int newIndex = CalculateRotationEdge(other.gameObject);

        if (prevIndex != newIndex)
        {
            prevIndex = newIndex;

            Quaternion ang = Quaternion.Euler(0, 0, angles[newIndex]);

            LeanTween.rotateLocal(radialArrow, ang.eulerAngles, 0.15f)
                .setEase(LeanTweenType.easeOutExpo);
        }

        // Target Location
        if (target != null)
        {

            int dist = other.gameObject.GetComponent<PlayerSpin>().currentChargeLevel;
            Debug.Log("target: " + dist);

            LeanTween.moveLocalX(target, distances[dist], 0.2f)
                .setEase(LeanTweenType.easeOutExpo);

            LeanTween.moveLocalX(targetQuad, distances[dist] / 2, 0.2f)
                .setEase(LeanTweenType.easeOutExpo);

            LeanTween.scaleX(targetQuad, (130f * dist) + quadScaleOffset, 0.2f)
                .setEase(LeanTweenType.easeOutExpo);
        }

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().StartTween();

        if (target != null) target.gameObject.GetComponent<ColorTween>().StartTween();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().EndTween();

        if (target != null) target.gameObject.GetComponent<ColorTween>().EndTween();
    }

    private int CalculateRotationEdge(GameObject player)
    {
        var playerPosition = player.transform.position;
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

        return edgeIndex;
    }
}
