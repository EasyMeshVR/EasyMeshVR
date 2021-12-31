using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMeshVR.Core
{
    [RequireComponent(typeof(Animator))]
    public class Hand : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private float speed;

        [SerializeField]
        private string animatorGripParam = "Grip";

        [SerializeField]
        private string animatorTriggerParam = "Trigger";

        private Animator animator;
        private float gripTarget;
        private float triggerTarget;
        private float gripCurrent;
        private float triggerCurrent;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            AnimateHand();
        }

        #endregion

        #region Private Methods

        internal void SetGrip(float v)
        {
            gripTarget = v;
        }

        internal void SetTrigger(float v)
        {
            triggerTarget = v;
        }

        void AnimateHand()
        {
            if (gripCurrent != gripTarget)
            {
                gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * speed);
                animator.SetFloat(animatorGripParam, gripCurrent);
            }

            if (triggerCurrent != triggerTarget)
            {
                triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, Time.deltaTime * speed);
                animator.SetFloat(animatorTriggerParam, triggerCurrent);
            }
        }

        #endregion
    }
}
