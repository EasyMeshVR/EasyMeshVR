using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace EasyMeshVR.UI
{
    public class RoomButton : MonoBehaviour
    {
        private ScrollRect scrollRect;
        private EventTrigger eventTrigger;

        private void Start()
        {
            scrollRect = GetComponentInParent<ScrollRect>();
            eventTrigger = GetComponent<EventTrigger>();

            // Create two event triggers to disable the scrolling rectangle
            // when hovered over the join room button.
            EventTrigger.Entry entry;
            UnityAction<BaseEventData> call;

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            call = new UnityAction<BaseEventData>(pointerEnter);
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(call);
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            call = new UnityAction<BaseEventData>(pointerExit);
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(call);
            eventTrigger.triggers.Add(entry);
        }

        private void pointerEnter(BaseEventData eventData)
        {
            scrollRect.enabled = false;
        }
        private void pointerExit(BaseEventData eventData)
        {
            scrollRect.enabled = true;
        }
    }
}
