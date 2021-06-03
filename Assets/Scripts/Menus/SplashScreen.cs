using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    public Image background;
    public Image splashIcon;

    public int duration;

    private void OnEnable()
    {
        StartSplashScreenAnims();
    }

    private void StartSplashScreenAnims()
    {
        //ColorTween bgTween =

        background.GetComponent<ColorTween>().StartTween();
        splashIcon.GetComponent<ColorTween>().StartTween();
        //splashIcon.GetComponent<ColorTween>()
        splashIcon.GetComponent<SimpleAnimation.TweenSequence>().PlayAllAnimationsNow();

        StartCoroutine(WaitSplashDuration());
    }

    private IEnumerator WaitSplashDuration()
    {
        yield return new WaitForSeconds(duration);
        OnSplashScreenFadeOut();
        yield return new WaitForSeconds(2);
        OnSplashScreenComplete();
    }



    private void OnSplashScreenFadeOut()
    {
        GameObject.Find("Main Camera").GetComponent<SimpleAnimation.TweenSequence>().PlayAllAnimationsNow();

        FindObjectOfType<FadeCanvas>().StartFadeIn();
    }

    private void OnSplashScreenComplete()
    {
        this.gameObject.SetActive(false);
    }
}
