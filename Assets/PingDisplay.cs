using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] public float updateInterval = 5f;
    
    public string myPing;
    
    private TMP_Text _text;
    private NetworkManager _networkManager;
    private bool _isHost;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _networkManager = FindObjectOfType<SteamNetworkManager>();
    }

    private void OnEnable()
    {
        if (_networkManager.networkAddress.Equals("localhost"))
        {
            // If host
            _isHost = true;
            InvokeRepeating(nameof(UpdatePingDisplay), 0f, updateInterval);
        }
        else
        {
            // If not host
            _isHost = false;
            InvokeRepeating(nameof(InitializePingDisplay), 0f, 0.1f);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
        _text.text = "0ms";
        myPing = _text.text;
    }

    private void InitializePingDisplay()
    {
        if (!string.Format("{0}ms", (int)(NetworkTime.rtt * 1000)).Equals("0ms") 
            || _networkManager.networkAddress.Equals("localhost"))
        {
            CancelInvoke(nameof(InitializePingDisplay));
            InvokeRepeating(nameof(UpdatePingDisplay), 0f, updateInterval);
        }
        else
        {
            _text.text = "connecting...";
            myPing = _text.text;
        }
    }

    private void UpdatePingDisplay()
    {
        if (!_isHost)
        {
            _text.text = string.Format("{0}ms", (int) (NetworkTime.rtt * 1000));
            myPing = _text.text;
        }
        else
        {
            _text.text = "Host";
            myPing = _text.text;
            CancelInvoke(nameof(UpdatePingDisplay));
        }
    }
}
