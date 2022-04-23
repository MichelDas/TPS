using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPS_Redux
{
    public class PlayerMovement : MonoBehaviour
    {
        InputHandler inputHandler;
        StatesManager statesManager;
        Rigidbody rb;

        Vector3 lookPosition;
        Vector3 storeDirection;

        public float runSpeed = 3;
        public float walkSpeed = 1.5f;
        public float aimSpeed = 1;
        public float speedMultiplier = 10;
        public float rotateSpeed = 2;
        public float turnSpeed = 5;

        public float coverAcceleration = 0.5f;
        public float coverMaxSpeed = 2;

        float horizontal;
        float vertical;

        Vector3 lookDirection;

        private PhysicMaterial zFriction;
        private PhysicMaterial maxFriction;
        Collider col;

        // variables needed for cover
        // A list of covers which we will ignore, like the cover we just left
        List<CoverPosition> ignoreCovers = new List<CoverPosition>();


        // This class should not have a start method
        // TODO replace this start with some Init method  which should be called from statesManager
        void Start()
        {
            inputHandler = GetComponent<InputHandler>();
            rb = GetComponent<Rigidbody>();
            statesManager = GetComponent<StatesManager>();
            col = GetComponent<Collider>();

            zFriction = new PhysicMaterial();
            zFriction.dynamicFriction = 0;
            zFriction.staticFriction = 0;

            maxFriction = new PhysicMaterial();
            maxFriction.dynamicFriction = 100;
            maxFriction.staticFriction = 100;
        }

        // This class should not have a FixedUpdate method
        // TODO replace this FixedUpdate with some FixedTick method which should be called from statesManager
        private void FixedUpdate()
        {
            lookPosition = statesManager.lookPosition;
            lookDirection = lookPosition - transform.position;

            // Handle Movement
            horizontal = statesManager.horizontal;
            vertical = statesManager.vertical;

            if (!statesManager.inCover)
            {
                // Do Normal movement
                HandleMovement();

                // search for cover when we are moving
                if (horizontal != 0 || vertical != 0)
                    SearchForCover();

            }
            else // if we are in cover
            {
                // if not aiming
                if (!statesManager.isAiming)
                {
                    HandleCoverMovement();   // handle the camera movement
                }
                    GetOutOfCover();  // when we want to leave the cover
            }
        }



        private void HandleCoverMovement()
        {
            // if we are in cover, we will move with a lerp not with physics
            

            // we decided were to look (and play the correct animation) based on our inputs
            if(horizontal != 0)
            {
                if(horizontal < 0)
                {
                    statesManager.coverDirection = -1;   // we are looking left
                }
                else
                {
                    statesManager.coverDirection = 1;  // we are looking right
                }
            }


            // Hondle Cover Rotation

            // Our rotation is the same as the first cover position, for a linear movement cover, this works perfectly
            // Later we will do it in a different way
            #region obsolete
            //Quaternion targetRotation = Quaternion.LookRotation(statesManager.coverPosition.pos1.forward);
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            #endregion


            // First we calculate how much we move in percentage,
            // Then add that to path percentage
            // then lerp the character to that percentage in the curve


            // store the full length of the movement we can do on the cover
            float lineLength = statesManager.coverPosition.length;

            // find the movement speed + acceleration
            float movement = ((horizontal * coverAcceleration)
                * coverMaxSpeed) * Time.deltaTime;

            // turn the movement into percentage
            float lerpMovement = movement / lineLength;

            // add that to our cover moving percentage
            statesManager.coverPercentage -= lerpMovement;

            // Clamp the cover percentage into 0-1 values
            statesManager.coverPercentage = Mathf.Clamp01(statesManager.coverPercentage);

            Vector3 curvePathPosition = statesManager.coverPosition.curvePath.GetPointAt(statesManager.coverPercentage);

            curvePathPosition.y = transform.position.y;

            HandleCoverRotation();

            #region obsolete
            // and start moving on the lerp
            //Vector3 targetPosition = Vector3.Lerp(statesManager.coverPosition.pos1.position,
            //                                     statesManager.coverPosition.pos2.position,
            //                                     statesManager.coverPercentage);

            #endregion

            // apply that to out transform
            transform.position = curvePathPosition;
        }

        private void HandleCoverRotation()
        {
            // find a percentae ahead of us
            float forwardPerc = statesManager.coverPercentage + 0.1f;

            // make sure it's always in the path
            if(forwardPerc > 0.99f)
            {
                forwardPerc = 1;
            }

            // find the position you are now
            Vector3 positionNow = statesManager.coverPosition.curvePath.GetPointAt(statesManager.coverPercentage);
            // and find the position forward of us, that is 0.1 ahead
            Vector3 positionForward = statesManager.coverPosition.curvePath.GetPointAt(forwardPerc);

            // the direction we want to look is the cross product of the two points
            Vector3 direction = Vector3.Cross(positionNow, positionForward);
            // remove the y other wise the character might start flying
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
        }

        private void GetOutOfCover()
        {
            // for now, if we want to get out of cover, we just hit the back button
            if (vertical < 0.5f)
            {
                if (!statesManager.isAiming) // we can't leave cover if we are aiming
                {
                    // reset variables in stateManager
                    statesManager.coverPosition = null;
                    statesManager.inCover = false;

                    //Start a coroutine to clear the list of covers to ignore
                    StartCoroutine("ClearIgnoreList");

                }
            }
        }

        private void SearchForCover()
        {
            // look forward for colliders that can be covers
            Vector3 origin = transform.position + Vector3.up / 2;
            Vector3 direction = transform.forward;
            RaycastHit hit;

            // change the distance of raycast accordingly
            if(Physics.Raycast(origin, direction, out hit, 2))
            {
                // if we find cover poisitions
                if (hit.transform.GetComponentInParent<CoverPosition>())
                {
                    // if ignore cover doesn't have have the cover position
                    if (!ignoreCovers.Contains(hit.transform.GetComponentInParent<CoverPosition>()))
                    {
                        //get in cover
                        statesManager.GetInCover(hit.transform.GetComponentInParent<CoverPosition>());
                        // add that cover to ignore list
                        ignoreCovers.Add(hit.transform.GetComponentInParent<CoverPosition>());
                    }
                }
            }
        }

        IEnumerator ClearIgnoreList()
        {
            yield return new WaitForSeconds(1);
            ignoreCovers.Clear();
        }

        private void HandleMovement()
        {
            bool onGround = statesManager.isOnGround;

            if (Mathf.Abs(horizontal) > 0.3f || Mathf.Abs(vertical) > 0.3f || !onGround)
            {
                col.material = zFriction;
            }
            else
            {
                col.material = maxFriction;
            }


            Vector3 ver = inputHandler.camTrans.forward * vertical;
            Vector3 hor = inputHandler.camTrans.right * horizontal;

            ver.y = 0;
            hor.y = 0;


            HandleMovement(hor, ver, onGround);
            HandleRotation(hor, ver, onGround);

            if (onGround)
            {
                rb.drag = 4;
            }
            else
            {
                rb.drag = 0;
            }

        }

        private void HandleMovement(Vector3 hor, Vector3 ver, bool onGround)
        {
            if (onGround)
            {
                rb.AddForce((ver + hor).normalized * Speed());
            }
        }

        private void HandleRotation(Vector3 hor, Vector3 ver, bool onGround)
        {
            if (statesManager.isAiming && !statesManager.inCover) // basically, if we are in cover, we do not rotate
            {
                // if the player is aiming, the character will always look at where the camera is looking
                lookDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                rb.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            }
            else
            {
                if (statesManager.inCover)
                    return;

                storeDirection = transform.position + hor + ver;

                Vector3 dir = storeDirection - transform.position;
                dir.y = 0;

                if(horizontal != 0 || vertical != 0)
                {
                    float angle = Vector3.Angle(transform.forward, dir);

                    if(angle != 0)
                    {
                        float newAngle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));
                        if(newAngle != 0)
                        {
                            rb.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir),turnSpeed*Time.deltaTime);
                        }
                    }
                }
            }
        }

        private float Speed()
        {
            float retVal = 0;

            if(statesManager.isAiming)
            {
                retVal = aimSpeed;
            }
            else
            {
                if (statesManager.isWalking || statesManager.isReloading)
                {
                    retVal = walkSpeed;
                }
                else
                {
                    retVal = runSpeed;
                }
            }

            retVal *= speedMultiplier;

            return retVal;
        }
    }
}
