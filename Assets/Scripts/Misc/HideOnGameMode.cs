using UnityEngine;

public class HideOnGameMode : MonoBehaviour
{
    public enum HideMode
    {
        Disable,
        Invisible,
        Destroy
    }

    public enum GameMode
    {
        Standard,
        ComboFrenzy
    }

    public HideMode hideMode;
    public GameMode gameMode;

    private void Awake()
    {
        if (gameMode == GameMode.Standard)
        {
            if (FindObjectOfType<LobbySettings>().GetGamemode() is StandardGamemode)
            {
                HideThis();
            }
        }
        else if (gameMode == GameMode.ComboFrenzy)
        {
            if (FindObjectOfType<LobbySettings>().GetGamemode() is ComboGamemode)
            {
                HideThis();
            }
        }
    }

    private void HideThis()
    {
        if (hideMode == HideMode.Disable)
        {
            gameObject.SetActive(false);
        }
        else if (hideMode == HideMode.Invisible)
        {
            if (!gameObject.GetComponent<CanvasGroup>().Equals(null))
            {
                gameObject.GetComponent<CanvasGroup>().alpha = float.Epsilon;
            }
            else
            {
                CanvasRenderer[] canvasRenderers = gameObject.GetComponentsInChildren<CanvasRenderer>();
                foreach (var canvasRenderer in canvasRenderers)
                {
                    canvasRenderer.SetAlpha(float.Epsilon);
                }
            }
        }
        else if (hideMode == HideMode.Destroy)
        {
            Destroy(gameObject);
        }
    }
}