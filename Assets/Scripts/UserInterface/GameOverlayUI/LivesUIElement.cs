using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LivesUIElement : MonoBehaviour
{
    [SerializeField] public GameObject livesObject;
    [SerializeField] public Image avatar;
    [SerializeField] public RawImage background;
    [SerializeField] public GameObject[] hearts;
    [SerializeField] public TMP_Text playerName;
    [SerializeField] public TMP_Text livesCounter;
    public GameObject player; // the player gameobject that owns this lives elem
}
