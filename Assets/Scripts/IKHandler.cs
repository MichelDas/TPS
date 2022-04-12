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

        [SerializeField] private bool enableTwoHandWield;
        [SerializeField] private Vector3 secondHandLookPosition;
        [SerializeField] private Transform secondaryWeaponHolder;
        [SerializeField] private Transform leftShoulder;

        [SerializeField] private Transform overRideLookTarget;

        [SerializeField] private Transform rightHandIKTarget;
        [SerializeField] private Transform rightElbowTarget;
        [SerializeField] private float rightHandIKWeight;
        float targetRHWeight;

        [SerializeField] private Transform leftHandIKTarget;
        [SerializeField] private Transform leftElbowTarget;
        [SerializeField] private float leftHandIKWeight;
        float targetLHWeight;

        Transform aimHelperRightShoulder;
        Transform aimHelperLeftShoulder;

        Transform aimHelper;
        bool LHIK_dis_notAiming;


        void Start()
        {
            aimHelper = new GameObject().transform;
            animator = GetComponent<Animator>();
            statesManager = GetComponent<StatesManager>();
        }

        private void FixedUpdate()
        {

            HandleShoulders();
            AimWeight();
            HandleRightHandIKWeight();
            HandleLeftHandIKWeight();
            //leftHandIKWeight  = 1 - animator.GetFloat("LeftHandIKWeightOverride");

            HandleShoulderRotation();
        }

        private void HandleShoulders()
        {
            // start function a jehetu Animator assign hoi ( HandleAnimations )
            // right shoulder null howar o possibility theke jai, tokhon porjonto assign na howa
            if (rightShoulder == null)
            {
                // if I dont already have a right shoulder object to follow,
                // just follow the right shoulder of the animator
                rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            }
            else
            {
                weaponHolder.position = rightShoulder.position;
            }
        }

        private void AimWeight()
        {
            if (statesManager.isAiming && !statesManager.isReloading)
            {
                Vector3 directionTowardsTarget = aimHelper.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

                // jei dike aim korte chai, shei dike takanor por e character aim korbe
                // tahole aim koro
                if (angle < 90)
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
        }

        private void HandleRightHandIKWeight()
        {
            float multiplier = 3;

            if (statesManager.inCover)  // if we are in cover
            {
                targetRHWeight = 0;  // we don't want ik in the right hand

                if (statesManager.isAiming) // if we are aiming
                {
                    targetRHWeight = 1;
                    multiplier = 2;
                }
                else
                {
                    multiplier = 10;
                }

            }
            else   // if we are not in cover
            {
                // TODO check this later again
                rightHandIKWeight = lookWeight;
                //targetWeight = lookWeight;
            }

            if(statesManager.isReloading) // if we are reloading
            {
                // no ik will be there
                targetRHWeight = 0;
                multiplier = 5;
            }

            // lerp to desired values
            rightHandIKWeight = Mathf.Lerp(rightHandIKWeight, targetRHWeight, Time.deltaTime * multiplier);
        }

        private void HandleLeftHandIKWeight()
        {
            // same as right hand with some extra checks
            float multiplier = 3;

            if (statesManager.inCover)  // if we are in cover
            {
                if (!LHIK_dis_notAiming)
                {
                    targetLHWeight = 1;
                    multiplier = 6;
                }
                else
                {
                    multiplier = 10;

                    if (statesManager.isAiming) // if we are aiming
                    {
                        targetLHWeight = 1;
                    }
                    else
                    {
                        targetLHWeight = 0;
                        leftHandIKWeight = 0;
                    }
                }
            }
            else   // if we are not in cover
            {
                multiplier = 10;
                if (!LHIK_dis_notAiming)
                {
                    targetWeight = 1;
                }
                else
                {
                    targetWeight = (statesManager.isAiming) ? 1 : 0;
                }
            }

            if (statesManager.isReloading) // if we are reloading
            {
                // no ik will be there
                targetLHWeight = 0;
                multiplier = 10;
            }

            // lerp to desired values
            leftHandIKWeight = Mathf.Lerp(leftHandIKWeight, targetLHWeight, Time.deltaTime * multiplier);

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
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }

            if (rightHandIKTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }

            if (rightElbowTarget)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightHandIKWeight);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightHandIKTarget.position);
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
            }

            if (leftElbowTarget)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftHandIKWeight);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowTarget.position);
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            }
        }
    }
}