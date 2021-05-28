using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudioEvents : MonoBehaviour
{
	private MenuAudioManager menuAudioManager;

    private void Awake()
    {
		menuAudioManager = FindObjectOfType<MenuAudioManager>();
		if (!menuAudioManager) Debug.LogError("Could not find component: MenuAudioManager");
	}

    private void Start()
	{
		EventTrigger trigger = GetComponent<EventTrigger>();
		if (!trigger) trigger = this.gameObject.AddComponent<EventTrigger>();

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
		// Subscribed callback may not be disabled entirely, check again here
		if (enabled) menuAudioManager.menuConfirm.Play();
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (enabled) menuAudioManager.menuHover.Play();
	}
}
