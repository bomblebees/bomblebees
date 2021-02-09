using System.Collections;
using System.Collections.Generic;
using Mirror.Authenticators;
using UnityEngine;

public class NetworkManagerManager : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerManager;
    [SerializeField] private GameObject steam3NetworkManager;
    [SerializeField] private GameObject kcp3NetworkManager;
    [SerializeField] private GameObject kcp2NetworkManager;
    [SerializeField] private GameObject steamNetworkManager;
    [SerializeField] private GameObject kcpNetworkManager;
    
    public void ChooseSteam3NetworkManager()
    {
        networkManagerManager.SetActive(false);
        steam3NetworkManager.SetActive(true);
    }
    public void ChooseKcp3NetworkManager()
    {
        networkManagerManager.SetActive(false);
        kcp3NetworkManager.SetActive(true);
    }
    
    public void ChooseKcp2NetworkManager()
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
