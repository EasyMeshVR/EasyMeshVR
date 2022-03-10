using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// Resize sphere collider radius based on midpoint
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
     void Update()
     {
        thumbstick.Enable();  
        value = thumbstick.ReadValue<Vector2>().x + thumbstick.ReadValue<Vector2>().y;
        // probably need a max to be less than
        // avoid dividing by 0
        if(Mathf.Abs(value) > 0.1f)
        {
            if(!matEnabled)
            {
              sphereVisual.GetComponent<MeshRenderer>().enabled = true;
              matEnabled = true;
            }
            sphereCollider.radius += value/100;
            sphereVisual.transform.localScale += new Vector3(value/50, value/50, value/50);
           // materialSwap = sphereVisual.GetComponent<MeshRenderer>();
          //  materialSwap.material = sphereMat;


        }
        // min values , can change
        if(sphereCollider.radius < 0.05f)
        {
          if(!matEnabled)
          {
            sphereVisual.GetComponent<MeshRenderer>().enabled = true;
            matEnabled = true;
          }
          sphereVisual.GetComponent<MeshRenderer>().enabled = true;

          sphereCollider.radius = .05f;
          sphereVisual.transform.localScale = new Vector3(.1f, .1f, .1f);
          Vector3 spherePos = sphereVisual.transform.position;
        }

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
