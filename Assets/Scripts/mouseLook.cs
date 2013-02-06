using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class mouseLook : MonoBehaviour {

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

	public bool canUpdateTheRotation;
    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
	private PhotonView myPhotonView;
    
    Quaternion originalRotation;
	
	void Awake () {
	}


   void Start ()
    {
        myPhotonView = gameObject.GetComponent<PhotonView>();
     	Screen.lockCursor = true;
        if (rigidbody)
            rigidbody.freezeRotation = true;
        originalRotation = transform.localRotation;
    }
    
    
    void Update ()
    {
    	
    	
    	if (Input.GetKey (KeyCode.Escape)) {
    		
			Screen.lockCursor = false;
    	}

    	
    	if (Input.GetMouseButton(0) ){
    		
    			
    		Screen.lockCursor = true;
    	}
    	
        if (axes == RotationAxes.MouseX && Input.mousePosition.x >= 0 &&  Input.mousePosition.x <= Screen.width &&  Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height && Screen.lockCursor && myPhotonView.isMine )
        {	
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationX = ClampAngle (rotationX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        
        
        else {
        	
        	//Screen.lockCursor = false;
        	
        }
    }
 

    public static float ClampAngle (float angle, float min, float max)
    {
    	
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }
    
}
