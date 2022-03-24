using System;
using UnityEngine;

namespace TPS_Redux
{
    public class ShootingRangeTarget : MonoBehaviour
    {
        public Animator anim;
        public void HitTarget()
        {
            anim.SetBool("Down", true);
        }
    }
}