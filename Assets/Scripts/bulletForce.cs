using UnityEngine;
using System.Collections;

public class bulletForce : MonoBehaviour {

	public GameObject spawnedFrom;
	
	
	void Start () {
		
	  	StartCoroutine(waitDestroy());
	}
	
	void Update () {
	

       rigidbody.AddForce(transform.forward * 100);// add force 
	}
	
	
	IEnumerator waitDestroy() {
		
		yield return new WaitForSeconds(3);//wait 3 seconds and after that destroy this object.
      	Destroy(this.gameObject);
	}


	void OnTriggerEnter(Collider hit){
		
		if (hit.gameObject.tag == "Player" && hit.collider.gameObject.name != gameObject.name ){
			
       		health spawnedFrom_healthScript = spawnedFrom.GetComponent<health>();// update the "health.cs" from this GameObject.
       		
       		health hit_healthScript = hit.collider.gameObject.GetComponent<health>();// update the "health.cs" from the hit GameObject.
       		
       		hit_healthScript.damage = true;
       		
       		if (hit_healthScript.life == 0) {
       			
       		spawnedFrom_healthScript.myPhotonView.RPC("addPoint", PhotonTargets.All);
       		}
       		
       		if (!hit_healthScript.isDeath) {
       			hit_healthScript.myPhotonView.RPC("hitFunc", PhotonTargets.All);
       		}
       		hit_healthScript.damage = false;
       		
			Destroy(this.gameObject);

		}
   	}
}
