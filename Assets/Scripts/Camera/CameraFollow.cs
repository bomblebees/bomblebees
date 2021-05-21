using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public List<Transform> targets;

    public Vector3 minArena = new Vector3(131, 443, -174);
    public Vector3 maxArena = new Vector3(89, 443, -186);

    public Vector3 offset;
    public float smoothTime = .5f;
    public float minZoom = 40f;
    public float maxZoom = 10f;
    public float zoomLimiter = 50f;

    private Camera cam;

    private Vector3 velocity;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void InitCameraFollow()
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player p in players) targets.Add(p.transform);
    }

    private void LateUpdate()
    {
        if (targets.Count == 0) return;

        MoveCamera();
        ZoomCamera();
    }

    void ZoomCamera()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    void MoveCamera()
    {
        Vector3 centerPoint = GetCenterPoint();

        Vector3 newPosition = centerPoint + offset;

        newPosition = new Vector3(
                        Mathf.Clamp(newPosition.x, maxArena.x, minArena.x),
                        Mathf.Clamp(newPosition.y, maxArena.y, minArena.y),
                        Mathf.Clamp(newPosition.z, maxArena.z, minArena.z)
                    );

        //newPosition = Vector3.Min(newPosition, minArena);
        //newPosition = Vector3.Max(newPosition, maxArena);

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach (Transform t in targets)
        {
            bounds.Encapsulate(t.position);
        }

        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach(Transform t in targets)
        {
            bounds.Encapsulate(t.position);
        }

        return bounds.center;
    }
}