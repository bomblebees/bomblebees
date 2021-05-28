using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialog : MonoBehaviour
{
    [SerializeField] public RPGTalk dialog;
    [SerializeField] public GameObject canvas;

    [SerializeField] private TMP_Text nextText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressBar;

    // The current tutorial section we are on
    public int section = 0;

    private bool cameraInitalized = false;

    // Update is called once per frame
    void Update()
    {
        if (section == 0) // movement
        {
            if (dialog.cutscenePosition == 3 && !cameraInitalized)
            {
                cameraInitalized = true;
                FindObjectOfType<CameraFollow>().InitCameraFollow();
            }

            if (dialog.cutscenePosition == 4 && dialog.enablePass)
            {
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Movement();
        } else if (section == 1) // swapping 
        {
            if (dialog.cutscenePosition == 5 && dialog.enablePass)
            {
                // Reset the value for assessment
                curCombos = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Swapping();
        } else if (section == 2) // placing
        {
            if (dialog.cutscenePosition == 4 && dialog.enablePass)
            {
                // Reset the value for assessment
                curPlaces = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Placing();
        } else if (section == 3) // spinning
        {
            if (dialog.cutscenePosition == 4 && dialog.enablePass)
            {
                // Reset the value for assessment
                curSpins = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_Spinning();
        } else if (section == 4) // spin charging
        {
            if (dialog.cutscenePosition == 3 && dialog.enablePass)
            {
                // Reset the value for assessment
                curChargeSpins = 0;
                dialog.enablePass = false;
                nextText.enabled = false;
            }

            // Assess when paused
            if (!dialog.enablePass) Assess_ChargeSpinning();
        }
    }

    private void Update_Progress(float percentage)
    {
        progressBar.fillAmount = percentage;

        progressText.text = String.Format("{0:0}", percentage * 100) + " %";

        // Reset after 100% reached
        if (percentage >= 1)
        {
            progressBar.gameObject.SetActive(false);
            progressBar.fillAmount = 0;
            progressText.text = "0 %";
        }
    }


    // Cache inputs
    private float horizontalAxis;
    private float verticalAxis;

    private float movedTime = 0f;
    private float moveThreshold = 3f;
    public void Assess_Movement()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            // Should hold it for a few seconds before moving on
            movedTime += Time.deltaTime;

            float percentage = movedTime / moveThreshold;

            Update_Progress(percentage);

            if (movedTime >= moveThreshold)
            {
                // Begin new talk and enable passing
                section++; 
                dialog.NewTalk("swap_begin", "swap_end");
                dialog.enablePass = true;
                nextText.enabled = true;
            }

        }
    }

    public int curCombos = 0;
    private int comboThreshold = 5;
    public void Assess_Swapping()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curCombos / (float)comboThreshold;

        Update_Progress(percentage);

        if (curCombos >= comboThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("place_begin", "place_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curPlaces = 0;
    private int placeThreshold = 5;
    public void Assess_Placing()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curPlaces / (float)placeThreshold;

        Update_Progress(percentage);

        if (curPlaces >= placeThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("spin_begin", "spin_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curSpins = 0;
    private int spinThreshold = 3;
    public void Assess_Spinning()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curSpins / (float)spinThreshold;

        Update_Progress(percentage);

        if (curSpins >= spinThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("charge_begin", "charge_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }

    public int curChargeSpins = 0;
    private int chargeThreshold = 3;
    public void Assess_ChargeSpinning()
    {
        if (!progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);

        float percentage = (float)curChargeSpins / (float)chargeThreshold;

        Update_Progress(percentage);

        if (curChargeSpins >= chargeThreshold)
        {
            // Begin new talk and enable passing
            section++;
            dialog.NewTalk("outro_begin", "outro_end");
            dialog.enablePass = true;
            nextText.enabled = true;
        }
    }
}
