
using UnityEngine;
using System.Collections;

public class health : MonoBehaviour {
	
	public GameObject[] spawnPoints;
	
	public GameObject waypointContainer;
	
	public PhotonView myPhotonView;
	public int life; 
	public bool death;
	public GameObject KIlledBY;
	public Transform body;
	
	public bool isDeath;
	public int selectedSpawn;
	
	public int killPoint; 
	public int deathPoint;
	new public Renderer renderer;
	
	public bool damage;

	
	void Start() {
        myPhotonView = gameObject.GetComponent<PhotonView>();
		if (myPhotonView.isMine){
			
			myPhotonView.RPC("setLife", PhotonTargets.All );
		}
	}
	
	void Update () {
		


		if (spawnPoints.Length == 0 && gameObject.GetComponent<move>().playerTeam == 1){ // if no spawnPoints, find them
        	
        	spawnPoints = GameObject.FindGameObjectsWithTag("team1_spawn");
    	}
    	
    	if (spawnPoints.Length == 0 && gameObject.GetComponent<move>().playerTeam == 2){ // if no spawnPoints, find them
        	
        	spawnPoints = GameObject.FindGameObjectsWithTag("team2_spawn");
    	}

		
		if (life <0 ) {
			
			life = 0;
		}
		
		if (life == 0){
			life = 0;
			death = true;
		}
		
		renderer = body.GetComponent<SkinnedMeshRenderer>();

        if (death && myPhotonView.isMine)
        {

            selectedSpawn = (Random.Range(0, spawnPoints.Length));
            myPhotonView.RPC("respawnFunc", PhotonTargets.All, selectedSpawn);
            death = false;

            if (KIlledBY != null)
            {
                if (KIlledBY.tag == "Other")
                {
                    PhotonPlayer target = KIlledBY.GetComponent<PhotonView>().owner;
                }
            }
        }
	}
	
	
	[RPC]
	IEnumerator respawnFunc (int selectedSpawn) {
		
		isDeath = true;
		life = 100;
		yield return new WaitForSeconds(1);
		renderer.enabled = false;
		transform.position = spawnPoints[selectedSpawn].transform.position;
		yield return new WaitForSeconds(0.8f);
		renderer.enabled = true;
		isDeath = false;

	}

	
	[RPC]
	void hitFunc (PhotonMessageInfo info) {	
		
		if (!isDeath ){ 
			life -= 12;
		}
		 KIlledBY = GameObject.Find(""+info.sender);
	}
	
    [RPC]
	void hitFunc2 (PhotonMessageInfo info) {	
		
		if (!isDeath ){ 
			life -= 24;
		}
		 KIlledBY = GameObject.Find(""+info.sender);
	}
	
	[RPC]
	void setLife () {	
		
		life = 100;
		
	}
	
	
}
