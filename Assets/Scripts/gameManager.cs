using UnityEngine;
using System.Collections;

public class gameManager : Photon.MonoBehaviour {

    public PhotonView myPhotonView;

	void Start () {
        myPhotonView = gameObject.GetComponent<PhotonView>();
	}


}
