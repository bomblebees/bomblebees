using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudioEvents : MonoBehaviour
{
	public AudioSource buttonClickSound;
	public AudioSource buttonHoverSound;

	private void Start()
	{
		EventTrigger trigger = GetComponent<EventTrigger>();

		EventTrigger.Entry buttonClickEvent = new EventTrigger.Entry();
		EventTrigger.Entry buttonHoverEvent = new EventTrigger.Entry();

		buttonClickEvent.eventID = EventTriggerType.PointerClick;
		buttonHoverEvent.eventID = EventTriggerType.PointerEnter;

		buttonClickEvent.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
		buttonHoverEvent.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });

		trigger.triggers.Add(buttonClickEvent);
		trigger.triggers.Add(buttonHoverEvent);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		buttonClickSound.Play();
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		buttonHoverSound.Play();
	}
}
