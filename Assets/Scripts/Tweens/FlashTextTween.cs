using TMPro;
using UnityEngine;

public class FlashTextTween : MonoBehaviour
{
    [SerializeField] public Color flashColor;

    public float timeBetweenFlashes = 0f;

    public float easeInTime = 0.1f;
    public float easeOutTime = 0.15f;

    public LeanTweenType easeIn = LeanTweenType.linear;
    public LeanTweenType easeOut = LeanTweenType.linear;

    private Color originalColor;
    private bool flashStopped = false;
    private bool firstFlash = true;

    TMP_Text text;

    public void OnEnable()
    {
        text = gameObject.GetComponent<TMP_Text>();
        originalColor = this.gameObject.GetComponent<TMP_Text>().color;
    }

    public void FlashOnce()
    {
        LeanTween.value(gameObject, updateColorCallback, originalColor, flashColor, easeInTime)
            .setEase(easeIn)
            .setOnComplete(EndFlashOnce);

        //LeanTween.colorText(this.gameObject.GetComponent<RectTransform>(), flashColor, 1f)
    }

    public void EndFlashOnce()
    {
        LeanTween.value(gameObject, updateColorCallback, flashColor, originalColor, easeOutTime).setEase(easeOut);
    }

    public void StopFlashContinuous()
    {
        flashStopped = true;
    }

    public void FlashContinuous()
    {
        if (flashStopped) return;

        if (firstFlash)
        {
            LeanTween.value(gameObject, updateColorCallback, originalColor, flashColor, easeInTime)
                .setEase(easeIn)
                .setOnComplete(FlashLoop);
            firstFlash = false;
        } else
        {
            LeanTween.value(gameObject, updateColorCallback, originalColor, flashColor, easeInTime)
                .setEase(easeIn)
                .setDelay(timeBetweenFlashes)
                .setOnComplete(FlashLoop);
        }


    }

    public void FlashLoop()
    {
        LeanTween.value(gameObject, updateColorCallback, flashColor, originalColor, easeOutTime)
            .setEase(easeOut)
            .setOnComplete(FlashContinuous);
    }

    void updateColorCallback(Color val)
    {
        text.color = val;
    }
}
