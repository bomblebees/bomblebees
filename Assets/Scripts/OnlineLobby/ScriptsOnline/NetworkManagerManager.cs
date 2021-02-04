using System.Collections;
using System.Collections.Generic;
using Mirror.Authenticators;
using UnityEngine;

public class NetworkManagerManager : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerManager;
    [SerializeField] private GameObject kcp2NetworkManager;
    [SerializeField] private GameObject steamNetworkManager;
    [SerializeField] private GameObject kcpNetworkManager;
    public void ChooseBasicNetworkManager()
    {
        networkManagerManager.SetActive(false);
        kcp2NetworkManager.SetActive(true);
    }
    
    public void ChooseSteamNetworkManager()
    {
        networkManagerManager.SetActive(false);
        steamNetworkManager.SetActive(true);
    }

    public void ChooseKcpNetworkManager()
    {
        networkManagerManager.SetActive(false);
        kcpNetworkManager.SetActive(true);
    }
}
