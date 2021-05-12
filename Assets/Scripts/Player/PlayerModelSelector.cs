using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelSelector : MonoBehaviour
{
    [Tooltip("The scale of the player model")]
    [SerializeField] private float modelScale = 1.111111f;

    [Tooltip("Selects model based on character chosen.")]
    [SerializeField] private GameObject[] playerModels = new GameObject[4];

    void Start()
    {
        // Get the character code
        int code = this.GetComponentInParent<Player>().characterCode;

        // Instantiate the object model
        GameObject model = Instantiate(
            playerModels[code],
            this.transform.position,
            Quaternion.identity,
            this.transform);

        // Rescale the model
        model.transform.localScale = new Vector3(modelScale, modelScale, modelScale);

        // set the hair color - (temp, remove when models are in)
        Color color = this.GetComponentInParent<Player>().playerColor;
        model.transform.Find("hair_g").gameObject.GetComponent<Renderer>().materials[0].SetColor("_BaseColor", color);
    }
}
