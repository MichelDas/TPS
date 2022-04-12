using System;
using UnityEngine;

public class FreeCameraLook : Pivot {

	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float turnSpeed = 1.5f;
	[SerializeField] private float turnsmoothing = .1f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;
	[SerializeField] private bool lockCursor = false;

	private float lookAngle;
	private float tiltAngle;

	private const float LookDistance = 100f;

	private float smoothX = 0;
	private float smoothY = 0;
	private float smoothXvelocity = 0;
	private float smoothYvelocity = 0;

    public float crosshairOffsetWiggle = 0.2f;
    CrosshairManager crosshairManager;

    //Apply offset to the target of the camera
    public bool overrideTarget;
    public Vector3 newTargetPosition;

    // Add cover limits on the lookAngle
    public float coverAngleMax;
    public float coverAngleMin;
    public bool inCover;
    public int coverDirection;

    //add the singleton
    public static FreeCameraLook instance;
    
    public static FreeCameraLook GetInstance()
    {
        return instance;
    }

	protected override void Awake()
	{
        instance = this;

		base.Awake();

		cam = GetComponentInChildren<Camera>().transform;
		pivot = cam.parent.parent; //take the correct pivot
	}

    protected override void Start()
    {
        base.Start();

        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        crosshairManager = CrosshairManager.GetInstance();

        // init the new target position so it doesn't fly the first time we need it
        newTargetPosition = target.position;
    }
	
    protected override	void Update ()
	{
		base.Update();

		HandleRotationMovement();

	}

	protected override void Follow (float deltaTime)
	{
        // we now want to be able to override the position the camera is following
        Vector3 tp = target.position;

        // update accordingly
        if (overrideTarget)   // this will become true if aiming
        {
            tp = newTargetPosition;
        }
        else
        {
            newTargetPosition = tp;
        }

        Vector3 targetPosition = Vector3.Lerp(transform.position, tp, deltaTime * moveSpeed);

        transform.position = targetPosition;
	}

	void HandleRotationMovement()
	{
        HandleOffsets();

		float x = Input.GetAxis("Mouse X") + offsetX;
		float y = Input.GetAxis("Mouse Y") + offsetY;

        if (turnsmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnsmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnsmoothing);
        }
        else
        {
            smoothX = x;
            smoothY = y;
        }

        if (!inCover) // if we are not in cover
        {
            lookAngle += smoothX * turnSpeed;
        }
        else  // if we are in cover
        {
            // find the angle between where the player model is looking and the world forward
            float angleFromWorldForward = Vector3.Angle(target.forward, Vector3.forward);

            // use that to find our world orientation
            int dot = DotOrientation(angleFromWorldForward);

            // now take our world orientation and find the maximum and minimum angles it can look
            float maxAngle = (angleFromWorldForward * dot) + coverAngleMax;
            float minAngle = (angleFromWorldForward * dot) + coverAngleMin;

            // add to the look angle
            lookAngle += smoothX * turnSpeed;

            // but clamp it to the values we found above i.e. coverMaxAngle and coverMinAngle
            lookAngle = Mathf.Clamp(lookAngle, minAngle, maxAngle);

            // as look angle is relative to the world position, we did this way
            // if we made look angle local rotation this would have been simpler
        }

        // reset the look angle when it does a full circle
        if (lookAngle > 360)
            lookAngle = 0;
        if (lookAngle < -360)
            lookAngle = 0;

		transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

		tiltAngle -= smoothY * turnSpeed;
		tiltAngle = Mathf.Clamp (tiltAngle, -tiltMin, tiltMax);

		pivot.localRotation = Quaternion.Euler(tiltAngle,0,0);

        if (x > crosshairOffsetWiggle || x < -crosshairOffsetWiggle || y > crosshairOffsetWiggle || y < -crosshairOffsetWiggle)
        {
            WiggleCrosshairAndCamera(0);
        }
	}

    private int DotOrientation(float angleFromWorldForward)
    {
        // to find the world orientation

        //we need to know if the north is in front of us or behind us
        float NSdot = Vector3.Dot(target.forward, Vector3.forward);
        // we also need to know if the east is in front of us or behind us
        float WEdot = Vector3.Dot(target.forward, Vector3.right);
        // the above variable returns -1 to 1

        int retVal = 0;

        // first we will check for north
        if(NSdot > 0)    // we are looking to the north
        {
            // if we are looking towards the north
            // then we need to check which is closer, north or East/West
            // basically if it's greater then 45 degree, it's not north anymore
            if(angleFromWorldForward > 45)
            {
                retVal = WestOrEast(WEdot);
            }
            else // if it is under 45 degree, then it's the north
            {
                retVal = -coverDirection;
            }
        }
        else    // same for the south
        {
            // if the angle is greater then 45 degrees from the south, take East West Values
            if (angleFromWorldForward > 45)
            {
                retVal = WestOrEast(WEdot);
            }
            else // if it is under 45 degree, then it's the north
            {
                retVal = -coverDirection;
            }
        }

        return retVal;
    }

    private int WestOrEast(float WEdot)
    {
        // we know it's not north or south
        int retVal = 0;

        // then the dot value will determine if it's west or east
        if(WEdot < 0)
        {
            // depending on what cover position we are in (what aiming animation is going to play )
            // we want to switch the mulptiplier to a negative or a positive value

            if (coverDirection > 0)
                retVal = -coverDirection;
            else
                retVal = coverDirection;
        }
        else
        {
            if (coverDirection < 0)
                retVal = -coverDirection;
            else
                retVal = coverDirection;
        }
        return retVal;
    }

    float offsetX;
    float offsetY;


    void HandleOffsets()
    {
        if (offsetX != 0)
        {
            offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
        }

        if (offsetY != 0)
        {
            offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
        }
    }

    public void WiggleCrosshairAndCamera(float kickback)
    { 
        crosshairManager.activeCrosshair.WiggleCrosshair();

        offsetY = kickback;
    }


}
