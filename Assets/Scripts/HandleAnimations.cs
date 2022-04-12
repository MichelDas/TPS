using UnityEngine;
using System;

namespace TPS_Redux
{
    public class HandleAnimations : MonoBehaviour
    {
        Animator anim;

        StatesManager statesManager;
        Vector3 lookDirection;

        private void Start()
        {
            statesManager = GetComponent<StatesManager>();
            SetupAnimator();
        }

        private void FixedUpdate()
        {
            statesManager.isReloading = anim.GetBool("Reloading");
            anim.SetBool("Aim", statesManager.isAiming);

            if (!statesManager.canRun)
            {
                anim.SetFloat("Forward", statesManager.vertical, 0.1f, Time.deltaTime);
                anim.SetFloat("Sideways", statesManager.horizontal, 0.1f, Time.deltaTime);
            }
            else
            {
                float movement = Mathf.Abs(statesManager.vertical) + Mathf.Abs(statesManager.horizontal);
                bool isWalking = statesManager.isWalking;

                movement = Mathf.Clamp(movement, 0, (isWalking || statesManager.isReloading) ? 0.5f : 1);

                anim.SetFloat("Forward", movement, 0.1f, Time.deltaTime);

            }

            anim.SetBool("Cover", statesManager.inCover);
            anim.SetInteger("CoverDirection", statesManager.coverDirection);
        }

        void SetupAnimator()
        {
            anim = GetComponent<Animator>();

            Animator[] animators = GetComponentsInChildren<Animator>();

            for(int i=0; i<animators.Length; i++)
            {
                if(animators[i] != anim)
                {
                    anim.avatar = animators[i].avatar;
                    Destroy(animators[i]);
                    break;
                }
            }
        }

        public void StartReload()
        {
            if (!statesManager.isReloading)
            {
                Debug.Log("start reloading");
                anim.SetTrigger("Reload");
            }
        }
    }
}