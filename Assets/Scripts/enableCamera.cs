using UnityEngine;
using System.Collections;

public class enableCamera : MonoBehaviour {
    private PhotonView myPhotonView;
	// Use this for initialization
	void Start () {
        myPhotonView = gameObject.GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(myPhotonView.isMine){	
        //	SmoothFollow cameraScript = Camera.main.GetComponent<SmoothFollow>();
        //	cameraScript.target = transform;
		}	
	}
}
