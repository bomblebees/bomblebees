using System;
using System.Net.NetworkInformation;
using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] public float updateInterval = 1f;
    private TMP_Text _text;
    public string myPing;
    private SteamNetworkManager _steamNetworkManager;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _steamNetworkManager = FindObjectOfType<SteamNetworkManager>();
    }

    private void OnEnable()
    {
        if (_steamNetworkManager.networkAddress.Equals("localhost"))
        {
            InvokeRepeating(nameof(UpdatePingDisplay), 0f, updateInterval);
        }
        else
        {
            InvokeRepeating(nameof(InitializePingDisplay), 0f, 0.1f);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(UpdatePingDisplay));
        _text.text = "Oms";
        myPing = _text.text;
    }

    private void InitializePingDisplay()
    {
        if (!string.Format("{0}ms", (int)(NetworkTime.rtt * 1000)).Equals("0ms")
            || _steamNetworkManager.networkAddress.Equals("localhost"))
        {
            CancelInvoke(nameof(InitializePingDisplay));
            InvokeRepeating(nameof(UpdatePingDisplay), updateInterval, updateInterval);
        }
        else
        {
            _text.text = "connecting...";
            myPing = _text.text;
        }
    }

    private void UpdatePingDisplay()
    {
        _text.text = string.Format("{0}ms", (int)(NetworkTime.rtt * 1000));
        myPing = _text.text;
    }
}
