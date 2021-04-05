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
    [Header("Volume Slider Bound (dB)")]
    [Range(-80, -1)]
    [SerializeField] private float minValue;
    [Range(0, 20)]
    [SerializeField] private float maxValue;
    
    [Header("Custom Default Volume (dB)")]
    [SerializeField] private float defaultSoundFXVolume = 0;
    [SerializeField] private float defaultMusicVolume = 0;

    [Header("Music Audio Source")] 
    [SerializeField] private bool disableMusic;
    [Space]
    [SerializeField] private AudioSource musicMenuScene;
    [Space]
    [SerializeField] private AudioSource musicRoomScene;
    [SerializeField] private bool changeMusicOnRoomScene;
    [SerializeField] private AudioSource musicGameScene;
    [SerializeField] private bool changeMusicOnGameScene;

    [Header("Sprite")]
    [SerializeField] private Sprite spriteSoundFXOn;
    [SerializeField] private Sprite spriteSoundFXOff;
    [SerializeField] private Sprite spriteMusicOn;
    [SerializeField] private Sprite spriteMusicOff;

    [Header("Scene")]
    [Scene][SerializeField] private String sceneRoom;
    [Scene][SerializeField] private String sceneGame;

    [Header("GameObject")]
    [SerializeField] private Slider sliderSoundFX;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Image iconSoundFX;
    [SerializeField] private Image iconMusic;
    [SerializeField] private AudioMixer audioMixerSoundFX;
    [SerializeField] private AudioMixer audioMixerMusic;
    
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

        sliderSoundFX.value = defaultSoundFXVolume;
        sliderMusic.value = defaultMusicVolume;
        
        SetupMuteState();
        
        musicMenuScene.loop = true;
        musicRoomScene.loop = true;
        musicGameScene.loop = true;
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
        SwitchMusicBetweenScene(scene);
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

    private void SwitchMusicBetweenScene(Scene scene)
    {
        if (disableMusic) return;
        
        if (changeMusicOnRoomScene == true && scene.path == sceneRoom)
        {
            StopAllMusic();
            musicRoomScene.Play();
        } 
        else if (changeMusicOnGameScene == true && scene.path == sceneGame)
        {
            StopAllMusic();
            musicGameScene.Play();
        }
        else 
        {
            if (musicMenuScene.isPlaying) return;
            StopAllMusic();
            musicMenuScene.Play();
        }
    }
    
    private void StopAllMusic()
    {
        if (musicMenuScene.isPlaying)
        {
            musicMenuScene.Stop();
        }

        if (musicRoomScene.isPlaying)
        {
            musicRoomScene.Stop();
        }

        if (musicGameScene.isPlaying)
        {
            musicGameScene.Stop();
        }
    }
}
