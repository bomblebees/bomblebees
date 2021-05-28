using TMPro;
using UnityEngine;

public class Announcer : MonoBehaviour
{
    [SerializeField] private TMP_Text announcerText;

    public void Announce(string message)
    {
        // set the message
        announcerText.text = message;

        // reset to 0 scale
        announcerText.gameObject.transform.localScale = new Vector3(0, 0, 0);

        // stop any running tweens
        announcerText.GetComponent<SimpleAnimation.TweenSequence>().StopAllCoroutines();

        // start the tween
        announcerText.GetComponent<SimpleAnimation.TweenSequence>().PlayScaleAnimations();
    }
}
