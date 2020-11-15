using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Instantiate(Resources.Load("Prefabs/Hex Grid"));
        Instantiate(Resources.Load("Prefabs/Player1"));
        Instantiate(Resources.Load("Prefabs/Development UI"));
        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Main Camera"));
        // Instantiate(Resources.Load("Prefabs/SceneObjects/Level1/Directional Light"));
    }
}
