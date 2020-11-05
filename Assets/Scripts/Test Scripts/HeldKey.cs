using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeldKey : MonoBehaviour
{
    public void setText()
    {
        this.gameObject.GetComponent<Text>().text = FindObjectOfType<HexGrid>().heldKey.ToString();
    }

}
