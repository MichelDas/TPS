using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPS_Redux
{
    public class IKHandler : MonoBehaviour
    {
        Animator animator;
        StatesManager statesManager;

        [SerializeField] private float lookWeight = 1;
        [SerializeField] private float bodyWeight = 0.8f;
        [SerializeField] private float headWeight = 1;
        [SerializeField] private float clampWeight = 1;

        float targetWeight;

        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Transform rightShoulder;

        [SerializeField] private Transform overRideLookTarget;

        [SerializeField] private Transform rightHandIKTarget;
        [SerializeField] private float rightHandIKWeight;

        [SerializeField] private Transform leftHandIKTarget;
        [SerializeField] private float leftHandIKWeight;

        Transform aimHelper;


        void Start()
        {
            aimHelper = new GameObject().transform;
            animator = GetComponent<Animator>();
            statesManager = GetComponent<StatesManager>();
        }

        private void FixedUpdate()
        {
            // start function a jehetu Animator assign hoi ( HandleAnimations )
            // right shoulder null howar o possibility theke jai, tokhon porjonto assign na howa
            if(rightShoulder == null)
            {
                // if I dont already have a right shoulder object to follow,
                // just follow the right shoulder of the animator
                rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            }
            else
            {
                weaponHolder.position = rightShoulder.position;
            }

            if(statesManager.isAiming && !statesManager.isReloading)
            {
                Vector3 directionTowardsTarget = aimHelper.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

                // jei dike aim korte chai, shei dike takanor por e character aim korbe
                // tahole aim koro
                if(angle < 90)
                {
                    targetWeight = 1;
                }
                else
                {
                    // noile wait koro
                    targetWeight = 0;
                }
            }
            else
            {
                targetWeight = 0;
            }

            float multiplier = (statesManager.isAiming) ? 5 : 30;

            lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * multiplier);

            rightHandIKWeight = lookWeight;
            leftHandIKWeight = lookWeight;
            //leftHandIKWeight  = 1 - animator.GetFloat("LeftHandIKWeightOverride");

            HandleShoulderRotation();
        }

        private void HandleShoulderRotation()
        {
            // aim helper camera jei dik a takai ase oi dike takate help korbe,
            // but ekta delay hobe. ete kore dekhte shundor lagbe
            aimHelper.position = Vector3.Lerp(aimHelper.position, statesManager.lookHitPosition, Time.deltaTime * 5);
            weaponHolder.LookAt(aimHelper.position);
            rightHandIKTarget.parent.transform.LookAt(aimHelper.position);
        }

        private void OnAnimatorIK()
        {
            animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, headWeight, clampWeight);

            Vector3 filterDirection = statesManager.lookPosition;

            animator.SetLookAtPosition(
                (overRideLookTarget != null) ?
                overRideLookTarget.position : filterDirection);

            if (leftHandIKTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            }

            if (rightHandIKTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            }
        }
    }
}