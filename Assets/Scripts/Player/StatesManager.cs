using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPS_Redux
{
    public class StatesManager : MonoBehaviour
    {

        public bool isAiming;
        public bool canRun;
        public bool isWalking;
        public bool shoot;
        public bool isReloading;
        public bool isOnGround;
        public bool isCrouching;
        public float stance;

        public float coverPercentage;
        public CoverPosition coverPosition;
        public bool inCover;
        public int coverDirection;
        public bool canAim;
        public bool crouchCover;
        public bool aimAtSides;
        public bool canAnimate;

        public float horizontal;
        public float vertical;
        public Vector3 lookPosition;
        public Vector3 lookHitPosition;
        public LayerMask layerMask;

        public CharacterAudioManager audioManager;

        [HideInInspector]
        public HandleShooting handleShooting;
        [HideInInspector]
        public HandleAnimations handleAnimations;

        private void Start()
        {
            audioManager = GetComponent<CharacterAudioManager>();
            handleShooting = GetComponent<HandleShooting>();
            handleAnimations = GetComponent<HandleAnimations>();
        }

        private void FixedUpdate()
        {
            isOnGround = IsOnGround();

            isWalking = inCover;

            HandleStance();
        }

        float targetStance;
        internal bool dontRun;

        private void HandleStance()
        {
            if (!isCrouching)
                targetStance = 1; // play standing animation
            else
                targetStance = 0; // play crouching animation

            stance = Mathf.Lerp(stance, targetStance, Time.deltaTime * 3f);
        }

        public void GetInCover(CoverPosition cover)
        {
            // find distance to the first position
            float distFromPos1 = Vector3.Distance(transform.position, cover.curvePath.GetPointAt(0));
            // subdivide with full length to return the percentage
            coverPercentage = distFromPos1 / cover.length;


            #region obsolete
            //// find the direction toward th 
            #endregion

            // find the point in the curve with the percentage we found above
            Vector3 targetPos = cover.curvePath.GetPointAt(coverPercentage);

            // start lerping so we can hug the wall
            StartCoroutine(LerpToCoverPositionPercentage(targetPos));

            coverPosition = cover;
            inCover = true;
        }

        private IEnumerator LerpToCoverPositionPercentage(Vector3 targetPos)
        {
            Vector3 startingPos = transform.position;
            Vector3 targPos = targetPos;

            targPos.y = transform.position.y;

            float t = 0;
            while(t < 1)
            {
                t += Time.deltaTime * 5;
                transform.position = Vector3.Lerp(startingPos, targPos, t);
                yield return null;
            }
        }

        bool IsOnGround()
        {
            Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
            float dis = 1.4f;
            RaycastHit hit;

            if (Physics.Raycast(origin, -Vector3.up, out hit, dis, layerMask))
            {
                return true;
            }
            return false;
        }
   
    }

}
