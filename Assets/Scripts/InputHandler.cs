 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPS_Redux
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private float horizontal;
        [SerializeField] private float vertical;
        [SerializeField] private float mouse1;
        [SerializeField] private float mouse2;
        [SerializeField] private float fire3;
        [SerializeField] private float middleMouse;
        [SerializeField] private float mouseX;
        [SerializeField] private float mouseY;

        [HideInInspector]
        public FreeCameraLook camProperties;
        [HideInInspector]
        public Transform camPivot;
        [HideInInspector]
        public Transform camTrans;

        CrosshairManager crosshairManager;
        ShakeCamera shakeCam;
        StatesManager statesManager;

        [SerializeField] private float normalFov = 60;
        [SerializeField] private float aimingFov = 40;
        float targetFov;
        float currentFov;
        [SerializeField] private float cameraNormalZ = -2;
        [SerializeField] private float cameraAimingZ = -0.86f;
        float targetZ;
        float actualZ;
        float curZ;
        LayerMask layerMask;

        [SerializeField] private float shakeRecoil = 0.5f;
        [SerializeField] private float shakeMovement = 0.3f;
        [SerializeField] private float shakeMin = 0.1f;
        float targetShake;
        float curShake;

        public bool leftPivot;

        public CameraValues cameraValues;

        [System.Serializable]
        public class CameraValues
        {
            public float coverCameraOffsetX = 0.2f;
            public float coverCameraOffsetZ = -0.2f;
            public float coverCameraOffsetY = 0;
            public Vector3 targetCameraOffset;
            public Vector3 startingPivotPos;

            public float coverLeftMaxAngle = 30;
            public float coverLeftMinAngle = -30;
            public float coverRightMaxAngle = 30;
            public float coverRightMinAngle = -30;
        }

        private bool canSwitch;
        private bool fpsMode;

        //ControllerSwitcher conSwitcher;

        private void Start()
        {
            crosshairManager = CrosshairManager.GetInstance();
            camProperties = FreeCameraLook.GetInstance();
            camPivot = camProperties.transform.GetChild(0);
            camTrans = camPivot.transform.GetChild(0);
            shakeCam = camPivot.GetComponentInChildren<ShakeCamera>();

            statesManager = GetComponent<StatesManager>();
            Cursor.lockState = CursorLockMode.Locked;

            layerMask = ~(1 << gameObject.layer);
            statesManager.layerMask = layerMask;

            //conSwitcher = ControllerSwitcher.Instance;

            //if (conSwitcher != null)
            //    canSwitch = true;

            // init the camera pivot starting position
            cameraValues.startingPivotPos = camPivot.localPosition;
        }

        private void FixedUpdate()
        {
            HandleInput();
            UpdateStatesManager();
            HandleShake();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            }

            // find where the camera is looking

            Ray ray = new Ray(camTrans.position, camTrans.forward);
            statesManager.lookPosition = ray.GetPoint(20);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction);

            if (Physics.Raycast(ray.origin, ray.direction, out hit, 500, layerMask))
            {
                Debug.Log("hitting " + hit.transform.gameObject.name);
                if(hit.transform.tag == "AICharacter")
                {
                    foreach(Crosshair cr in crosshairManager.crosshairs)
                    {
                        cr.PointedToEnemy = true;
                    }
                }
                else
                {
                    foreach (Crosshair cr in crosshairManager.crosshairs)
                    {
                        cr.PointedToEnemy = false;
                    }
                }
                    
                statesManager.lookHitPosition = hit.point;

            }
            else
            {
                foreach (Crosshair cr in crosshairManager.crosshairs)
                {
                    cr.PointedToEnemy = false;
                }
                statesManager.lookHitPosition = statesManager.lookPosition;
            }

            // check for obstacles in front of the camera
            CameraCollision(layerMask);

            //Update camera's position
            curZ = Mathf.Lerp(curZ, actualZ, Time.deltaTime * 15);
            camTrans.localPosition = new Vector3(0, 0, curZ);

            // camera's on cover aiming position & camera's pivot position
            cameraValues.targetCameraOffset = Vector3.zero;

            // change the x value of the pivot according to if he is looking left or right
            float pivotX = (!leftPivot) ? cameraValues.startingPivotPos.x : -cameraValues.startingPivotPos.x;

            // if we are in cover
            if (statesManager.inCover)
            {
                // after line 154 we are changing here the position of the camera again by
                // multiplying it with stateManger.coverDirection
                pivotX = cameraValues.startingPivotPos.x * statesManager.coverDirection;

                // update the camera values
                camProperties.inCover = statesManager.isAiming;
                camProperties.coverDirection = statesManager.coverDirection;
                camProperties.overrideTarget = statesManager.isAiming;

                if (statesManager.isAiming)
                {
                    // when aimin we want out camera to start peaking towards that position

                    // first find the offset in the local position of the transform
                    Vector3 localPos = new Vector3(cameraValues.coverCameraOffsetX * statesManager.coverDirection, 0,
                                                    cameraValues.coverCameraOffsetZ);

                    // then turn the local position to world position
                    Vector3 worldPos = transform.TransformPoint(localPos);
                    // we want the camera to be on the same Y as the player
                    worldPos.y = transform.position.y;
                    // the new target position for the camera is the world position
                    cameraValues.targetCameraOffset = worldPos;

                    // update the angles where the camera can look into according to the cover position
                    camProperties.coverAngleMin = (statesManager.coverDirection < 0) ? cameraValues.coverLeftMinAngle : cameraValues.coverRightMinAngle;
                    camProperties.coverAngleMax = (statesManager.coverDirection < 0) ? cameraValues.coverLeftMaxAngle : cameraValues.coverRightMaxAngle;
                }

                // change the pivot local position accordingly
                Vector3 targetPivotPos = new Vector3(pivotX, cameraValues.startingPivotPos.y, cameraValues.startingPivotPos.z);
                camPivot.localPosition = Vector3.Lerp(camPivot.localPosition, targetPivotPos, Time.deltaTime * 3);


                // lerp the new target position
                camProperties.newTargetPosition = Vector3.Lerp(camProperties.newTargetPosition,
                                                                cameraValues.targetCameraOffset, Time.deltaTime * 3);
            }

        }
        
        private void HandleInput()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            mouse1 = Input.GetAxis("Fire1");
            mouse2 = Input.GetAxis("Fire2");
            middleMouse = Input.GetAxis("Mouse ScrollWheel");
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            fire3 = Input.GetAxis("Fire3");

            //// from the tps to fps video
            //if (canSwitch)
            //{
            //    if (Input.GetKeyUp(KeyCode.F)){
            //        Ray ray = new Ray(camTrans.position, camTrans.forward);
            //        Vector3 lookPos = ray.GetPoint(20);

            //        //if (!fpsMode)
            //        //    conSwitcher.SwitchToFps(lookPos);
            //        //else
            //        //    ControlSwitcher.SwitchToTps(lookPos);
            //    }
            //}

            if (statesManager.inCover) // if we are in cover we want to override controls
            {
                if (statesManager.isAiming) // if we are aiming we cannot move
                {
                    horizontal = 0;
                }
                else
                {
                    // if we are at one edge of the cover
                    if(statesManager.coverPercentage > 0.99f)
                    {
                        if(!statesManager.coverPosition.blockPos2)   // and it's a viable aiming position
                        {
                            // then we can aim
                            statesManager.canAim = true;
                        }

                        // clamp the movement to only move one way
                        horizontal = Mathf.Clamp(horizontal, 0, 1);
                    }
                    // same as above, but for the other side
                    else if (statesManager.coverPercentage < 0.1f)
                    {
                        if (!statesManager.coverPosition.blockPos1)
                        {
                            statesManager.canAim = true;
                        }
                        // clamp the movement to only move one way
                        horizontal = Mathf.Clamp(horizontal, -1, 0);
                    }
                    else
                    {
                        statesManager.canAim = false;
                    }
                }

            }
        }


        private void UpdateStatesManager()
        {
            statesManager.canRun = !statesManager.isAiming;

            if (!statesManager.inCover)  // we can walk if we are not in cover
                statesManager.isWalking = (fire3 > 0);  // holding left shift

            statesManager.horizontal = horizontal;
            statesManager.vertical = vertical;

            // if we are in cover
            if (statesManager.inCover)
            {
                // if out input let us aim, we will aim
                //if(mouse2 > 0 && statesManager.canAim)
                if (statesManager.canAim && (mouse2 > 0))
                {
                    statesManager.isAiming = true;
                }
                else
                {
                    statesManager.isAiming = false;
                }
            }
            else
            {
                // if we are not in cover, same as before
                statesManager.isAiming = statesManager.isOnGround && (mouse2 > 0);
            }

            if (statesManager.isAiming)
            {
                targetZ = cameraAimingZ;   // update target Z position of the camera
                targetFov = aimingFov;

                if (mouse1 > 0.5f && !statesManager.isReloading) // we will shoot
                {
                    statesManager.shoot = true;
                }
                else
                {
                    statesManager.shoot = false;
                }
            }
            else
            {
                statesManager.shoot = false;
                targetZ = cameraNormalZ;  // update Z of the camera
                targetFov = normalFov;
            }
        }


        private void HandleShake()
        {
            if (statesManager.shoot && statesManager.handleShooting.currentBullets > 0)
            {
                targetShake = shakeRecoil;
                camProperties.WiggleCrosshairAndCamera(0.05f);
                targetFov += 5;
            }
            else
            {
                if (statesManager.vertical != 0)
                {
                    targetShake = shakeMovement;
                }
                else
                {
                    if (statesManager.horizontal != 0)
                    {
                        targetShake = shakeMovement;
                    }
                    else
                    {
                        targetShake = shakeMin;
                    }
                }
            }

            curShake = Mathf.Lerp(curShake, targetShake, Time.deltaTime * 10);
            shakeCam.positionShakeSpeed = curShake;

            currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * 5);
            Camera.main.fieldOfView = currentFov;
        }


        private void CameraCollision(LayerMask layerMask)
        {
            // Do a raycast from the pivot of the camera to the camera
            Vector3 origin = camPivot.TransformPoint(Vector3.zero);
            Vector3 direction = camTrans.TransformPoint(Vector3.zero) - camPivot.TransformPoint(Vector3.zero);
            RaycastHit hit;

            // the distance of the raycast is controlled by if we are aiming or not
            actualZ = targetZ;

            // if an ostacle is found
            if (Physics.Raycast(origin, direction, out hit, Mathf.Abs(targetZ), layerMask))
            {
                // if we hit something, find the distance of the object
                float dist = Vector3.Distance(camPivot.position, hit.point);
                actualZ = -dist; // and the opposite is where we want to place out camera

            }
        }

    }
}

