using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// Resize sphere collider radius
public class ResizeRadius : MonoBehaviour
{
    [SerializeField] SphereCollider sphereCollider;

    [SerializeField] GameObject sphereVisual;

    [SerializeField] public InputAction thumbstick;

    [SerializeField] Material sphereMat;

    MeshRenderer materialSwap;
    

    bool matEnabled = false;
    float timer = 0f;


     float value;

    // Someone help me get thiis out of update
    // Resizes radius using thumbstick
     void Update()
     {
        thumbstick.Enable();  
        value = thumbstick.ReadValue<Vector2>().x + thumbstick.ReadValue<Vector2>().y;

        // probably also need a max to be less than
        // avoid dividing by 0
        if(Mathf.Abs(value) > 0.1f)
        {
            if(!matEnabled)
            {
              sphereVisual.GetComponent<MeshRenderer>().enabled = true;
              matEnabled = true;
            }
            sphereCollider.radius += value/200;
            sphereVisual.transform.localScale += new Vector3(value/100, value/100, value/100);
        }
        // min values , can change
        if(sphereCollider.radius < 0.005f)
        {
          if(!matEnabled)
          {
            sphereVisual.GetComponent<MeshRenderer>().enabled = true;
            matEnabled = true;
          }
          sphereVisual.GetComponent<MeshRenderer>().enabled = true;

          sphereCollider.radius = .005f;
          sphereVisual.transform.localScale = new Vector3(.01f, .01f, .01f);
          Vector3 spherePos = sphereVisual.transform.position;
        }

        // max values , can change
        if(sphereCollider.radius > .3f)
        {
          if(!matEnabled)
          {
            sphereVisual.GetComponent<MeshRenderer>().enabled = true;
            matEnabled = true;
          }
          sphereVisual.GetComponent<MeshRenderer>().enabled = true;

          sphereCollider.radius = .3f;
          sphereVisual.transform.localScale = new Vector3(.6f, .6f, .6f);
          Vector3 spherePos = sphereVisual.transform.position;
        }

        // Show the new size for a second before disappearing
        if(value == 0f)
        {
          timer += Time.deltaTime;
          matEnabled = false;
          if(timer >= 1f)
          {
            sphereVisual.GetComponent<MeshRenderer>().enabled = false;
            timer = 0f;
          }
        }
        

     }

 



     



}
