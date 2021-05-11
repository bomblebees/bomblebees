using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] public float updateInterval = 5f;

    public int myPingValue;
    public string myPingDisplay;
    public bool isHost;
    public bool isConnecting;
    public int lastSessionPing;
    
    private NetworkManager _networkManager;
    private TMP_Text _text;

    private void Awake()
    {
        _networkManager = FindObjectOfType<SteamNetworkManager>();
        _text = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        if (_networkManager.networkAddress.Equals("localhost"))
        {
            HostStatus();
        }
        else
        {
            ConnectingStatus();
            InvokeRepeating(nameof(InitializePingDisplay), 0f, 0.1f);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
        lastSessionPing = myPingValue;
        myPingValue = 0;
        _text.text = null;
        myPingDisplay = _text.text;
        isHost = false;
        isConnecting = false;
    }

    private void InitializePingDisplay()
    {
        myPingValue = (int) (NetworkTime.rtt * 1000);
        
        switch (myPingValue)
        {
            case 0 when _networkManager.networkAddress.Equals("localhost"):
                Debug.LogWarning("This should never be called");
                HostStatus();
                break;
            case 0:
                ConnectingStatus();
                break;
            default:
            {
                if (myPingValue.Equals(lastSessionPing))
                {
                    ConnectingStatus();
                }
                else
                {
                    CancelInvoke(nameof(InitializePingDisplay));
                    InvokeRepeating(nameof(UpdatePingDisplay), 0f, updateInterval);
                }
                break;
            }
        }
    }

    private void UpdatePingDisplay()
    {
        myPingValue = (int) (NetworkTime.rtt * 1000);
        _text.text = myPingValue + "ms";
        myPingDisplay = _text.text;

        if (_networkManager.networkAddress.Equals("localhost"))
        {
            Debug.LogWarning("This should never be called");
            HostStatus();
        }
    }

    private void HostStatus()
    {
        isConnecting = false;
        isHost = true;
        myPingValue = 0;
        _text.text = "Host";
        myPingDisplay = _text.text;
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
    }

    private void ConnectingStatus()
    {
        isHost = false;
        isConnecting = true;
        myPingValue = 0;
        _text.text = "connecting...";
        myPingDisplay = _text.text;
    }
}
