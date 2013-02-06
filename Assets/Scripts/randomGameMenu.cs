using UnityEngine;
using System.Collections;

public class RandomGameMenu : MonoBehaviour {


	// this script is used to create a simple Lobby menu with a ramndom matchmaking,
	 void Start() {
        
        PhotonNetwork.ConnectUsingSettings("0.1");
    	renderer.material.color = Color.white;
    }


    void OnMouseEnter () {
    	renderer.material.color = Color.red;
	}
	
	void OnMouseExit () {
		
    	renderer.material.color = Color.white;
		
	}
	
	
	void OnMouseDown () {
		
		PhotonNetwork.JoinRandomRoom();
	}
	
	

}
