using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TypingArea : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftTypingHand;
    public GameObject rightTypingHand;

    private void OnTriggerEnter(Collider other)
    {
        GameObject hand = other.GetComponentInParent<XRController>().gameObject;

        if (hand == null) return;

        if (hand == leftHand)
        {
            leftTypingHand.SetActive(true);
        }
        else if (hand == rightHand)
        {
            rightTypingHand.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject hand = other.GetComponentInParent<XRController>().gameObject;

        if (hand == null) return;

        if (hand == leftHand)
        {
            leftTypingHand.SetActive(false);
        }
        else if (hand == rightHand)
        {
            rightTypingHand.SetActive(false);
        }
    }
}
