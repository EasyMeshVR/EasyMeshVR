using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;
using EasyMeshVR.Multiplayer;
using Photon.Pun;

public class LockAxis : ToolClass
{
    [SerializeField] ChangeMatClosest cm;

    public GameObject currentObj;
    public bool inRadius = false;
    public SphereCollider leftSphere;
    public SphereCollider rightSphere;
    public bool unlocked = true;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

   void OnEnable()
    {
        leftSphere = GameObject.Find("LeftRadius").GetComponent<SphereCollider>();
        rightSphere = GameObject.Find("RightRadius").GetComponent<SphereCollider>();
    }

    public override void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

    // First press - locks to x, 2nd press - locks to y, 3rd press - locks to z, 4th - unlocked
    public override void PrimaryAction()
    {
        if(!inRadius)
            return;

        if(currentObj == null)
            return;

        if(unlocked)
        {
            lockX = true;
            unlocked = false;
            lockY = false;
            lockZ = false;
            lockAxis();
            return;
        }

        if(lockX)
        {
            lockX = false;
            lockY = true;
            lockZ = false;
            unlocked = false;

            lockAxis();
            return;
        }

         if(lockY)
        {
            lockX = false;
            lockY = false;
            lockZ = true;
            unlocked = false;

            lockAxis();
            return;
        }

         if(lockZ)
        {
            lockX = false;
            lockY = false;
            lockZ = false;
            unlocked = true;
            Unlock();
            return;
        }
    }

    public override void SecondaryAction()
    {
        if(!inRadius)
            return;

        if(currentObj == null)
            return;

       // unlocked = true;
      //  lockX = false;
       // lockY = false;
       // lockZ = false;
        Unlock();
    }

    // Locks X-axis relative to player 1 (Z axis in Unity)
    public void lockAxis()
    {
        if(unlocked || currentObj == null)
            return;
    
        // reinstantiate config joint every time
        if(currentObj.GetComponent<ConfigurableJoint>() != null)
            Destroy(currentObj.GetComponent<ConfigurableJoint>());

        // change rigidbody
        currentObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        // change grabinteractable
        currentObj.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.VelocityTracking;
        // add configjoint and change settings
        currentObj.AddComponent<ConfigurableJoint>();
        currentObj.GetComponent<ConfigurableJoint>().axis = new Vector3(0,0,-1);
        
        // Lock X axis relative to player 1 spawn (Z axis)
        if(lockX)
        {
            currentObj.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Free;

            currentObj.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;
            currentObj.GetComponent<ConfigurableJoint>().zDrive = drive;
            return;
        }
        // Lock Y axis
        if(lockY)
        {
            currentObj.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Free;
            currentObj.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            currentObj.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;
            currentObj.GetComponent<ConfigurableJoint>().yDrive = drive;
            return;
        }
        // Lock Z axis relative to player 1 spawn (X axis)
        if(lockZ)
        {
            currentObj.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Free;
            currentObj.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            currentObj.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            currentObj.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;
            currentObj.GetComponent<ConfigurableJoint>().xDrive = drive;
            return;
        }
    }

    public void Unlock()
    {
        if(currentObj == null)
            return;
    
        if(currentObj.GetComponent<ConfigurableJoint>() == null)
            return;
        
        // change rigidbody
        currentObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        // change grabinteractable
        currentObj.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.Instantaneous;

        if(currentObj.GetComponent<ConfigurableJoint>() != null)
        {
            Destroy(currentObj.GetComponent<ConfigurableJoint>());
        }
    }



    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vertex") || other.CompareTag("Edge") || other.CompareTag("Face"))
        {
           // currentObj = other.gameObject;
           currentObj = cm.nearObject; 
            inRadius = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //if(!switchControllers.rayActive)
       // {
           // Unlock();
            inRadius = false;
            currentObj = null;
        //}
    }

    public override void Disable()
    {
        isEnabled = false;
    }

    public override void Enable()
    {
        isEnabled = true;
    }
}
