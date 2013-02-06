using UnityEngine;
using System.Collections;

public class HeroCamera : MonoBehaviour
{
	
	public LayerMask collisionLayers = -1;
    public float heroHeight = 2.0f;
    public float heroDistance = 5.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;
	public int zoomRate = 200;
	public float zoomDampening = 5.0f;
	public float xSpeed = 200.0f;
	public float ySpeed = 200.0f;
    public float rotationDampening = 3.0f;
    public float offsetFromWall = 0.1f;
	
	
	public enum CameraState
	{
		FirstPerson,
		ThirdPerson,
		Orbit
	}
	public CameraState camState = CameraState.ThirdPerson;
	
	public Transform cam = null;

    private bool camSwitch = false;
	
	Transform hero;
	public Transform headBone = null;
	int minAngleY = -80;
	int maxAngleY = 80;
    float xAngl = 0.0f;
    float yAngl = 0.0f;
    float curDist;
    float desDist;
    float finalDist;
	
	//HeroClimb // hClimb;

	
	string lastState = "ThirdPerson";
	public float fpsCamDist = -0.15f;
	
	
	//=================================================================================================================o
    void Start ()
    {
    	hero = transform;
		// hClimb = GetComponent <HeroClimb> () as HeroClimb;
        cam = Camera.main.transform;
    	Vector3 angls = new Vector3(0, cam.eulerAngles.y, 0);
    	xAngl = angls.x;
    	yAngl = angls.y;

		curDist = heroDistance;
    	desDist = heroDistance;
    	finalDist = heroDistance;
		
		// hClimb.doClimbDel += DoClimb;
		
		Camera.main.nearClipPlane = 0.01f;
		
		
	
		
    	//Screen.lockCursor = true;
    	Screen.showCursor = false;
		
		// if no headbone search for it
		if (headBone == null) {
			Transform[] bones = GetComponentsInChildren <Transform>() as Transform[];
			foreach (Transform t in bones) {
				if (t.name == "head")
					headBone = t;
			}
		}
    }
	//=================================================================================================================o
	
	// Remember the last camera state when climbing
	void DoClimb (string s)
	{
		if (s != "None") // While climbing
		{
			if (camState == CameraState.FirstPerson)
			{
				camState = CameraState.ThirdPerson;
			}
			else if (camState == CameraState.Orbit)
			{
				camState = CameraState.ThirdPerson;
			}
			else if (camState == CameraState.ThirdPerson)
			{
				return;
			}
		}
		else // Leaving climb modus
		{
			if (lastState == "FirstPerson")
			{
				camState = CameraState.FirstPerson;
			}
			else if (lastState == "ThirdPerson")
			{
				camState = CameraState.ThirdPerson;
			}
			else if (lastState == "Orbit")
			{
				camState = CameraState.Orbit;
			}
		}
	}

    void Update()
    {
        if (GameObject.FindWithTag("Player") && !Screen.lockCursor)
            Screen.lockCursor = true;

        if (Input.GetMouseButtonDown(2))
        {
            camSwitch = !camSwitch;

            if (camSwitch)
                camState = CameraState.Orbit;
            else
                camState = CameraState.ThirdPerson;
        }
    }

	//=================================================================================================================o
    void LateUpdate ()
    {
		// 1,2,3 buttons for switching camera modi
		if ( Input.GetKeyDown ("2") ) 
		{
    		// Orbit
			lastState = "Orbit";
			
    		Camera.main.fieldOfView = 70.0f;
    		camState = CameraState.Orbit;
    	}
		else if ( Input.GetKeyDown ("3") )
		{
    		// ThirdPerson
			lastState = "ThirdPerson";
    		
			Camera.main.fieldOfView = 70.0f;
			curDist = heroDistance;
	    	desDist = heroDistance;
	    	finalDist = heroDistance;
    		camState = CameraState.ThirdPerson;
    	}
		
		// Camera states
		switch (camState)
		{
		case CameraState.FirstPerson:
			FirstPerson();
			break;
		case CameraState.ThirdPerson:
			ThirdPerson();
			break;
		case CameraState.Orbit:
			Orbit();
			break;
		}
	}
	//=================================================================================================================o
	void FirstPerson ()
	{
		// Horizontal
		xAngl = hero.eulerAngles.y;
		// Vertical
    	yAngl = ClampAngle (yAngl, minAngleY /1.5f, maxAngleY /1.1f);
		
		// Desired distance
		desDist = fpsCamDist;
		// Camera rotation
    	Quaternion camRot = Quaternion.Euler (yAngl, xAngl, 0);
    	// Camera position
		Vector3 camPos = headBone.position - (cam.forward * desDist) - (cam.up * -heroHeight /4);
		
		// Apply Y-mouse axis
		yAngl -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
		
		// Apply position and rotation
		cam.rotation = camRot;
		cam.position = camPos;
	}
	//=================================================================================================================o
	void ThirdPerson ()
	{
		// Desired distance via mouse wheel
		desDist -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desDist);
		desDist = Mathf.Clamp (desDist, minDistance, maxDistance);
		finalDist = desDist;
		
