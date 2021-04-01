using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using Mirror;
using UnityEngine;
using UnityEngine.Audio;
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

    // Start is called before the first frame update
    void Start()
    {
        sliderSoundFX.minValue = minValue;
        sliderSoundFX.maxValue = maxValue;
        sliderMusic.minValue = minValue;
        sliderMusic.maxValue = maxValue;

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

    private void SoundFXMuted()
    {
        iconSoundFX.sprite = spriteSoundFXOff;
        soundFXIsMuted = true;
        audioMixerSoundFX.SetFloat("volumeSoundFX", -80);
    }
    
    private void SoundFXUnmuted()
    {
        iconSoundFX.sprite = spriteSoundFXOn;
        soundFXIsMuted = false;
    }
    
    private void MusicMuted()
    {
        iconMusic.sprite = spriteMusicOff;
        musicIsMuted = true;
        audioMixerMusic.SetFloat("volumeMusic", -80);
    }
    
    private void MusicUnmuted()
    {
        iconMusic.sprite = spriteMusicOn;
        musicIsMuted = false;
    }
}
