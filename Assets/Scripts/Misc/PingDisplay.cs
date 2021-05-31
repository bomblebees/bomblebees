using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] public float updateInterval = 5f;

    public int myPingValue;
    public string myPingDisplay = "connecting...";
    public bool isConnected;
    public int lastSessionPing;

    private NetworkManager _networkManager;
    private TMP_Text _text;

    private void Awake()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _text = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        ConnectingStatus();
        InvokeRepeating(nameof(InitializePingDisplay), float.Epsilon, 0.1f);
    }

    private void InitializePingDisplay()
    {
        myPingValue = (int) (NetworkTime.rtt * 1000);

        if (_networkManager.networkAddress.Equals("localhost"))
        {
            HostStatus();
        }
        else if (myPingValue.Equals(0) || myPingValue.Equals(lastSessionPing))
        {
            ConnectingStatus();
        }
        else
        {
            ConnectedStatus();
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
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
        isConnected = true;
        _text.text = "Host";
        myPingDisplay = _text.text;
    }

    private void ConnectingStatus()
    {
        isConnected = false;
        _text.text = "connecting...";
        myPingDisplay = _text.text;
    }

    private void ConnectedStatus()
    {
        isConnected = true;
        CancelInvoke(nameof(InitializePingDisplay));
        InvokeRepeating(nameof(UpdatePingDisplay), float.Epsilon, updateInterval);
    }

    public void PracticeStatus()
    {
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
        isConnected = true;
        _text.text = "Practice Mode";
        myPingDisplay = _text.text;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(InitializePingDisplay));
        CancelInvoke(nameof(UpdatePingDisplay));
        lastSessionPing = myPingValue;
        ConnectingStatus();
    }
}