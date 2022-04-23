using UnityEngine;
using System.Collections;
using System;

namespace TPS_Redux
{
    public class HandleShooting : MonoBehaviour
    {
        StatesManager statesManager;
        public Animator weaponAnim;
        public float fireRate;
        float timer;
        public Transform bulletSpawnPoint;
        public GameObject smokeParticle;
        //public ParticleSystem[] muzzle;
        public GameObject muzzle;
        public GameObject casingPrefab;
        public Transform caseSpawnPosition;

        public int currentBullets = 30;

        private void Start()
        {
            statesManager = GetComponent<StatesManager>();
        }

        bool isShooting;
        bool dontShoot;

        bool emptyGun;

        private void Update()
        {
            isShooting = statesManager.shoot;

            if (isShooting)
            {
                if(timer <= 0)
                {
                    //weaponAnim.SetBool("Shoot", false);

                    if (currentBullets > 0)
                    {
                        emptyGun = false;
                        //statesManager.audioManager.PlayGunSound();

                        if (casingPrefab)
                        {
                            GameObject go = Instantiate(casingPrefab, caseSpawnPosition.position, caseSpawnPosition.rotation);
                            Rigidbody rig = go.GetComponent<Rigidbody>();
                            rig.AddForce(transform.right.normalized * 2 + Vector3.up * 1.3f, ForceMode.Impulse);
                            rig.AddRelativeTorque(go.transform.right * 1.5f, ForceMode.Impulse);
                        }


                        //for (int i = 0; i < muzzle.Length; i++)
                        //{
                        //    ShootMuzzle(muzzle[i].gameObject);
                        //}
                        if (muzzle)
                        {
                            ShootMuzzle();
                        }

                        RaycastShoot();

                        currentBullets--;
                    }
                    else
                    {
                        if (emptyGun)
                        {
                            statesManager.handleAnimations.StartReload();
                            currentBullets = 30;
                        }
                        else
                        {
                        //    statesManager.audioManager.PlayEffect("empty_gun");
                            emptyGun = true;
                        }
                    }

                    timer = fireRate;
                }
                else
                {
                    //weaponAnim.SetBool("Shoot", true);
                    timer -= Time.deltaTime;
                }
            }
            else
            {
                timer -= 1;
                weaponAnim.SetBool("Shoot", false);
            }
        }

        private void RaycastShoot()
        {

            // This is the function that will interact with other scripts
            // I will work on this a lot

            Vector3 direction = statesManager.lookHitPosition - bulletSpawnPoint.position;
            RaycastHit hit;

            if(Physics.Raycast(bulletSpawnPoint.position, direction, out hit, 100, statesManager.layerMask))
            {
                if (smokeParticle)
                {
                    GameObject go = Instantiate(smokeParticle, hit.point, Quaternion.identity) as GameObject;
                    go.transform.LookAt(bulletSpawnPoint.position);
                }
                if (hit.transform.GetComponent<AICharacter>())
                {
                    hit.transform.GetComponent<AICharacter>().HitCharacter();
                }
                if (hit.transform.GetComponent<ShootingRangeTarget>())
                {
                    hit.transform.GetComponent<ShootingRangeTarget>().HitTarget();
                }
            }

        }
        GameObject muz;
        private void ShootMuzzle()
        {
            //muz =
            //muz.SetActive(true);

            GameObject go = Instantiate(muzzle, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            go.transform.SetParent(bulletSpawnPoint);
        }
        private void DeactiveMuzzle()
        {
            muz.SetActive(false);
        }
    }
}