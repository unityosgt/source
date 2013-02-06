using UnityEngine;
using System.Collections;



//this script is used to create a simple Lobby menu and crate new room 

public class newGameMenu : MonoBehaviour {
	
	// Use this for initialization
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
		
		PhotonNetwork.CreateRoom( "server" , true, true, 2);
	
	}
		
	
}
