// --------------------------------------------------------------------------------------------------------------------
// *Enable Photon Cloud Networking by uncommenting where noted below
// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{    
	private string roomName = "Game Room";
	public  GUISkin mySkin;
	public GameObject[] spawnPoints;
	private Vector2 scrollPos = Vector2.zero;
	private bool selectTeam;
	private bool selectSkin;
	private bool mainMenu;
	public Texture2D menutex1;
	public GameObject team1;
	public GameObject team2;
	public  GUIStyle myStyle;
	public GameObject NewPrefab;
	private int skinC;
	private int team;
	
	private void Start ()
	{
		selectTeam = true;
    }
	
    private void Awake ()
	{
		// Connect to the main photon server. This is the only IP and port we ever need to set(!)
		if (!PhotonNetwork.connected) {
			PhotonNetwork.ConnectUsingSettings ("1.0");
		}

		// PhotonNetwork.logLevel = NetworkLogLevel.Full; // turn on if needed

		// generate a name for this player, if none is assigned yet
		if (string.IsNullOrEmpty (PhotonNetwork.playerName)) {
			PhotonNetwork.playerName = "Guest" + Random.Range (1, 9999);
		}
	}

	private void OnGUI ()
	{
    	
		GUI.skin = mySkin;

		if (mainMenu) {
			GUILayout.BeginArea (new Rect ((Screen.width - 400) / 2, (Screen.height - 250) / 2, 450, 440));
			GUILayout.Label (string.Format ("Players: {0}  Games: {1}", PhotonNetwork.countOfPlayers, PhotonNetwork.countOfRooms));

			// Player name
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Player Name:", GUILayout.Width (150));
			PhotonNetwork.playerName = GUILayout.TextField (PhotonNetwork.playerName, 14);
			if (GUI.changed) {
				// Save name
				PlayerPrefs.SetString ("playerName", PhotonNetwork.playerName);
			}

			GUILayout.EndHorizontal ();

			GUILayout.Space (15);


			// Create a room (fails if exist!)
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("CREATE GAME:", GUILayout.Width (150));
			this.roomName = GUILayout.TextField (this.roomName, 14);
			if (GUILayout.Button ("GO")) {


				PhotonNetwork.CreateRoom (this.roomName, true, true, 10);

			}

			GUILayout.EndHorizontal ();

			// Join random room
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("JOIN RANDOM GAME:", GUILayout.Width (150));
			if (PhotonNetwork.GetRoomList ().Length == 0) {
				GUILayout.Label ("NO GAMES AVAILABLE, CREATE A NEW GAME!");
			} else {
				if (GUILayout.Button ("GO")) {
					PhotonNetwork.JoinRandomRoom ();
				}
			}

			GUILayout.EndHorizontal ();

			GUILayout.Space (30);
			GUILayout.Label ("GAME LISTING:");
			if (PhotonNetwork.GetRoomList ().Length == 0) {
				GUILayout.Label ("---------------------------");
			} else {
				// Room listing: simply call GetRoomList: no need to fetch/poll whatever!
				this.scrollPos = GUILayout.BeginScrollView (this.scrollPos);
				foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList()) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label (roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
					if (GUILayout.Button ("JOIN")) {
						PhotonNetwork.JoinRoom (roomInfo.name);
					}

					GUILayout.EndHorizontal ();
				}

				GUILayout.EndScrollView ();
			}

			GUILayout.EndArea ();
		}
    
		if (selectTeam) {
		    

			Rect texRect = new Rect (Screen.width / 2 - 222, Screen.height / 1 - 350, 400, 66);
   			
			if (texRect.Contains (Input.mousePosition) && Input.GetMouseButtonUp (0)) {
				
				StartCoroutine (wait01Sec ());
				team = 1;

			}
   			
   			
			GUI.DrawTexture (new Rect (Screen.width / 2 - 222, Screen.height / 1 - 350, 400, 66), menutex1);
			
            

		}
		
		if (selectSkin && !selectTeam && !mainMenu) {
			GUI.Label (new Rect (Screen.width / 2 - (100), 100, 380, 60), "Select Your Player", myStyle);
             
        		
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100)) {
        			
				if (hit.collider.gameObject.name == "Player_S1" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 1;
					mainMenu = true;
            			
				}
        			
				if (hit.collider.gameObject.name == "Player_S2" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 2;
					mainMenu = true;
				}


				if (hit.collider.gameObject.name == "Player_S3" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 3;
					mainMenu = true;
				}


				if (hit.collider.gameObject.name == "Player_S4" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 4;
					mainMenu = true;
				}
					
					
				if (hit.collider.gameObject.name == "Player_S5" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 5;
					mainMenu = true;
            			
				}
					
					
				if (hit.collider.gameObject.name == "Player_S6" && Input.GetMouseButtonUp (0)) {
					selectSkin = false;
					skinC = 6;
					mainMenu = true;
            			
				}
					
					

        
			}

			if (team == 1) {
				team1.SetActiveRecursively (true);
			}
        		
        		
        		
			if (team == 2) {
				team2.SetActiveRecursively (true);
			}

		} else {
			team2.SetActiveRecursively (false);
			team1.SetActiveRecursively (false);
		}
    	
	} 

    
	public void OnCreatedRoom ()
	{
    	
    	
	}
   
	private void OnJoinedRoom ()
	{
    	
   		
		int selectedTeam1_Spawn = (Random.Range (0, spawnPoints.Length));
		int selectedTeam2_Spawn = (Random.Range (0, spawnPoints.Length));

        
		if (skinC == 1 && team == 1) {
        	
			NewPrefab = PhotonNetwork.Instantiate ("Player_C1", spawnPoints [selectedTeam1_Spawn].transform.position, Quaternion.identity, 0);
		}
        
		if (skinC == 2 && team == 1) {
        	
			NewPrefab = PhotonNetwork.Instantiate ("Player_C2", spawnPoints [selectedTeam1_Spawn].transform.position, Quaternion.identity, 0);
		}
        
		if (skinC == 3 && team == 1) {
		 
			NewPrefab = PhotonNetwork.Instantiate ("Player_C3", spawnPoints [selectedTeam1_Spawn].transform.position, Quaternion.identity, 0);
        	
		}
        
		if (skinC == 4 && team == 2) {
        	
			NewPrefab = PhotonNetwork.Instantiate ("Player_C4", spawnPoints [selectedTeam2_Spawn].transform.position, Quaternion.identity, 0);
		}
		
		if (skinC == 5 && team == 2) {
		
			NewPrefab = PhotonNetwork.Instantiate ("Player_C5", spawnPoints [selectedTeam2_Spawn].transform.position, Quaternion.identity, 0);
        	
		}
        
		if (skinC == 6 && team == 2) {
        	
			NewPrefab = PhotonNetwork.Instantiate ("Player_C6", spawnPoints [selectedTeam2_Spawn].transform.position, Quaternion.identity, 0);
		}
	}

	void Update ()
	{
		if (spawnPoints.Length == 0 && team == 1) { // if no spawnPoints, find them
        	
			spawnPoints = GameObject.FindGameObjectsWithTag ("team1_spawn");
		}
    	
		if (spawnPoints.Length == 0 && team == 2) { // if no spawnPoints, find them
        	
			spawnPoints = GameObject.FindGameObjectsWithTag ("team2_spawn");
		}
	}
    
	IEnumerator wait01Sec ()
	{
    	
		yield return new WaitForSeconds(0.1f);
		selectSkin = true;
		selectTeam = false;
	}
}