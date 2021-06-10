using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float rotateSpeed = 5;

    void Update()
    {
        transform.Rotate(Vector3.back, Time.deltaTime * rotateSpeed);
    }
}
