using UnityEngine;
using System.Collections;

public class menuManager : MonoBehaviour
{
    public GameObject createNewGameMenu;
    public GameObject createNewGameMenu2;

	void OnJoinedLobby() {
     	this.createNewGameMenu2.active = true;
     	this.createNewGameMenu.active = true;

    }
    
   	void OnJoinedRoom() {
     	this.createNewGameMenu2.active = false;
     	this.createNewGameMenu.active = false;

   	}
}






