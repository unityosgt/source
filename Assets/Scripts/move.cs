using UnityEngine;
using System.Collections;

public class move : MonoBehaviour
{
	// other scripts
	private ChatVik CHATScript;  //|inov| reference to the chatscript for posting messages
	
	public AnimationClip idleAnimation;//all the animation
	public AnimationClip runAnimation;
	public AnimationClip jumpAnimation;
	public AnimationClip punch1Animation;
	public AnimationClip punch2Animation;
	public AnimationClip punch3Animation;
	public AnimationClip hit1Animation;
	public AnimationClip deathAnimation;
	public bool canMove;
	public bool idleA;//all the animation
	public bool runA;
	public bool jumpA;
	public bool punch1A;
	public bool punch2A;
	public bool punch3A;
	public bool deathA;
	public bool punch;
	public bool hit;
	public int playerTeam;
	public int skinColor;
	public int point;
	public int randomPunchaAnimation;
	public float jumpSpeed = 2F;
	public float gravity = 90.0F;
	private Vector3 moveDirection = Vector3.zero;
	private GameObject attacker;
	public Transform torso;
	int i = 0;
	public int team1Players;
	public int team2Players;
	
	//|inov| public variable that holds this players name for quick access
	public string myName = "";
	public PhotonView myPhotonView;
	public float speed = 3.0f;
	public bool isNormalControl = true;
	float strafeValue = 0.0f;
	float rotationValue = 0.0f;
	

	void Start()
	{

		myPhotonView = gameObject.GetComponent<PhotonView> ();
		
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Other");


		animation [punch1Animation.name].AddMixingTransform (torso);
		animation [punch2Animation.name].AddMixingTransform (torso);
		animation [punch3Animation.name].AddMixingTransform (torso);
		animation [hit1Animation.name].AddMixingTransform (torso);

		animation [hit1Animation.name].layer = 90;
		animation [punch1Animation.name].layer = 55;
		animation [punch2Animation.name].layer = 55;
		animation [punch3Animation.name].layer = 55;
		animation [jumpAnimation.name].layer = 40;

		animation [runAnimation.name].wrapMode = WrapMode.Loop;// set the animation to loop ..
		animation [deathAnimation.name].wrapMode = WrapMode.ClampForever;// set the animation to loop ..
		
		//|inov| get chat script and store it for future use
		CHATScript = GameObject.Find ("GameManager").GetComponent<ChatVik> ();
		
		if (myPhotonView.isMine) {
			//|inov| sets local variable myName to this users name for future use
			myName = PhotonNetwork.playerName;
			//|inov| sends my local name to all other players
			myPhotonView.RPC ("setNameGlobal", PhotonTargets.All, myName);
		}

	}

