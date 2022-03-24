using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDisabler : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("destroying muzzle");
        Destroy(this.gameObject, .5f);
    }
}
