using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private GameObject dialog;

    public void ShowTutorialScreen()
    {
        dialog.SetActive(true);
    }

    public void UnshowTutorialScreen()
    {
        dialog.SetActive(false);
    }

    public void StartTutorial()
    {
        TutorialDialog tutorial = FindObjectOfType<TutorialDialog>();

        tutorial.dialog.enabled = true;
        tutorial.canvas.SetActive(true);
        tutorial.LoadReferences();

        UnshowTutorialScreen();
    }
}
