using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : ComboObject
{
    public string effectPath = "Prefabs/ComboEffects/Bomb Explosion";

    private void Start()
    {
        StartCoroutine(TickDown());
    }
}
