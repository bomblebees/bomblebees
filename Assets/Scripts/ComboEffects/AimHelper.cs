using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimHelper : MonoBehaviour
{
    public GameObject radialArrow;
    public GameObject target = null;
    public GameObject targetQuad = null;

    // caches
    float prevIndex = -1;
    float prevDist = -1;

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

            if (target != null)
            {
                target.SetActive(false);
                targetQuad.SetActive(false);
            }
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

            int newDist = other.gameObject.GetComponent<PlayerSpin>().currentChargeLevel;

            if (prevDist != newDist)
            {
                prevDist = newDist;

                // Move the target
                LeanTween.moveLocalX(target, distances[newDist], 0.2f)
                    .setEase(LeanTweenType.easeOutExpo);

                // Apply bounce animation to the target (ignores first move)
                if (other.gameObject.GetComponent<PlayerSpin>().currentChargeLevel > 0)
                    target.GetComponent<ScaleTween>().StartTween();

                // Move the quad to the center of the target and bomb
                LeanTween.moveLocalX(targetQuad, distances[newDist] / 2, 0.2f)
                    .setEase(LeanTweenType.easeOutExpo);

                // Scale the quad accordingly
                LeanTween.scaleX(targetQuad, (130f * newDist) + quadScaleOffset, 0.2f)
                    .setEase(LeanTweenType.easeOutExpo);
            }

        }

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().StartTween();

        if (target != null)
        {
            target.gameObject.GetComponent<ColorTween>().StartTween();
            targetQuad.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "LocalPlayer") return;

        radialArrow.gameObject.GetComponentInChildren<ColorTween>().EndTween();

        if (target != null)
        {
            target.gameObject.GetComponent<ColorTween>().EndTween();
            targetQuad.SetActive(false);
        }
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
