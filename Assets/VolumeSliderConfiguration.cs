using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using Mirror;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VolumeSliderConfiguration : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] private Slider sliderSoundFX;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Image iconSoundFX;
    [SerializeField] private Image iconMusic;
    [SerializeField] private AudioMixer audioMixerSoundFX;
    [SerializeField] private AudioMixer audioMixerMusic;
    
    [Header("Sprite")]
    [SerializeField] private Sprite spriteSoundFXOn;
    [SerializeField] private Sprite spriteSoundFXOff;
    [SerializeField] private Sprite spriteMusicOn;
    [SerializeField] private Sprite spriteMusicOff;
    
    [Header("Volume Slider Bound (dB)")]
    [Range(-80, -1)]
    [SerializeField] private float minValue;
    [Range(0, 20)]
    [SerializeField] private float maxValue;

    private float originalMinValue;
    private float originalMaxValue;
    
    private bool soundFXIsMuted;
    private bool musicIsMuted;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        sliderSoundFX.minValue = minValue;
        sliderSoundFX.maxValue = maxValue;
        sliderMusic.minValue = minValue;
        sliderMusic.maxValue = maxValue;
        
        SetupMuteState();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupMuteState();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!soundFXIsMuted && Math.Abs(sliderSoundFX.value - sliderSoundFX.minValue) < float.Epsilon)
        {
            SoundFXMuted();
        }

        if (soundFXIsMuted && Math.Abs(sliderSoundFX.value - sliderSoundFX.minValue) > float.Epsilon)
        {
            SoundFXUnmuted();
        }

        if (!musicIsMuted && Math.Abs(sliderMusic.value - sliderMusic.minValue) < float.Epsilon)
        {
            MusicMuted();
        }

        if (musicIsMuted && Math.Abs(sliderMusic.value - sliderMusic.minValue) > float.Epsilon)
        {
            MusicUnmuted();
        }
    }

    void SetupMuteState()
    {
        if (Math.Abs(sliderSoundFX.value - minValue) < float.Epsilon)
        {
            SoundFXMuted();
        }
        else
        {
            SoundFXUnmuted();
        }

        if ( Math.Abs(sliderMusic.value - minValue) < float.Epsilon)
        {
            MusicMuted();
        }
        else
        {
            MusicUnmuted();
        }
    }

    private void SoundFXMuted()
    {
        iconSoundFX.sprite = spriteSoundFXOff;
        audioMixerSoundFX.SetFloat("volumeSoundFX", -80);
        soundFXIsMuted = true;
    }
    
    private void SoundFXUnmuted()
    {
        iconSoundFX.sprite = spriteSoundFXOn;
        soundFXIsMuted = false;
    }
    
    private void MusicMuted()
    {
        iconMusic.sprite = spriteMusicOff;
        audioMixerMusic.SetFloat("volumeMusic", -80);
        musicIsMuted = true;
    }
    
    private void MusicUnmuted()
    {
        iconMusic.sprite = spriteMusicOn;
        musicIsMuted = false;
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixerSoundFX.SetFloat("volumeSoundFX", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixerMusic.SetFloat("volumeMusic", volume);
    }
}
