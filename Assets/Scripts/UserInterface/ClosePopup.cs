using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosePopup : MonoBehaviour
{
    public void OnClickButtonClose()
    {
        this.gameObject.SetActive(false);
    }
}
