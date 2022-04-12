using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPS_Redux
{
    public class WeaponManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        // TODO part 7 Cover adds new lines at 49 minute

        // TODO part 7 Cover adds new lines at 54 minute


    }

    [System.Serializable]
    public class WeaponReferenceBase
    {
        public string weaponID;
        public GameObject weaponModel;
        public Animator modelAnimator;
        public GameObject ikHolder;
        public Transform rightHandTarget;
        public Transform leftHandTarget;
        public Transform lookTarget;
        public ParticleSystem[] muzzle;
        public GameObject[] muzzleObj;
        public Transform bulletSpawner;
        public Transform casingSpawner;
        public WeaponStats weaponStats;
        public Transform rightElbowTarget;
        public Transform leftElbowTarget;

        public bool dis_LHIK_notAiming;
    }

    [System.Serializable]
    public class WeaponStats
    {
        public int curBullets;
        public int maxBullets;
        public float fireRate;
        public AudioClip shootSound;
    }

}
