using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCameraFOV : MonoBehaviour
{
    public Camera parentCam;
    public Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        cam.fieldOfView = parentCam.fieldOfView;
    }
}
