using System;
using UnityEngine;
using UnityEngine.Audio;
using Mirror;
using System.Collections.Generic;

public class AudioManager : NetworkBehaviour
{
    private EventManager eventManager;

    [ServerCallback]
    public override void OnStartServer()
    {
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

        eventManager.EventPlayerSwap += ServerPlayComboSound;
        eventManager.EventBombPlaced += RpcPlayPlaceSound;
        eventManager.EventEndRound += ServerPlayerWhistleSound;
    }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = .2f;
        [Range(-3f, 3f)] public float pitch = 1;
        [Range(0f, 1f)] public float spatialBlend = 0;

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
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = s.spatialBlend;
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

    [Server]
    public void ServerPlayComboSound(char oldKey, char newKey, bool combo, GameObject player)
    {
        if (combo) RpcPlayComboSound(oldKey);

        // For specific combo noises, add switch statement here.
        // Use variable oldKey to determine what color combo was made
    }

    [ClientRpc]
    public void RpcPlayComboSound(char comboKey)
    {
        PlaySound("comboCreation");
    }

    [ClientRpc]
    public void RpcPlayPlaceSound(GameObject bomb, GameObject player)
    {
        PlaySound("bombPlace");
    }

    [Server] public void ServerPlayerWhistleSound(List<Player> players) { RpcPlayWhistleSound(); }

    [ClientRpc]
    public void RpcPlayWhistleSound()
    {
        PlaySound("endWhistle");
    }
}
