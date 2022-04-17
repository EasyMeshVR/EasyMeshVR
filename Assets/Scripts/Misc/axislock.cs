using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
public class axislock : MonoBehaviour
{
    public bool unlock = true;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    // Start is called before the first frame update
        

    // Update is called once per frame
    void Update()
    {
        if(lockZ)
        {
            if(gameObject.GetComponent<ConfigurableJoint>() != null)
                return;
            
            unlock = false;
            lockY = false;
            lockX = false;

            // change rigidbody
            //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

            // change grabinteractable
            gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.VelocityTracking;

            // add configjoint and change settings
            gameObject.AddComponent<ConfigurableJoint>();
            gameObject.GetComponent<ConfigurableJoint>().axis = new Vector3(0,0,-1);

            gameObject.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Free;
            gameObject.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            gameObject.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;


            gameObject.GetComponent<ConfigurableJoint>().xDrive = drive;
        }

        if(lockY)
        {
            if(gameObject.GetComponent<ConfigurableJoint>() != null)
                return;
            
            unlock = false;
            lockZ = false;
            lockX = false;

            // change rigidbody
            //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

            // change grabinteractable
            gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.VelocityTracking;

            // add configjoint and change settings
            gameObject.AddComponent<ConfigurableJoint>();
            gameObject.GetComponent<ConfigurableJoint>().axis = new Vector3(0,0,-1);

            gameObject.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Free;
            gameObject.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            gameObject.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;


            gameObject.GetComponent<ConfigurableJoint>().yDrive = drive;
        }

        // x axis is actually z axis
        if(lockX)
        {
            if(gameObject.GetComponent<ConfigurableJoint>() != null)
                return;
            
            unlock = false;
            lockZ = false;
            lockY = false;

            // change rigidbody
            //gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

            // change grabinteractable
            gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.VelocityTracking;

            // add configjoint and change settings
            gameObject.AddComponent<ConfigurableJoint>();
            gameObject.GetComponent<ConfigurableJoint>().axis = new Vector3(0,0,-1);

            gameObject.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Free;

            gameObject.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            gameObject.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive();
            drive.positionDamper = 100f;
            drive.maximumForce = Mathf.Infinity;


            gameObject.GetComponent<ConfigurableJoint>().zDrive = drive;
        }

        if(unlock)
        {
            if(gameObject.GetComponent<ConfigurableJoint>() == null)
            {
                return;
            }

            lockX = false;
            lockY = false;
            lockZ = false;

            
            // change rigidbody
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // change grabinteractable
            gameObject.GetComponent<XRGrabInteractable>().movementType = XRBaseInteractable.MovementType.Instantaneous;

            if(gameObject.GetComponent<ConfigurableJoint>() != null)
            {
                Destroy(gameObject.GetComponent<ConfigurableJoint>());
            }
        }
    }
}
