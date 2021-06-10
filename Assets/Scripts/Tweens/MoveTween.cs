using UnityEngine;

public class MoveTween : MonoBehaviour
{
    public Vector3 targetPosition;
    public Vector3 endPosition;

    public float initialDelay = 0f;

    public float easeInTime = 0.1f;
    public float easeOutTime = 0.15f;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    public void StartTween()
    {
        LeanTween.moveLocal(gameObject, targetPosition, easeInTime)
            .setEase(easeIn)
            .setDelay(initialDelay)
            .setOnComplete(EndTween);
    }

    public void EndTween()
    {
        LeanTween.moveLocal(gameObject, endPosition, easeOutTime).setEase(easeOut);
    }
}
