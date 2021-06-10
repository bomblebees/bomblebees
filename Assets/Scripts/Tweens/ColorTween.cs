using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorTween : MonoBehaviour
{
    public Color targetColor;
    public Color endColor;

    public float easeInTime;
    public float easeOutTime;

    public float startDelay = 0f;
    public float finishDelay = 0f;

    public float timeBetweenLoop;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    public bool startOnEnabled = false;
    public bool endAfterFinish = true;

    private void OnEnable()
    {
        if (startOnEnabled) StartTween();
    }

    private int cur = 0;

    public void StartTween()
    {
        // Cancel the tween if we are already playing
        LeanTween.cancel(cur);

        if (endAfterFinish)
        {
            cur = LeanTween.value(gameObject, updateColorCallback, endColor, targetColor, easeInTime)
                .setEase(easeIn)
                .setDelay(startDelay)
                .setOnComplete(EndTween)
                .id;
        } else
        {
            cur = LeanTween.value(gameObject, updateColorCallback, endColor, targetColor, easeInTime)
                .setEase(easeIn)
                .setDelay(startDelay)
                .id;
        }
    }

    public void EndTween()
    {
        cur = LeanTween.value(gameObject, updateColorCallback, targetColor, endColor, easeOutTime)
            .setEase(easeOut)
            .setDelay(finishDelay)
            .id;
    }

    private bool loopStarted = false;
    private bool loopEnd = false;

    public void LoopTween()
    {
        if (loopStarted) return;

        loopStarted = true;


        LeanTween.delayedCall(gameObject, timeBetweenLoop, () => {
            if (loopEnd) return;

            cur = LeanTween.value(gameObject, updateColorCallback, endColor, targetColor, easeInTime)
                .setEase(easeIn)
                .setLoopPingPong(1)
                .id;

        }).setRepeat(-1);

    }

    public void EndLoopTween()
    {
        // Cancel the tween if we are already playing
        LeanTween.cancel(cur);
        loopEnd = true;
    }

    void updateColorCallback(Color val)
    {
        if (GetComponent<Image>()) GetComponent<Image>().color = val;
        if (GetComponent<RawImage>()) GetComponent<RawImage>().color = val;
        if (GetComponent<TMP_Text>()) gameObject.GetComponent<TMP_Text>().color = val;
    }
}
