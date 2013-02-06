using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {

	void OnControllerColliderHit(ControllerColliderHit hit) 
	{
		// if this player is not "it", the player can't tag anyone, so don't do anything on collision
		if (PhotonNetwork.player.ID != GameLogic.playerWhoIsIt)
		{
			return;
		}
		
		// this collision happened for the "it" player, so check who's next (except Plane)
		if (!hit.gameObject.name.Equals("Plane"))
		{
			PhotonView rootView = hit.gameObject.transform.root.GetComponent<PhotonView>();
			GameLogic.TagPlayer(rootView.owner.ID);
		}
	}
	
	
	void OnJoinedRoom()
	{
		// this script is not existing when we enter the room, so it's not called
		// instead, this is attached to a character which we network instantiate on joining a room
		
		// so: use Awake for initial setup
	}
}
