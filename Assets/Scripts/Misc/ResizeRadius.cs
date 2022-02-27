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

     float value;

    //  void Awake()
    //  {
    //  }

    // public void OnMove(InputValue input)
    // {
    //     value = input.Get<Vector2>();

    //     //moveVec = new Vector3(inputVec.x, 0, inputVec.y);
    // }

     void Update()
     {
        thumbstick.performed += ctx => Debug.Log("Left Value: " + ctx.ReadValue<Vector2>());

        thumbstick.Enable();  
        value = thumbstick.ReadValue<Vector2>().x + thumbstick.ReadValue<Vector2>().y;
        print("value is " + value);
        // probably need a max to be less than
        // avoid dividing by 0
        if(Mathf.Abs(value) > 0.1f)
        {
            sphereCollider.radius += value/100;
            sphereVisual.transform.localScale += new Vector3(value/100, value/100, value/100);
        }
        if(sphereCollider.radius < 0.1f)
        {
            sphereCollider.radius = .1f;
            sphereVisual.transform.localScale = new Vector3(.1f, .1f, .1f);
            Vector3 spherePos = sphereVisual.transform.position;
            // add somehting for moving it closer and further also up there
            //sphereVisual.transform.localPosition = new Vector3(spherePos.x, spherePos.y, .1f);


        }





     }

     



}
