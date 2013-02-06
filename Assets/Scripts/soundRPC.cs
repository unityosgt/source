using UnityEngine;
using System.Collections;

public class soundRPC : MonoBehaviour {

    public AudioClip StepSound;//all the sound effect!.. drag it in the inspector...
    public AudioClip FireSound;
    public AudioClip ReloadingSound;
    
	void Start () {
	
	}
	
	/*[RPC]
	void audioStep () {
		
		
		audio.clip = StepSound;
		audio.Play();
		audio.volume = 0.28f;
	}
	
	
	
	
	[RPC]
	void audioFire () {
		
		audio.clip = FireSound;
		audio.Play();
		audio.volume = 0.20f;
	}
	
	
	
	
	
	
	
	[RPC]
	IEnumerator audioReloading () {
		
		yield return new WaitForSeconds(0.3f);//wait 0.3 second and play tha sound.
		audio.clip = ReloadingSound;
		audio.volume = 0.20f;
		audio.Play();
	}*/
	
	
	
}
