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

        private void UpdateStatesManager()
        {
            statesManager.isAiming = statesManager.isOnGround && (mouse2 > 0);
            statesManager.canRun = !statesManager.isAiming;
            statesManager.isWalking = (fire3 > 0); // if he is holding left shift

            statesManager.horizontal = horizontal;
            statesManager.vertical = vertical;

            if (statesManager.isAiming)
            {
                targetZ = cameraAimingZ;
                targetFov = aimingFov;

                if(mouse1 > 0.5f && !statesManager.isReloading)
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
            if(statesManager.shoot && statesManager.handleShooting.currentBullets > 0)
            {
                targetShake = shakeRecoil;
                camProperties.WiggleCrosshairAndCamera(0.05f);
                targetFov += 5;
            }
            else
            {
                if(statesManager.vertical != 0)
                {
                    targetShake = shakeMovement;
                }
                else
                {
                    if(statesManager.horizontal != 0)
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
        }

    }
}

