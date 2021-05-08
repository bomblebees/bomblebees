using System;
using Mirror;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] public float updateInterval = 1f;
    private TMP_Text _text;
    public string myPing;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdatePingDisplay), 0f, updateInterval);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(UpdatePingDisplay));
    }

    private void UpdatePingDisplay()
    {
        _text.text = string.Format("{0}ms", (int)(NetworkTime.rtt * 1000));
        myPing = _text.text;
    }
}
