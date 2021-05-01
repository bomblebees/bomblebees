using System;
using UnityEngine;
using UnityEngine.Audio;
using Mirror;
using System.Collections.Generic;

public class AudioManager : NetworkBehaviour
{
    private EventManager eventManager;
	private RoundManager roundManager;

    [ServerCallback]
    public override void OnStartServer()
    {
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");

		roundManager = RoundManager.Singleton;
		if (roundManager == null) Debug.LogError("Cannot find Singleton: RoundManager");

        eventManager.EventPlayerSwap += ServerPlayComboSound;
        eventManager.EventBombPlaced += RpcPlayPlaceSound;
        eventManager.EventEndRound += ServerPlayerWhistleSound;
        eventManager.EventPlayerSpin += RpcPlayHitSound;

		Debug.Log("eventManager: " + eventManager);
		Debug.Log("roundManager: " + roundManager);
		// wanted to try subscribing to an event in RoundManager instead of EventManager for player eliminated sound
		// The other way would be to put the event in EventManager instead of RoundManager and invoke in RoundManager?

		roundManager.EventPlayerEliminated += RpcPlayPlayerEliminatedSound;
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
    [SerializeField] private AudioMixerGroup audioMixerGroup;

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
            s.source.outputAudioMixerGroup = audioMixerGroup;
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
    public void ServerPlayComboSound(char oldKey, char newKey, bool combo, GameObject player, int numBombsAwarded)
    {
        if (combo) RpcPlayComboSound(oldKey, numBombsAwarded, player);

        // For specific combo noises, add switch statement here.
        // Use variable oldKey to determine what color combo was made
    }

    [ClientRpc]
    public void RpcPlayComboSound(char comboKey, int numBombs, GameObject player)
    {
		// if the client player game object is local player (name gets changed on client start in Player)
		if (player.name == "LocalPlayer")
		{
			// play different bomb sounds for different size combo
			switch (numBombs)
			{
				case 1:
					PlaySound("comboCreation1");
					break;
				case 2:
					PlaySound("comboCreation2");
					break;
				case 3:
					PlaySound("comboCreation3");
					break;
				default:
					PlaySound("comboCreation3");
					break;
			}
			// PlaySound("comboCreation");
		}
    }

    [ClientRpc]
    public void RpcPlayPlaceSound(GameObject bomb, GameObject player)
    {
		// Play different bomb sound for different component (bomb) type

		if (bomb.TryGetComponent(out BombObject bombComponent)) {
			// "Kickable" bomb placed, play respective play sound type
			PlaySound("bombPlace");
		}
		else if (bomb.TryGetComponent(out SludgeObject sludgeComponent))
		{
			// Deployable placed, play deployable sound fx
			PlaySound("bombPlace");
		}
		else if (bomb.TryGetComponent(out LaserObject laserComponent))
		{
			// Deployable placed, play deployable sound fx
			PlaySound("deployablePlace");
		}
		else if (bomb.TryGetComponent(out PlasmaObject plasmaComponent))
		{
			// Deployable placed, play deployable sound fx
			PlaySound("deployablePlace");
		}
		else
		{
			// Play default sound if type is none of these
			PlaySound("bombPlace");
		}
    }

	[Server] public void ServerPlayerWhistleSound(List<Player> players) { RpcPlayWhistleSound(); }

	[ClientRpc]
	public void RpcPlayPlayerEliminatedSound(GameObject eliminatedPlayer)
	{
		Debug.Log("Play player eliminated sound");
		PlaySound("playerEliminated");
	}

	[ClientRpc]
    public void RpcPlayWhistleSound()
    {
        PlaySound("endWhistle");
    }

    [ClientRpc]
    public void RpcPlayHitSound(GameObject player, GameObject bomb)
    {
        PlaySound("bombHit");
    }
}
