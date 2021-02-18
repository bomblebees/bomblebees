﻿using System;
using UnityEngine;
using UnityEngine.Audio;
using Mirror;

public class AudioManager : NetworkBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = .2f;
        [Range(.1f, 3f)] public float pitch = 1;

        [HideInInspector] public AudioSource source;
    }

    public Sound[] sounds;

    [ClientCallback]
    public override void OnStartClient()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
        }
    }

    [Client]
    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.Play();
    }

    [Client]
    public void StopPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
    }
}