		// Horizontal smooth rotation
		xAngl = Mathf.LerpAngle (cam.eulerAngles.y, hero.eulerAngles.y, rotationDampening * Time.deltaTime);
		// Vertical angle limitation
    	yAngl = ClampAngle (yAngl, minAngleY, maxAngleY);
    	// Camera rotation
    	Quaternion camRot = Quaternion.Euler (yAngl, xAngl, 0);
    	// Camera height
    	Vector3 headPos = new Vector3 (0, -heroHeight /1.2f, 0);
    	// Camera position
    	Vector3 camPos = hero.position - (camRot * Vector3.forward * desDist + headPos);
		
		// Recalculate hero position
		Vector3 trueHeroPos = new Vector3 (hero.position.x, hero.position.y + heroHeight, hero.position.z);
		
		// Check for collision with Linecast
		RaycastHit hit;
		bool isOk = false;
		if ( Physics.Linecast (trueHeroPos, camPos - Vector3.up + Vector3.forward, out hit, collisionLayers.value)) // slightly behind and below the camera
		{
			// Final distance
			finalDist = Vector3.Distance (trueHeroPos, hit.point) - offsetFromWall;
			isOk = true;
		}
		
		// Lerp current distance if not corrected
		if ( !isOk || ( finalDist > curDist ) )
			curDist = Mathf.Lerp (curDist, finalDist, Time.deltaTime * zoomDampening);
		else
			curDist = finalDist;
		
		// Clamp current distance
		//curDist = Mathf.Clamp (curDist, minDistance, maxDistance);
		
		// Recalculate camera position
		camPos = hero.position - (camRot * Vector3.forward * curDist + headPos);
		
		// Left shift = no y rotation
		if ( !Input.GetKey ( KeyCode.LeftShift ) )
		{
			// Apply Y-mouse axis
			yAngl -= Input.GetAxis ( "Mouse Y" ) * ySpeed * 0.02f;
		}
		
		
		// Apply position and rotation
		cam.rotation = camRot;
		cam.position = camPos;

        if (Input.GetAxis("Mouse X") !=0)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * 300 * Time.deltaTime, 0);
        }
	}
	//=================================================================================================================o
	void Orbit ()
	{
		// Desired distance via mouse wheel
		desDist -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desDist);
		desDist = Mathf.Clamp (desDist, minDistance, maxDistance);
		finalDist = desDist;
		
		// Horizontal smooth rotation
		xAngl += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
		// Vertical angle limitation
    	yAngl = ClampAngle (yAngl, minAngleY, maxAngleY);
		
		// Camera rotation
    	Quaternion camRot = Quaternion.Euler (yAngl, xAngl, 0);
    	// Camera height
    	Vector3 headPos = new Vector3 (0, -heroHeight /1.2f, 0);
    	// Camera position
    	Vector3 camPos = hero.position - (camRot * Vector3.forward * desDist + headPos);
		
		// Recalculate hero position
		Vector3 trueHeroPos = new Vector3 (hero.position.x, hero.position.y + heroHeight, hero.position.z);
		
		// Check if there is something between camera and character
		RaycastHit hit;
		bool isOk = false;
		if ( Physics.Linecast (trueHeroPos, camPos, out hit, collisionLayers.value))
		{
			// Final distance
			finalDist = Vector3.Distance (trueHeroPos, hit.point) - offsetFromWall;
			isOk = true;
		}
		
		// Lerp current distance if not corrected
		if ( !isOk || ( finalDist > curDist ) )
			curDist = Mathf.Lerp (curDist, finalDist, Time.deltaTime * zoomDampening);
		else
			curDist = finalDist;
		
		// Clamp current distance
		//curDist = Mathf.Clamp (curDist, minDistance, maxDistance);
		
		// Recalculate camera position
		camPos = hero.position - (camRot * Vector3.forward * curDist + headPos);
		
		// Left shift = no y rotation
		if ( !Input.GetKey ( KeyCode.LeftShift ) )
		{
			// Apply Y-mouse axis
			yAngl -= Input.GetAxis ( "Mouse Y" ) * ySpeed * 0.02f;
		}

		
		// Apply position and rotation
		cam.rotation = camRot;
		cam.position = camPos;
	}
	//=================================================================================================================o
	
	// Clamp angle at 360deg
	static float ClampAngle ( float angle, float min, float max )
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
	//=================================================================================================================o
}
