using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class GameSetupController : MonoBehaviour
{
    public bool turnOffOnline = false; // Not required; it just stops a harmless error from occurring - ari
    // Start is called before the first frame update
    void Start()
    {
        if(!turnOffOnline) CreatePlayer();
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Player1"), new Vector3(120f, 0f, 30f), Quaternion.identity);
    }
}
