using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Transform), typeof(CanvasGroup))]

public class MultiKillAnnouncerTween : MonoBehaviour
{
    [Header("Settings")]
    [Header("Pop-up")]
    [SerializeField] private float scaleBefore;
    [SerializeField] private float scaleAfter = 1f;
    [SerializeField] private float scaleDuration = 1f;
    [SerializeField] private float scaleDelay;
    [SerializeField] private Ease scaleEase = Ease.OutBounce;
    [SerializeField] private AnimationCurve scaleCustomCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private bool scaleUseCustomCurve;

    [Header("Fade out")]
    [SerializeField] private float fadeBefore = 1f;
    [SerializeField] private float fadeAfter;
    [SerializeField] private float fadeDuration = 4f;
    [SerializeField] private float fadeDelay = 1f;
    [SerializeField] private Ease fadeEase = Ease.InExpo;
    [SerializeField] private AnimationCurve fadeCustomCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private bool fadeUseCustomCurve;
    
    private Transform _transform;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _transform.localScale = new Vector3(scaleBefore, scaleBefore, scaleBefore);
        _canvasGroup.alpha = fadeBefore;
        
        if (scaleUseCustomCurve)
        {
            _transform.DOScale(new Vector3(scaleAfter, scaleAfter, scaleAfter), scaleDuration).SetDelay(scaleDelay).SetEase(scaleCustomCurve);
        }
        else
        {
            _transform.DOScale(new Vector3(scaleAfter, scaleAfter, scaleAfter), scaleDuration).SetDelay(scaleDelay).SetEase(scaleEase);
        }

        if (fadeUseCustomCurve)
        {
            _canvasGroup.DOFade(fadeAfter, fadeDuration).SetDelay(fadeDelay).SetEase(fadeCustomCurve);
        }
        else
        {
            _canvasGroup.DOFade(fadeAfter, fadeDuration).SetDelay(fadeDelay).SetEase(fadeEase);
        }
    }
}
