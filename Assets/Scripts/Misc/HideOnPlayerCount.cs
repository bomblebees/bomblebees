using UnityEngine;

public class HideOnPlayerCount : MonoBehaviour
{
    public enum HideMode
    {
        Disable,
        Invisible,
        Destroy
    }
    
    public HideMode hideMode;
    [Header("If Less Than:")]
    [SerializeField] private int playerCount;

    private void Awake()
    {
        if (FindObjectOfType<NetworkRoomManagerExt>().currentPlayers < playerCount)
        {
            HideThis();
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