	void Update ()
	{
	    
		CharacterController controller = GetComponent<CharacterController> ();
		health healthScript = gameObject.GetComponent<health> ();//read and write variable from this script

		
		if (Input.GetMouseButtonDown (2)) {
			isNormalControl = !isNormalControl;
		}
		 
		if (isNormalControl) {
			strafeValue = Input.GetAxis ("HorRotate");
			rotationValue = Input.GetAxis ("Horizontal");
		} else {
			strafeValue = Input.GetAxis ("Horizontal");
			rotationValue = Input.GetAxis ("HorRotate");
		}
		

		if (Input.GetMouseButtonUp (0) && myPhotonView.isMine && !hit && !punch && !healthScript.isDeath && !isDelayingAttack) {
		    
			StartCoroutine (DelayAttack ());
			
			
			if (!animation.IsPlaying (punch1Animation.name) && !animation.IsPlaying (punch2Animation.name) && !animation.IsPlaying (punch3Animation.name)) {

				if (randomPunchaAnimation == 1) {
					punch1A = true;
				}

				if (randomPunchaAnimation == 2) {

					punch2A = true;
				}

				if (randomPunchaAnimation == 3) {

					punch3A = true;

				}

				punch = true;

				if (Input.GetAxis ("Vertical") == 0 && rotationValue == 0) {
					if (randomPunchaAnimation != 3) {

						randomPunchaAnimation++;
					} else {

						randomPunchaAnimation = 1;
					}
				} else {

					if (randomPunchaAnimation == 2) {

						randomPunchaAnimation++;
					} else {

						randomPunchaAnimation = 2;
					}
				}
				
				
				//EffectsManager.Instance.Play (Effects.Attack);  // <- uncomment to Enable Particle Effect for Player Attack

			}
			
			
			StartCoroutine (DelayWalk ());
			
		} else {
			punch = false;
		}


	
		if ((Input.GetAxis ("Vertical") >= 0.1f || rotationValue >= 0.1f || Input.GetAxis ("Vertical") <= -0.1f || rotationValue <= -0.1f) && !healthScript.isDeath && myPhotonView.isMine && !isDelayingWalk) {
			runA = true;
			idleA = false;
			deathA = false;
			SoundManager.Instance.Play (SoundEffect.Walk, true);


		} else if (!healthScript.isDeath && myPhotonView.isMine) {
			runA = false;
			idleA = true;
			deathA = false;
			SoundManager.Instance.Stop (SoundEffect.Walk);
		}

		if (!healthScript.isDeath) {

			if (myPhotonView.isMine) {


				if (controller.isGrounded && !isDelayingWalk) {
					moveDirection = new Vector3 (rotationValue, 0, Input.GetAxis ("Vertical"));
					moveDirection = transform.TransformDirection (moveDirection);
					moveDirection *= speed;
					
					if (Input.GetButton ("Jump")) {

						jumpA = true;

						if (jumpA) {

							moveDirection.y = jumpSpeed;
						}
					} else {

						jumpA = false;
					}

				}

				moveDirection.y -= gravity * Time.deltaTime;
				controller.Move (moveDirection * Time.deltaTime);
				
				if (isNormalControl) {		//if using normal control								//take input from HorRotate Axis
					if (Input.GetButton ("HorRotate")) {
						transform.Rotate (0, strafeValue * 300 * Time.deltaTime, 0);
					}
				} else {
					if (Input.GetButton ("Horizontal")) {									//if using inverted controls																		//take input from Horzontal Axis(Strafe values already 
						transform.Rotate (0, strafeValue * 300 * Time.deltaTime, 0);			//contains values from Horizontal Axis, where inverting 
					}																		//controls in initial Update code)	
				}

			}
		}


		//*************************************************************	**************************************************
		//**************************************************ANIMATION**************************************************
		if (idleA) {
			animation.CrossFade (idleAnimation.name);

		}
		if (runA && !isDelayingWalk) {

			animation [runAnimation.name].speed = Mathf.Sign (Input.GetAxis ("Vertical"));
			animation.CrossFade (runAnimation.name);//and play it.

		}

		if (jumpA) {
			animation [jumpAnimation.name].speed = 1.5f;
			animation.CrossFade (jumpAnimation.name);//and play it.
			StartCoroutine (stop ());
		}

		if (punch1A) {
			animation [punch1Animation.name].speed = 2.0f;
			animation.Play (punch1Animation.name);
			StartCoroutine (stop ());
		}


		if (punch2A) {
			animation [punch2Animation.name].speed = 2.0f;
			animation.Play (punch2Animation.name);//and play it.
			StartCoroutine (stop ());

		}

		if (punch3A) {
			animation [punch3Animation.name].speed = 1.8f;
			animation.Play (punch3Animation.name);//and play it.
			StartCoroutine (stop ());
		}

		if (deathA) {
			animation.CrossFade (deathAnimation.name);

		}

		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		RaycastHit hitR;
		Debug.DrawRay (transform.position + new Vector3 (0, 0.5f, 0), transform.TransformDirection (Vector3.forward), Color.red);
		if (Physics.Raycast (transform.position + new Vector3 (0, 0.5f, 0), transform.TransformDirection (Vector3.forward), out hitR, 2) ||
            Physics.Raycast (transform.position + new Vector3 (0, 0.5f, 0), transform.TransformDirection (Vector3.back), out hitR, 2) ||
            Physics.Raycast (transform.position + new Vector3 (0, 0.5f, 0), transform.TransformDirection (Vector3.left), out hitR, 2) ||
            Physics.Raycast (transform.position + new Vector3 (0, 0.5f, 0), transform.TransformDirection (Vector3.right), out hitR, 2)) {
			if (hitR.collider.gameObject.GetComponent<move> ()) {
				
				if (hitR.collider.gameObject.tag == "Other" &&
                punch &&
                hitR.collider.gameObject.GetComponent<health> ().isDeath != true && 
                hitR.collider.gameObject.GetComponent<move> ().playerTeam != this.playerTeam &&
                hitR.collider.gameObject != gameObject) {
			
					hitR.collider.gameObject.GetComponent<health> ().myPhotonView.RPC ("hitFunc", PhotonTargets.All);
					hitR.collider.gameObject.GetComponent<move> ().myPhotonView.RPC ("hitAnimation", PhotonTargets.All);

				}
			
			} 
			

			
			
			if (hitR.collider.gameObject.GetComponent<move> ()) {
			
				if (hitR.collider.gameObject.tag == "Other") {
			  
					attacker = hitR.collider.gameObject;
				  
					if (Input.GetMouseButtonUp (0) && !PickUpItems.Instance.SuperPower) {
				     
						hitR.collider.gameObject.GetComponent<health> ().myPhotonView.RPC ("hitFunc", PhotonTargets.All);
						hitR.collider.gameObject.GetComponent<move> ().myPhotonView.RPC ("hitAnimation", PhotonTargets.All);
					 
					} else if (Input.GetMouseButtonUp (0) && PickUpItems.Instance.SuperPower) {
				     
						EffectsManager.Instance.Play (Effects.AdditionalAttack);
						hitR.collider.gameObject.GetComponent<health> ().myPhotonView.RPC ("hitFunc2", PhotonTargets.All);
						hitR.collider.gameObject.GetComponent<move> ().myPhotonView.RPC ("hitAnimation", PhotonTargets.All);
						PickUpItems.Instance.SuperPower = false;
					 
					}
				
				}
			
			}   
			
			
		}

		
		if (hit) {
			StartCoroutine (hitF ());

		}

		if (healthScript.isDeath) {
			deathA = true;

		}




	}

