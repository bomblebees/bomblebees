using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VolumeSliderLinker : MonoBehaviour
{
    [Header("Global")]
    [SerializeField] private Slider sliderSoundFXGlobal;
    [SerializeField] private Slider sliderMusicGlobal;
    [SerializeField] private Image iconSoundFXGlobal;
    [SerializeField] private Image iconMusicGlobal;

    [Header("Local")]
    [SerializeField] private Slider sliderSoundFXLocal;
    [SerializeField] private Slider sliderMusicLocal;
    [SerializeField] private Image iconSoundFXLocal;
    [SerializeField] private Image iconMusicLocal;

    private bool soundFXIsMuted;
    private bool musicIsMuted;

    private void Awake()
    {
        sliderSoundFXGlobal = GameObject.Find("Global Sound FX Slider").GetComponent<Slider>();
        sliderMusicGlobal = GameObject.Find("Global Music Slider").GetComponent<Slider>();
        iconSoundFXGlobal = GameObject.Find("Global Sound FX Icon").GetComponent<Image>();
        iconMusicGlobal = GameObject.Find("Global Music Icon").GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SyncSlider(sliderSoundFXLocal, sliderSoundFXGlobal);
        SyncIcon(iconSoundFXLocal, iconSoundFXGlobal);

        SyncSlider(sliderMusicLocal, sliderMusicGlobal);
        SyncIcon(iconMusicLocal, iconMusicGlobal);

        sliderSoundFXLocal.onValueChanged.AddListener(delegate
        {
            SyncSlider(sliderSoundFXGlobal, sliderSoundFXLocal);
        });
        
        sliderMusicLocal.onValueChanged.AddListener(delegate
        {
            SyncSlider(sliderMusicGlobal, sliderMusicLocal);
        });
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
        SyncSlider(sliderSoundFXLocal, sliderSoundFXGlobal);
        SyncIcon(iconSoundFXLocal, iconSoundFXGlobal);

        SyncSlider(sliderMusicLocal, sliderMusicGlobal);
        SyncIcon(iconMusicLocal, iconMusicGlobal);
    }

    void Update()
    {
        if (!soundFXIsMuted && Math.Abs(sliderSoundFXGlobal.value - sliderSoundFXGlobal.minValue) < float.Epsilon)
        {
            SoundFXMuted();
        }

        if (soundFXIsMuted && Math.Abs(sliderSoundFXGlobal.value - sliderSoundFXGlobal.minValue) > float.Epsilon)
        {
            SoundFXUnmuted();
        }

        if (!musicIsMuted && Math.Abs(sliderMusicGlobal.value - sliderMusicGlobal.minValue) < float.Epsilon)
        {
            MusicMuted();
        }

        if (musicIsMuted && Math.Abs(sliderMusicGlobal.value - sliderMusicGlobal.minValue) > float.Epsilon)
        {
            MusicUnmuted();
        }
    }

    private void SoundFXMuted()
    {
        SyncIcon(iconSoundFXLocal, iconSoundFXGlobal);
        soundFXIsMuted = true;
    }
    
    private void SoundFXUnmuted()
    {
        SyncIcon(iconSoundFXLocal, iconSoundFXGlobal);
        soundFXIsMuted = false;
    }
    
    private void MusicMuted()
    {
        SyncIcon(iconMusicLocal, iconMusicGlobal);
        musicIsMuted = true;
    }
    
    private void MusicUnmuted()
    {
        SyncIcon(iconMusicLocal, iconMusicGlobal);
        musicIsMuted = false;
    }

    private void SyncSlider(Slider slider1, Slider slider2)
    {
        slider1.minValue = slider2.minValue;
        slider1.maxValue = slider2.maxValue;
        slider1.value = slider2.value;
    }

    private void SyncIcon(Image icon1, Image icon2)
    {
        icon1.sprite = icon2.sprite;
    }
}
