using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class InGame_UI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] [Scene] private string RoomScene;
    [SerializeField] private GameObject screenOptions;
    [SerializeField] private GameObject screenHowToPlay;
    [SerializeField] private GameObject screenBombleList;
    
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToggleScreenOptions();
        }
    }

    #region Screen: Options

    public void ToggleScreenOptions()
    {
        screenOptions.SetActive(!screenOptions.activeSelf);
    }
    
    public void ReturnToRoom()
    {
        networkManager.ServerChangeScene(RoomScene);
    }

    public void Quit()
    {
        Application.Quit();
    }

    [SerializeField] private AudioMixer audioMixerSoundFX;
    [SerializeField] private AudioMixer audioMixerMusic;
    
    public void SetSoundFXVolume(float volume)
    {
        audioMixerSoundFX.SetFloat("volumeSoundFX", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixerMusic.SetFloat("volumeMusic", volume);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log("Fullscreen = " + Screen.fullScreen);
    }
    
    #endregion
    
    #region Screen: HOW TO PLAY

    public void ToggleScreenHowToPlay()
    {
        screenHowToPlay.SetActive(!screenHowToPlay.activeSelf);
    }

    #endregion

    #region Screen: BOMBLE LIST

    public void ToggleScreenBombleList()
    {
        screenBombleList.SetActive(!screenBombleList.activeSelf);
    }

    #endregion
}