	IEnumerator hitF ()
	{

		yield return new WaitForSeconds(0.1f);
		animation.Play (hit1Animation.name);
		StartCoroutine (stop ());

	}

	IEnumerator stop ()
	{

		yield return new WaitForSeconds(0.1f);
		punch1A = false;
		punch2A = false;
		punch3A = false;
		jumpA = false;
		hit = false;
	}
	
	bool isDelayingWalk = false;
	public float walkDelayTime = 2.0f;

	IEnumerator DelayWalk ()
	{
		if (!isDelayingWalk) {		
			isDelayingWalk = true;
			moveDirection = Vector3.zero;
			yield return new WaitForSeconds(walkDelayTime);
			isDelayingWalk = false;
		}
		yield break;
	}
	
	bool isDelayingAttack = false;
	public float attackDelayTime = 1.0f;

	IEnumerator DelayAttack ()
	{
		if (!isDelayingAttack) {
			isDelayingAttack = true;
			yield return new WaitForSeconds(attackDelayTime);
			isDelayingAttack = false;
		}
		yield break;
	}

	[RPC]
	void hitAnimation ()
	{

		hit = true;
	}

	[RPC]
	void RedPlayer ()
	{

		playerTeam = 1;
	}

	[RPC]
	void BluePlayer ()
	{

		playerTeam = 2;
	}

	[RPC]
	void namePlayer ()
	{

		//ddd	
	}

	[RPC]
	void setNameGlobal (string playerName)
	{
		//|inov| displays the newly connected players name in the chat box
		CHATScript.AddMessage (playerName + " joined the game");
		//|inov| other players also store the received name for future use
		myName = playerName;
	}

	[RPC]
	void addP ()
	{
		if (myPhotonView.isMine) {
			point++;
		}
	}
	
	
}


