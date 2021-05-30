using UnityEngine;

[RequireComponent(typeof(Transform), typeof(CanvasGroup))]
public class ForceScale : MonoBehaviour
{
    [SerializeField] private float scaleX = 1f;
    [SerializeField] private float scaleY = 1f;
    [SerializeField] private float scaleZ = 1f;
    
    private Vector3 _scaleVector3;
    private Transform _transform;
    private CanvasGroup _canvasGroup;
    private float _canvasGroupOriginalAlpha;

    private void Awake()
    {
        _scaleVector3 = new Vector3(scaleX, scaleY, scaleZ);
        _transform = GetComponent<Transform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroupOriginalAlpha = _canvasGroup.alpha;
        
        _canvasGroup.alpha = float.Epsilon;
    }

    private void Update()
    {
        if (_transform.localScale.Equals(_scaleVector3) && _canvasGroup.alpha.Equals(_canvasGroupOriginalAlpha)) return;
        
        _transform.localScale = _scaleVector3;
        _canvasGroup.alpha = _canvasGroupOriginalAlpha;
    }
}
