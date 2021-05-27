using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
	public AudioSource menuHover;
	public AudioSource menuConfirm;
	public AudioSource menuCancel;
	public AudioSource menuReady;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlayReady()
	{
		menuReady.Play();
	}
}
