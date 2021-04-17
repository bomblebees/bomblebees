﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHelper : MonoBehaviour
{
    [SerializeField] public Sprite defaultBombIcon;
    [SerializeField] public Sprite blueBombIcon;
    [SerializeField] public Sprite greenBombIcon;
    [SerializeField] public Sprite yellowBombIcon;
    [SerializeField] public Sprite redBombIcon;
    [SerializeField] public Sprite purpleBombIcon;

    public Sprite GetKeySprite(char key)
    {
        switch (key)
        {
            case 'b': return blueBombIcon;
            case 'g': return greenBombIcon;
            case 'y': return yellowBombIcon;
            case 'r': return redBombIcon;
            case 'p': return purpleBombIcon;
            default: return defaultBombIcon;
        }
    }

    public Color GetKeyColor(char key)
    {
        switch (key)
        {
            case 'b': return new Color32(0, 217, 255, 255);
            case 'g': return new Color32(23, 229, 117, 255);
            case 'y': return new Color32(249, 255, 35, 255);
            case 'r': return Color.red;
            case 'p': return new Color32(241, 83, 255, 255);
            case 'w': return new Color32(178, 178, 178, 255);
            case 'e': return Color.white;
            default: return Color.white;
        }
    }

    public string GetBombTextByKey(char key)
    {
        switch (key)
        {
            case 'b': return "Blink";
            case 'g': return "Plasma";
            case 'y': return "Laser";
            case 'r': return "Big";
            case 'p': return "Sludge";
            case 'w': return "Queen Bee";
            default: return "None";
        }
    }
}