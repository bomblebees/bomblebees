using UnityEngine;

public class ForceScale : MonoBehaviour
{
    [SerializeField] private float scale = 1;
    private Vector3 _scaleVector3;
    private Transform _transform;

    private void Awake()
    {
        _scaleVector3 = new Vector3(scale, scale, scale);
        _transform = GetComponent<Transform>();
        _transform.localScale = _scaleVector3;
    }

    private void Update()
    {
        if (_transform.localScale.Equals(_scaleVector3)) return;

        _transform.localScale = _scaleVector3;
    }
}
