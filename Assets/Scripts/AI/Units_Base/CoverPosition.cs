using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPosition : MonoBehaviour {

	public Transform pos1;
	public bool blockPos1;
	public Transform pos2;
	public bool blockPos2;

	public float length;
	public CoverType coverType;

	public enum CoverType
    {
		full,
		half
    }

    private void Start()
    {
		length = Vector3.Distance(pos1.position, pos2.position);
    }
}

