using UnityEngine;
using UnityEngine.UI;

public class AutoRefresh : MonoBehaviour
{
    private Button _refreshButton;
    [SerializeField] private float autoUpdateInterval = 1f;

    private void Awake()
    {
        _refreshButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(Refresh), 0f, autoUpdateInterval);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Refresh));
    }

    private void Refresh()
    {
        _refreshButton.onClick.Invoke();
    }
}
