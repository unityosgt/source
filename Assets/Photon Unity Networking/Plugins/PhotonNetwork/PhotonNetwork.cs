// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonNetwork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.IO;

using ExitGames.Client.Photon;
using UnityEngine;

/// <summary>
/// The main class to use the PhotonNetwork plugin.
/// This class is static.
/// </summary>
public static class PhotonNetwork
{

    public const string versionPUN = "1.15";

    /// <summary>
    /// This Monobehaviour allows Photon to run an Update loop.
    /// </summary>
    public static readonly PhotonHandler photonMono;

    /// <summary>
    /// Our photon class
    /// </summary>
    public static readonly NetworkingPeer networkingPeer;

    /// <summary>
    /// The maximum amount of assigned PhotonViews PER player (or scene). See the documentation on how to raise this limitation
    /// </summary>
    public static readonly int MAX_VIEW_IDS = 1000; // VIEW & PLAYER LIMIT CAN BE EASILY CHANGED, SEE DOCS

    public static ServerSettings PhotonServerSettings = (ServerSettings)Resources.Load(Path.GetFileNameWithoutExtension(NetworkingPeer.serverSettingsAssetPath), typeof(ServerSettings));

    /// <summary>
    /// The minimum difference that a Vector2 or Vector3(e.g. a transforms rotation) needs to change before we send it via a PhotonView's OnSerialize/ObservingComponent
    /// Note that this is the sqrMagnitude. E.g. to send only after a 0.01 change on the Y-axix, we use 0.01f*0.01f=0.0001f. As a remedy against float inaccuracy we use 0.000099f instead of 0.0001f.
    /// </summary>
    public static float precisionForVectorSynchronization = 0.000099f; 
    /// <summary>
    /// The minimum angle that a rotation needs to change before we send it via a PhotonView's OnSerialize/ObservingComponent
    /// </summary>
    public static float precisionForQuaternionSynchronization = 1.0f;
    /// <summary>
    /// The minimum difference between floats before we send it via a PhotonView's OnSerialize/ObservingComponent
    /// </summary>
    public static float precisionForFloatSynchronization = 0.01f;     
    

    // "VARIABLES"

    /// <summary>
    /// Are we connected to the photon server (can be IN or OUTSIDE a room)
    /// </summary>
    public static bool connected
    {
        get
        {
            if (offlineMode)
            {
                return true;
            }
            
            return connectionState == ConnectionState.Connected;
        }
    }

    /// <summary>
    /// Simplified connection state
    /// </summary>
    public static ConnectionState connectionState
    {
        get
        {
            if (offlineMode)
            {
                return ConnectionState.Connected;
            }

            if (networkingPeer == null)
            {
                return ConnectionState.Disconnected;
            }

            switch (networkingPeer.PeerState)
            {
                case ExitGames.Client.Photon.PeerStateValue.Disconnected:
                    return ConnectionState.Disconnected;
                case ExitGames.Client.Photon.PeerStateValue.Connecting:
                    return ConnectionState.Connecting;
                case ExitGames.Client.Photon.PeerStateValue.Connected:
                    return ConnectionState.Connected;
                case ExitGames.Client.Photon.PeerStateValue.Disconnecting:
                    return ConnectionState.Disconnecting;
                case ExitGames.Client.Photon.PeerStateValue.InitializingApplication:
                    return ConnectionState.InitializingApplication;
            }

            return ConnectionState.Disconnected;
        }
    }

    /// <summary>
    /// Detailed connection state
    /// </summary>
    public static PeerState connectionStateDetailed
    {
        get
        {
            if (offlineMode)
            {
                return PeerState.Connected;
            }

            if (networkingPeer == null)
            {
                return PeerState.Disconnected;
            }

            return networkingPeer.State;
        }
    }

    /// <summary>
    /// Get the room we're currently in. Null if we aren't in any room.
    /// </summary>
    public static Room room
    {
        get
        {
            if (isOfflineMode)
            {
                if (offlineMode_inRoom)
                {
                    return new Room("OfflineRoom", new Hashtable());
                }
                else
                {
                    return null;
                }
            }

            return networkingPeer.mCurrentGame;
        }
    }

    /// <summary>
    /// Network log level.
    /// </summary>
    public static PhotonLogLevel logLevel = PhotonLogLevel.ErrorsOnly;

    /// <summary>
    /// The local PhotonPlayer. Also available when not connected
    /// </summary>
    public static PhotonPlayer player
    {
        get
        {
            if (networkingPeer == null)
            {
                return null; // Surpress ExitApplication errors
            }

            return networkingPeer.mLocalActor;
        }
    }

    /// <summary>
    /// The PhotonPlayer of the master client. The master client is the 'virtual owner' of the room. You can use it if you need authorative decision made by one of the players.
    /// </summary>
    /// <remarks>
    /// The masterClient is null until a room is joined and becomes null again when the room is left.
    /// </remarks>
    public static PhotonPlayer masterClient
    {
        get
        {
            if (networkingPeer == null)
            {
                return null;
            }

            return networkingPeer.mMasterClient;
        }
    }

    /// <summary>
    /// Our local player name
    /// </summary>
    /// <remarks>Setting the name will automatically send it, if connected. Setting null, won't change the name.</remarks>
    public static string playerName
    {
        get
        {
            return networkingPeer.PlayerName;
        }

        set
        {
            networkingPeer.PlayerName = value;
        }
    }

    /// <summary>
    /// The full PhotonPlayer list, including the local player.
    /// </summary>
    public static PhotonPlayer[] playerList
    {
        get
        {
            if (networkingPeer == null)            
                return new PhotonPlayer[0];

            return networkingPeer.mPlayerListCopy;
        }
    }

    /// <summary>
    /// The other PhotonPlayers, not including our local playe.rov
    /// </summary>
    public static PhotonPlayer[] otherPlayers
    {
        get
        {
            if (networkingPeer == null)            
                return new PhotonPlayer[0];

            return networkingPeer.mOtherPlayerListCopy;
        }
    }

    /// <summary>
    /// Offline mode can be set to re-use your multiplayer code in singleplayer game modes. 
    /// When this is on PhotonNetwork will not create any connections and there is near to 
    /// no overhead. Mostly usefull for reusing RPC's and PhotonNetwork.Instantiate
    /// </summary>
    public static bool offlineMode
    {
        get
        {
            return isOfflineMode;
        }

        set
        {
            if (value == isOfflineMode)
            {
                return;
            }

            if (value && connected)
            {
                Debug.LogError("Can't start OFFLINE mode while connected!");
            }
            else
            {
                networkingPeer.Disconnect(); // Cleanup (also calls OnLeftRoom to reset stuff)
                isOfflineMode = value;
                if (isOfflineMode)
                {
                    NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
                    networkingPeer.ChangeLocalID(1);
                    networkingPeer.mMasterClient = player;
                }
                else
                {
                    networkingPeer.ChangeLocalID(-1);
                    networkingPeer.mMasterClient = null;
                }
            }
        }
    }

    private static bool isOfflineMode = false;

    private static bool offlineMode_inRoom = false;

    /// <summary>
    /// The maximum number of players for a room. Better: Set it in CreateRoom.
    /// If no room is opened, this will return 0.
    /// </summary>
    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public static int maxConnections
    {
        get
        {
            if (room == null)
            {
                return 0;
            }

            return (int)room.maxPlayers;
        }

        set
        {
            room.maxPlayers = value;
        }
    }

    /// <summary>
    /// This setting defines if players in a room should destroy a leaving player's instantiated GameObjects and PhotonViews.
    /// 
    /// When "this client" creates a room/game, autoCleanUpPlayerObjects is copied to that room's properties and used by all 
    /// PUN clients in that room (no matter what their autoCleanUpPlayerObjects value is).
    /// 
    /// If room.AutoCleanUp is enabled in a room, the PUN clients will destroy a player's objects on leave.
    /// </summary>
    /// <remarks>
    /// When enabled, the server will clean RPCs, instantiated GameObjects and PhotonViews of the leaving player and joining 
    /// players won't get those at anymore.
    ///
    /// Once a room is created, this setting can't be changed anymore.
    /// 
    /// Enabled by default.
    /// </remarks>
    public static bool autoCleanUpPlayerObjects
    {
        get
        {
            return m_autoCleanUpPlayerObjects;
        }
        set
        {
            if (room != null)
                Debug.LogError("Setting autoCleanUpPlayerObjects while in a room is not supported.");
            m_autoCleanUpPlayerObjects = value;
        }
    }

    private static bool m_autoCleanUpPlayerObjects = true;

    /// <summary>
    /// Defines if the PhotonNetwork should join the "lobby" when connected to the Master server.
    /// If this is false, OnConnectedToMaster() will be called when connection to the Master is available.
    /// OnJoinedLobby() will NOT be called if this is false.
    /// 
    /// Enabled by default.
    /// </summary>
    /// <remarks>
    /// The room listing will not become available.
    /// Rooms can be created and joined (randomly) without joining the lobby (and getting sent the room list).
    /// </remarks>
    public static bool autoJoinLobby
    {
        get
        {
            return autoJoinLobbyField;
        }
        set
        {
            autoJoinLobbyField = value;
        }
    }
    /// <summary>
    /// Backing field.
    /// </summary>
    private static bool autoJoinLobbyField = true;


    
    /// <summary>
    /// Returns true when we are connected to Photon and in the lobby state
    /// </summary>
    public static bool insideLobby
    {
        get
        {
            return networkingPeer.insideLobby;
        }
    }


    /// <summary>
    /// Defines how many times per second PhotonNetwork should send a package. If you change
    /// this, do not forget to also change 'sendRateOnSerialize'.
    /// </summary>
    /// <remarks>
    /// Less packages are less overhead but more delay.
    /// Setting the sendRate to 50 will create up to 50 packages per second (which is a lot!).
    /// Keep your target platform in mind: mobile networks are slower and less reliable.
    /// </remarks>
    public static int sendRate
    {
        get
        {
            return 1000 / sendInterval;
        }

        set
        {
            sendInterval = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateInterval = sendInterval;
            }

            if (value < sendRateOnSerialize)
            {
                // sendRateOnSerialize needs to be <= sendRate
                sendRateOnSerialize = value;
            }
        }
    }

    /// <summary>
    /// Defines how many times per second OnPhotonSerialize should be called on PhotonViews.
    /// </summary>
    /// <remarks>
    /// Choose this value in relation to 'sendRate'. OnPhotonSerialize will creart the commands to be put into packages.
    /// A lower rate takes up less performance but will cause more lag.
    /// </remarks>
    public static int sendRateOnSerialize
    {
        get
        {
            return 1000 / sendIntervalOnSerialize;
        }

        set
        {
            if (value > sendRate)
            {
                Debug.LogError("Error, can not set the OnSerialize SendRate more often then the overall SendRate");
                value = sendRate;
            }

            sendIntervalOnSerialize = 1000 / value;
            if (photonMono != null)
            {
                photonMono.updateIntervalOnSerialize = sendIntervalOnSerialize;
            }
        }
    }

    private static int sendInterval = 50;

    private static int sendIntervalOnSerialize = 100;

    /// <summary>
    /// Can be used to pause dispatch of incoming evtents (RPCs, Instantiates and anything else incoming).
    /// This can be useful if you first want to load a level, then go on receiving data of PhotonViews and RPCs.
    /// The client will go on receiving and sending acknowledgements for incoming packages and your RPCs/Events.
    /// This adds "lag" and can cause issues when the pause is longer, as all incoming messages are just queued.
    /// </summary>
    public static bool isMessageQueueRunning
    {
        get
        {
            return m_isMessageQueueRunning;
        }

        set
        {
            if (value == m_isMessageQueueRunning) return;
            PhotonNetwork.networkingPeer.IsSendingOnlyAcks = !value;
            m_isMessageQueueRunning = value;
            if (!value)
            {
                PhotonHandler.StartThread(); // Background loading thread: keeps connection alive
            }
        }
    }

    private static bool m_isMessageQueueRunning = true;

    /// <summary>
    /// Used once per dispatch to limit unreliable commands per channel (so after a pause, many channels can still cause a lot of unreliable commands)
    /// </summary>
    public static int unreliableCommandsLimit
    {
        get
        {
            return networkingPeer.LimitOfUnreliableCommands;
        }

        set
        {
            networkingPeer.LimitOfUnreliableCommands = value;
        }
    }

    /// <summary>
    /// Photon network time, synched with the server
    /// </summary>
    /// <remarks>
    /// v1.3:
    /// This time reflects milliseconds since start of the server, cut down to 4 bytes.
    /// It will overflow every 49 days from a high value to 0. We do not (yet) compensate this overflow.
    /// Master- and Game-Server will have different time values.
    /// v1.10:
    /// Fixed issues with precision for high server-time values. This should update with 15ms precision by default.
    /// </remarks>
    public static double time
    {
        get
        {
            if (offlineMode)
            {
                return Time.time;
            }
            else
            {
                //return (uint)networkingPeer.ServerTimeInMilliSeconds / 1000.0f;
                return ((double)(uint)networkingPeer.ServerTimeInMilliSeconds) / 1000.0f;
            }
        }
    }

    /// <summary>
    /// Are we the master client?
    /// </summary>
    public static bool isMasterClient
    {
        get
        {
            if (offlineMode)
            {
                return true;
            }
            else
            {
                return networkingPeer.mMasterClient == networkingPeer.mLocalActor;
            }
        }
    }

    /// <summary>
    /// True if we are in a room (client) and NOT the room's masterclient
    /// </summary>
    public static bool isNonMasterClientInRoom
    {
        get
        {
            return !isMasterClient && room != null;
        }
    }

    /// <summary>
    /// The count of players currently looking for a room.
    /// This is updated on the MasterServer (only) in 5sec intervals (if any count changed).
    /// </summary>
    public static int countOfPlayersOnMaster
    {
        get
        {
            return networkingPeer.mMasterCount;
        }
    }

    /// <summary>
    /// The count of players currently inside a room
    /// This is updated on the MasterServer (only) in 5sec intervals (if any count changed).
    /// </summary>
    public static int countOfPlayersInRooms
    {
        get
        {
            return countOfPlayers;
        }
    }

    /// <summary>
    /// The count of players currently using this application.
    /// This is updated on the MasterServer (only) in 5sec intervals (if any count changed).
    /// </summary>
    public static int countOfPlayers
    {
        get
        {
            return networkingPeer.mPeerCount + networkingPeer.mMasterCount;
        }
    }

    /// <summary>
    /// The count of rooms currently in use.
    /// When inside the lobby this is based on PhotonNetwork.GetRoomList().Length.
    /// When not inside the lobby, this value updated on the MasterServer (only) in 5sec intervals (if any count changed).
    /// </summary>
    public static int countOfRooms
    {
        get
        {
            if (insideLobby)
            {
                return GetRoomList().Length;
            }
            else
            {
                return networkingPeer.mGameCount;
            }
        }
    }

    /// <summary>
    /// Enables or disables the collection of statistics about this client's traffic.
    /// If you encounter issues with clients, the traffic stats are a good starting point to find solutions.
    /// </summary>
    /// <remarks>
    /// Only with enabled stats, you can use GetVitalStats
    /// </remarks>
    public static bool NetworkStatisticsEnabled
    {
        get
        {
            return networkingPeer.TrafficStatsEnabled;
        }

        set
        {
            networkingPeer.TrafficStatsEnabled = value;
        }
    }

    /// <summary>
    /// Resets the traffic stats and re-enables them.
    /// </summary>
    public static void NetworkStatisticsReset()
    {
        networkingPeer.TrafficStatsReset();
    }


    /// <summary>
    /// Only available when NetworkStatisticsEnabled was used to gather some stats.
    /// </summary>
    /// <returns>A string with vital networking statistics.</returns>
    public static string NetworkStatisticsToString()
    {
        if (networkingPeer == null || offlineMode)
        {
            return "Offline or in OfflineMode. No VitalStats available.";
        }

        return networkingPeer.VitalStatsToString(false);
    }

    /// <summary>
    /// Static constructor used for basic setup.
    /// </summary>
    static PhotonNetwork()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying || !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        // This can happen when you recompile a script IN play made
        // This helps to surpress some errors, but will not fix breaking
        bool doubleInstall = false;
        GameObject pGO = GameObject.Find("PhotonMono");
        doubleInstall = pGO != null;
        if (doubleInstall)
        {
            GameObject.Destroy(pGO);
            Debug.LogWarning("The Unity recompile forced a restart of UnityPhoton!");
        }

#endif
        Application.runInBackground = true;

        // Set up a MonoBheaviour to run Photon, and hide it.        
        GameObject photonGO = new GameObject();
        photonMono = (PhotonHandler)photonGO.AddComponent<PhotonHandler>();
        photonGO.name = "PhotonMono";
        photonGO.hideFlags = UnityEngine.HideFlags.HideInHierarchy;

        // Set up the NetworkingPeer
        networkingPeer = new NetworkingPeer(photonMono, string.Empty, ExitGames.Client.Photon.ConnectionProtocol.Udp);
        networkingPeer.LimitOfUnreliableCommands = 20;

        // Local player
        CustomTypes.Register();
    }

    // FUNCTIONS



    /// <summary>
    /// Connect to the configured Photon server:
    /// Reads NetworkingPeer.serverSettingsAssetPath and connects to cloud or your own server.
    /// Uses: Connect(string serverAddress, int port, string appID, string gameVersion)
    /// </summary>
    /// <remarks>
    /// The PUN Setup Wizard stores your appID in a settings file and applies a server address/port, too.
    /// </remarks>
    /// <param name="gameVersion">This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).</param>
    public static void ConnectUsingSettings(string gameVersion)
    {
        if (PhotonServerSettings == null)
        {
            Debug.LogError("Loading the settings file failed. Check path: " + NetworkingPeer.serverSettingsAssetPath);
            return;
        }
        if (PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
        {
            offlineMode = true;
            return;//
        }
        else
        {
            Connect(PhotonServerSettings.ServerAddress, PhotonServerSettings.ServerPort, PhotonServerSettings.AppID, gameVersion);
        }
    }

    [System.Obsolete("This method is obsolete; use ConnectUsingSettings with the gameVersion argument instead")]
    public static void ConnectUsingSettings()
    {
        ConnectUsingSettings("1.0");
    }

    [System.Obsolete("This method is obsolete; use Connect with the gameVersion argument instead")]
    public static void Connect(string serverAddress, int port, string uniqueGameID)
    {
        Connect(serverAddress, port, uniqueGameID, "1.0");
    }

    /// <summary>
    /// Connect to the photon server by address, port, appID and game(client) version.
    /// This method is used by ConnectUsingSettings which applies values from the settings file.
    /// </summary>
    /// <param name="serverAddress">The master server's address (either your own or Photon Cloud address).</param>
    /// <param name="port">The master server's port to connect to.</param>
    /// <param name="appID">Your application ID (Photon Cloud provides you with a GUID for your game).</param>
    /// <param name="gameVersion">This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).</param>
    public static void Connect(string serverAddress, int port, string appID, string gameVersion)
    {
        if (port <= 0)
        {
            Debug.LogError("Aborted Connect: invalid port: " + port);
            return;
        }

        if (serverAddress.Length <= 2)
        {
            Debug.LogError("Aborted Connect: invalid serverAddress: " + serverAddress);
            return;
        }

        if (networkingPeer.PeerState != PeerStateValue.Disconnected)
        {
            Debug.LogWarning("Connect() only works when disconnected. Current state: " + networkingPeer.PeerState);
            return;
        }

        if (offlineMode)
        {
            offlineMode = false; // Cleanup offline mode
            Debug.LogWarning("Shut down offline mode due to a connect attempt");
        }

        if (!isMessageQueueRunning)
        {
            isMessageQueueRunning = true;
            Debug.LogWarning("Forced enabling of isMessageQueueRunning because of a Connect()");
        }

        serverAddress = serverAddress + ":" + port;

        //Debug.Log("Connecting to: " + serverAddress + " app: " + uniqueGameID);
        networkingPeer.mAppVersion = gameVersion + versionPUN;
        networkingPeer.Connect(serverAddress, appID, 0);
    }

    /// <summary>
    /// Disconnect from the photon server (also leaves your multiplayer room, if any). 
    /// OnDisconnectedFromPhoton is called when the disconnect is completed. Then you can re-connect.
    /// </summary>
    public static void Disconnect()
    {
        if (networkingPeer == null)
        {
            return; // Surpress error when quitting playmode in the editor
        }
        networkingPeer.Disconnect();
    }

    /// <summary>
    /// Set up security protocols. See documentation for details
    /// </summary>
    [System.Obsolete("Used for compatibility with Unity networking only. Does not function at the moment.")]
    public static void InitializeSecurity()
    {
        return; /*
        if (offlineMode)
        {
            return;
        }

        if (!connected)
        {
            Debug.LogError("Cannot InitializeSecurity when not connected!");
            return;
        }

        networkingPeer.EstablishEncryption();*/
    }

    /// <summary>
    /// Creates a room with given name but fails if this room is existing already.
    /// </summary>
    /// <remarks>
    /// If you don't want to create a unique room-name, pass null or "" as name and the server will assign a roomName (a GUID as string).
    /// Call this only on the master server. 
    /// Internally, the master will respond with a server-address (and roomName, if needed). Both are used internally
    /// to switch to the assigned game server and roomName.
    /// 
    /// PhotonNetwork.autoCleanUpPlayerObjects will become this room's AutoCleanUp property and that's used by all clients that join this room.
    /// </remarks>
    /// <param name="roomName">Unique name of the room to create.</param>
    public static void CreateRoom(string roomName)
    {
        Debug.Log("this custom props " + player.customProperties.ToStringFull());
        if (connectionStateDetailed == PeerState.ConnectedToGameserver || connectionStateDetailed == PeerState.Joining || connectionStateDetailed == PeerState.Joined)
        {
            Debug.LogError("CreateRoom aborted: You are already connecting to a room!");
        }
        else if (room != null)
        {
            Debug.LogError("CreateRoom aborted: You are already in a room!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
            }
            else
            {
                networkingPeer.OpCreateGame(roomName, true, true, 0, PhotonNetwork.autoCleanUpPlayerObjects, null, null);
            }
        }
    }

    /// <summary>
    /// Creates a room with given name but fails if this room is existing already.
    /// </summary>
    /// <remarks>
    /// If you don't want to create a unique room-name, pass null or "" as name and the server will assign a roomName (a GUID as string).
    /// Call this only on the master server. 
    /// Internally, the master will respond with a server-address (and roomName, if needed). Both are used internally
    /// to switch to the assigned game server and roomName
    /// </remarks>
    /// <param name="roomName">Unique name of the room to create. Pass null or "" to make the server generate a name.</param>
    /// <param name="isVisible">Shows (or hides) this room from the lobby's listing of rooms.</param>
    /// <param name="isOpen">Allows (or disallows) others to join this room.</param>
    /// <param name="maxPlayers">Max number of players that can join the room.</param>
    public static void CreateRoom(string roomName, bool isVisible, bool isOpen, int maxPlayers)
    {
        CreateRoom(roomName, isVisible, isOpen, maxPlayers, null, null);
    }

    /// <summary>
    /// Creates a room with given name but fails if this room is existing already.
    /// </summary>
    /// <remarks>
    /// If you don't want to create a unique room-name, pass null or "" as name and the server will assign a roomName (a GUID as string).
    /// Call this only on the master server. 
    /// Internally, the master will respond with a server-address (and roomName, if needed). Both are used internally
    /// to switch to the assigned game server and roomName.
    /// 
    /// PhotonNetwork.autoCleanUpPlayerObjects will become this room's AutoCleanUp property and that's used by all clients that join this room.
    /// </remarks>
    /// <param name="roomName">Unique name of the room to create. Pass null or "" to make the server generate a name.</param>
    /// <param name="isVisible">Shows (or hides) this room from the lobby's listing of rooms.</param>
    /// <param name="isOpen">Allows (or disallows) others to join this room.</param>
    /// <param name="maxPlayers">Max number of players that can join the room.</param>
    /// <param name="customRoomProperties">Custom properties of the new room (set on create, so they are immediately available).</param>
    /// <param name="propsToListInLobby">Array of custom-property-names that should be forwarded to the lobby (include only the useful ones).</param>
    public static void CreateRoom(string roomName, bool isVisible, bool isOpen, int maxPlayers, Hashtable customRoomProperties, string[] propsToListInLobby)
    {
        if (connectionStateDetailed == PeerState.Joining || connectionStateDetailed == PeerState.Joined || connectionStateDetailed == PeerState.ConnectedToGameserver)
        {
            Debug.LogError("CreateRoom aborted: You can only create a room while not currently connected/connecting to a room.");
        }
        else if (room != null)
        {
            Debug.LogError("CreateRoom aborted: You are already in a room!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
            }
            else
            {
                if (maxPlayers > 255)
                {
                    Debug.LogError("Error: CreateRoom called with " + maxPlayers + " maxplayers. This has been reverted to the max of 255 players because internally a 'byte' is used."); 
                    maxPlayers = 255;                    
                }

                networkingPeer.OpCreateGame(roomName, isVisible, isOpen, (byte)maxPlayers, PhotonNetwork.autoCleanUpPlayerObjects, customRoomProperties, propsToListInLobby);
            }
        }
    }

    /// <summary>
    /// Join room by room.Name.
    /// This fails if the room is either full or no longer available (might close at the same time).
    /// </summary>
    /// <param name="roomName">The room instance to join (only listedRoom.Name is used).</param>
    public static void JoinRoom(RoomInfo listedRoom)
    {
        if (listedRoom == null)
        {
            Debug.LogError("JoinRoom aborted: you passed a NULL room");
            return;
        }

        JoinRoom(listedRoom.name);
    }

    /// <summary>
    /// Join room with given title.
    /// This fails if the room is either full or no longer available (might close at the same time).
    /// </summary>
    /// <param name="roomName">Unique name of the room to create.</param>
    public static void JoinRoom(string roomName)
    {
        if (connectionStateDetailed == PeerState.Joining || connectionStateDetailed == PeerState.Joined || connectionStateDetailed == PeerState.ConnectedToGameserver)
        {
            Debug.LogError("JoinRoom aborted: You can only join a room while not currently connected/connecting to a room.");
        }
        else if (room != null)
        {
            Debug.LogError("JoinRoom aborted: You are already in a room!");
        }
        else if (roomName == string.Empty)
        {
            Debug.LogError("JoinRoom aborted: You must specifiy a room name!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
            }
            else
            {
                networkingPeer.OpJoin(roomName);
            }
        }
    }

    /// <summary>
    /// Joins any available room but will fail if none is currently available.
    /// </summary>
    /// <remarks>
    /// If this fails, you can still create a room (and make this available for the next who uses JoinRandomRoom).
    /// Alternatively, try again in a moment.
    /// </remarks>
    public static void JoinRandomRoom()
    {
        if (connectionStateDetailed == PeerState.Joining || connectionStateDetailed == PeerState.Joined || connectionStateDetailed == PeerState.ConnectedToGameserver)
        {
            Debug.LogError("JoinRandomRoom aborted: You can only join a room while not currently connected/connecting to a room.");
        }
        else if (room != null)
        {
            Debug.LogError("JoinRandomRoom aborted: You are already in a room!");
        }
        else
        {
            if (offlineMode)
            {
                offlineMode_inRoom = true;
                NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
            }
            else
            {
                networkingPeer.OpJoinRandomRoom(null);
            }
        }
    }

    /// <summary>
    /// Leave the current room
    /// </summary>
    public static void LeaveRoom()
    {
        if (!offlineMode && PhotonNetwork.connectionStateDetailed != PeerState.Joined)
        {
            UnityEngine.Debug.LogError("PhotonNetwork: Error, you cannot leave a room if you're not in a room!(1)");
            return;
        }
        else if (room == null)
        {
            UnityEngine.Debug.LogError("PhotonNetwork: Error, you cannot leave a room if you're not in a room!(2)");
            return;
        }

        if (offlineMode)
        {
            offlineMode_inRoom = false;
            NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom);
        }
        else
        {
            networkingPeer.OpLeave();
        }
    }

    /// <summary>
    /// Gets an array of (currently) known rooms as RoomInfo.
    /// This list is automatically updated in near-real time. The list is only updated while in the lobby.
    /// </summary>
    /// <remarks>Creates a new instance of the list each time called. Copied from networkingPeer.mGameList.</remarks>
    /// <returns>RoomInfo[] of current rooms in lobby.</returns>
    public static RoomInfo[] GetRoomList()
    {
        if (offlineMode)
        {
            return new RoomInfo[0];
        }

        if (networkingPeer == null)
        {
            return new RoomInfo[0]; // Surpress erorrs when quitting game
        }
        return networkingPeer.mGameListCopy;
    }

    /// <summary>
    /// Sets this (local) player's properties.
    /// This caches the properties in PhotonNetwork.player.customProperties.
    /// CreateRoom, JoinRoom and JoinRandomRoom will all apply your player's custom properties when you enter the room.
    /// While in a room, your properties are synced with the other players.
    /// If the Hashtable is null, the custom properties will be cleared.
    /// Custom properties are never cleared automatically, so they carry over to the next room, if you don't change them.
    /// </summary>
    /// <remarks>
    /// Don't set properties by modifying PhotonNetwork.player.customProperties!
    /// </remarks>
    /// <param name="customProperties">Only string-typed keys will be used from this hashtable. If null, custom properties are all deleted.</param>
    public static void SetPlayerCustomProperties(Hashtable customProperties)
    {
        if (customProperties == null)
        {
            customProperties = new Hashtable();
            foreach (object k in PhotonNetwork.player.customProperties.Keys)
            {
                customProperties[(string)k] = null;
            }
        }

        if (room != null && room.isLocalClientInside)
        {
            player.SetCustomProperties(customProperties);
        }
        else
        {
            player.InternalCacheProperties(customProperties);
        }
    }

    /// <summary>
    /// Request a new viewID for the local player
    /// </summary>
    /// <returns></returns>
    public static PhotonViewID AllocateViewID()
    {
        // int playerID = player.ID;
        int newID = 0;
        while (networkingPeer.allocatedIDs.ContainsKey(newID))
        {
            newID++;
        }

        if (newID >= MAX_VIEW_IDS)
        {
            Debug.LogError("ERROR: Too many view IDs used!");
            newID = 0;
        }

        int ID = newID;
        PhotonViewID viewID = new PhotonViewID(ID, player);

        networkingPeer.allocatedIDs.Add(newID, viewID);
        return viewID;
    }

    /// <summary>
    /// Unregister a view ID
    /// </summary>
    /// <param name="viewID">PhotonViewID instance</param>
    public static void UnAllocateViewID(PhotonViewID viewID)
    {
        UnAllocateViewID(viewID.ID % MAX_VIEW_IDS); //We need only the last bit, not the actor info (actor changes per room)
    }

    /// <summary>
    /// Unregister a view ID
    /// </summary>
    /// <param name="ID">ID of a PhotonView</param>
    static void UnAllocateViewID(int ID)
    {
        networkingPeer.allocatedIDs.Remove(ID);
    }



    /// <summary>
    /// Instantiate a prefab over the network. This prefab needs to be located in the root of a "Resources" folder.
    /// </summary>
    /// <remarks>Instead of using prefabs in the Resources folder, you can manually Instantiate and assign PhotonViews. See doc.</remarks>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this PhotonView.</param>
    /// <returns>The new instance of a GameObject with initialized PhotonView.</returns>
    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, int group)
    {
        return Instantiate(prefabName, position, rotation, group, null);
    }

    /// <summary>
    /// Instantiate a prefab over the network. This prefab needs to be located in the root of a "Resources" folder.
    /// </summary>
    /// <remarks>Instead of using prefabs in the Resources folder, you can manually Instantiate and assign PhotonViews. See doc.</remarks>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this PhotonView.</param>
    /// <param name="data">Optional instantiation data. This will be saved to it's PhotonView.instantiationData.</param>
    /// <returns>The new instance of a GameObject with initialized PhotonView.</returns>
    public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, int group, object[] data)
    {
        if (!VerifyCanUseNetwork())
        {
            return null;
        }

        GameObject prefabGo = (GameObject)Resources.Load(prefabName, typeof(GameObject));
        if (prefabGo == null)
        {
            Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + prefabName + "]. Please verify you have this gameobject in a Resources folder (and not in a subfolder)");
            return null;
        }
        if (prefabGo.GetComponent<PhotonView>() == null)
        {
            Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + prefabName + "] as it has no PhotonView attached to the root.");
            return null;
        }

        Component[] views = (Component[])prefabGo.GetComponentsInChildren<PhotonView>(true);
        PhotonViewID[] viewIDs = new PhotonViewID[views.Length];
        for (int i = 0; i < viewIDs.Length; i++)
        {
            viewIDs[i] = AllocateViewID();
        }

        // Send to others, create info
        Hashtable instantiateEvent = networkingPeer.SendInstantiate(prefabName, position, rotation, group, viewIDs, data, false);

        // Instantiate the GO locally (but the same way as if it was done via event). This will also cache the instantiationId
        return networkingPeer.DoInstantiate(instantiateEvent, networkingPeer.mLocalActor, prefabGo);
    }



    /// <summary>
    /// Instantiate a scene-owned prefab over the network. The PhotonViews will be controllable by the MasterClient. This prefab needs to be located in the root of a "Resources" folder.
    /// </summary>
    /// <remarks>Instead of using prefabs in the Resources folder, you can manually Instantiate and assign PhotonViews. See doc.</remarks>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this PhotonView.</param>
    /// <param name="data">Optional instantiation data. This will be saved to it's PhotonView.instantiationData.</param>
    /// <returns>The new instance of a GameObject with initialized PhotonView.</returns>
    public static GameObject InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, int group, object[] data)
    {
        if (!VerifyCanUseNetwork())
        {
            return null;
        }
        if (!isMasterClient)
        {
            Debug.LogError("PhotonNetwork error [InstantiateSceneObject]: Only the master client can Instantiate scene objects");
            return null;
        }

        GameObject prefabGo = (GameObject)Resources.Load(prefabName, typeof(GameObject));
        if (prefabGo == null)
        {
            Debug.LogError("PhotonNetwork error [InstantiateSceneObject]: Could not Instantiate the prefab [" + prefabName + "]. Please verify you have this gameobject in a Resources folder (and not in a subfolder)");
            return null;
        }
        if (prefabGo.GetComponent<PhotonView>() == null)
        {
            Debug.LogError("PhotonNetwork error [InstantiateSceneObject]: Could not Instantiate the prefab [" + prefabName + "] as it has no PhotonView attached to the root.");
            return null;
        }

        Component[] views = (Component[])prefabGo.GetComponentsInChildren<PhotonView>(true);
        PhotonViewID[] viewIDs = new PhotonViewID[views.Length];
        for (int i = 0; i < viewIDs.Length; i++)
        {
            viewIDs[i] = AllocateSceneViewID();
        }

        // Send to others, create info
        Hashtable instantiateEvent = networkingPeer.SendInstantiate(prefabName, position, rotation, group, viewIDs, data, true);

        // Instantiate the GO locally (but the same way as if it was done via event). This will also cache the instantiationId
        return networkingPeer.DoInstantiate(instantiateEvent, networkingPeer.mLocalActor, prefabGo);
    }

    /// <summary>
    /// Request a new viewID for the scene
    /// </summary>
    /// <returns></returns>
    private static PhotonViewID AllocateSceneViewID()
    {
        int newID = -1;
        for (int i = MAX_VIEW_IDS - 1; i >= 1; i--)
        {
            if (IsSceneViewIDFree(i))
            {
                newID = i;
                break;
            }
        }
        if (newID == -1)
            Debug.LogError("You ran out of scene view ID's!");

        PhotonViewID viewID = new PhotonViewID(newID, null);

        return viewID;
    }    
    private static bool IsSceneViewIDFree(int ID)
    {
        if (ID <= 0 || ID >= MAX_VIEW_IDS)
        {
            return false;
        }
        PhotonView[] photonViews = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];
        foreach (PhotonView view in photonViews)
        {
            if (!view.isSceneView)
            {
                continue;
            }
            if (view.viewID != null && view.viewID.ID == ID)
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// The current roundtrip time to the photon server
    /// </summary>
    /// <returns>Roundtrip time (to server and back).</returns>
    public static int GetPing()
    {
        return networkingPeer.RoundTripTime;
    }

    /// <summary>
    /// Can be used to immediately send the RPCs and Instantiates just made, 
    /// so they are on their way to the other players.
    /// </summary>
    /// <remarks>
    /// This could be useful if you do a RPC to load a level and then load it yourself.
    /// While loading, no RPCs are sent to others, so this would delay the "load" RPC.
    /// You can send the RPC to "others", use this method, disable the message queue 
    /// (by isMessageQueueRunning) and then load.
    /// </remarks>
    public static void SendOutgoingCommands()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        while (networkingPeer.SendOutgoingCommands())
        {
        }
    }

    /// <summary>
    /// Request a client to disconnect (KICK). Only the master client can do this.
    /// </summary>
    /// <param name="kickPlayer">The PhotonPlayer to kick.</param>
    public static void CloseConnection(PhotonPlayer kickPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (!player.isMasterClient)
        {
            Debug.LogError("CloseConnection: Only the masterclient can kick another player.");
        }

        if (kickPlayer == null)
        {
            Debug.LogError("CloseConnection: No such player connected!");
        }
        else
        {
            int[] rec = new int[1];
            rec[0] = kickPlayer.ID;
            networkingPeer.OpRaiseEvent(PhotonNetworkMessages.CloseConnection, null, true, 0, rec);
        }
    }

    /// <summary>
    /// Destroy supplied PhotonView. This will remove all Buffered RPCs and destroy the GameObject this view is attached to (plus all childs, if any)
    /// This has the same effect as calling Destroy by passing a GameObject
    /// </summary>
    /// <param name="view"></param>
    public static void Destroy(PhotonView view)
    {
        if (view != null && view.isMine)
        {
            int ID = networkingPeer.GetInstantiatedObjectsId(view.gameObject);
            if (ID == -1)
            {
                networkingPeer.DestroyPhotonView(view, false);
            }
            else
            {
                Destroy(view.gameObject); 
            }
        }
        else
        {
            Debug.LogError("Destroy: Could not destroy view ID [" + view + "]. Does not exist, or is not ours!");
        }
    }

    /// <summary>
    /// Destroys given GameObject. This GameObject must've been instantiated using PhotonNetwork.Instantiate and must have a PhotonView at it's root.
    /// This has the same effect as calling Destroy by passing an attached PhotonView from this GameObject
    /// </summary>
    /// <param name="go"></param>
    public static void Destroy(GameObject go)
    {
        PhotonView view = go.GetComponent<PhotonView>();
        if (view == null)
        {
            Debug.LogError("Cannot call Destroy(GameObject go); on the gameobject \""+go.name+"\" as it has no PhotonView attached.");
        }
        else if (view.isMine)
        {
            int ID = networkingPeer.GetInstantiatedObjectsId(go);
            if (ID == -1)
            {
                Debug.LogError("Cannot call Destroy(GameObject go); on the gameobject \"" + go.name + "\" as it was not instantiated using PhotonNetwork.Instantiate.");
            }
            else
            {
                networkingPeer.RemoveInstantiatedGO(go, false); //Success
            }
        }
        else
        {
            Debug.LogError("Cannot call Destroy(GameObject go); on the gameobject \"" + go.name + "\" as we don't control it (Owner: "+view.owner+").");
        }
    }


    /// <summary>
    /// Destroy all GameObjects/PhotonViews of this player. can only be called on the local player. The only exception is the master client which call call this for all players.
    /// </summary>
    /// <param name="player"></param>
    public static void DestroyPlayerObjects(PhotonPlayer destroyPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }
        if (player.isMasterClient || destroyPlayer == player)
        {
            networkingPeer.DestroyPlayerObjects(destroyPlayer, false);
        }
        else
        {
            Debug.LogError("Couldn't destroy objects for player \"" + destroyPlayer + "\" as we are not the masterclient.");
        }
    }

    /// <summary>
    /// MasterClient method only: Destroy ALL instantiated GameObjects
    /// </summary>
    public static void RemoveAllInstantiatedObjects()
    {
        if (isMasterClient)
        {
            networkingPeer.RemoveAllInstantiatedObjects();
        }
        else
        {
            Debug.LogError("Couldn't call RemoveAllInstantiatedObjects as only the master client is allowed to call this.");
        }
    }

    /// <summary>
    /// Destroy ALL PhotonNetwork.Instantiated GameObjects by given player. Can only be called on the local player. The only exception is the master client which call call this for all players.
    /// </summary>
    /// <param name="player"></param>
    public static void RemoveAllInstantiatedObjects(PhotonPlayer targetPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (player.isMasterClient || targetPlayer == player)
        {
            networkingPeer.RemoveAllInstantiatedObjectsByPlayer(targetPlayer, false);
        }
        else
        {
            Debug.LogError("Couldn't RemoveAllInstantiatedObjects for player \"" + targetPlayer + "\" as only the master client or the player itself is allowed to call this.");
        }      
    }

    /// <summary>
    /// Send an RPC on given PhotonView. Do not call this directly; You should use PhotonView.RPC instead!
    /// </summary>
    /// <param name="view"></param>
    /// <param name="methodName"></param>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    internal static void RPC(PhotonView view, string methodName, PhotonTargets target, params object[] parameters)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (room == null)
        {
            Debug.LogWarning("Cannot send RPCs in Lobby! RPC dropped.");
            return;
        }

        if (networkingPeer != null)
        {
            networkingPeer.RPC(view, methodName, target, parameters);
        }
        else
        {
            Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
        }
    }

    /// <summary>
    /// Send an RPC on given PhotonView. Do not call this directly; You should use PhotonView.RPC instead!
    /// </summary>
    /// <param name="view"></param>
    /// <param name="methodName"></param>
    /// <param name="targetPlayer"></param>
    /// <param name="parameters"></param>
    internal static void RPC(PhotonView view, string methodName, PhotonPlayer targetPlayer, params object[] parameters)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        if (room == null)
        {
            Debug.LogWarning("Cannot send RPCs in Lobby, only processed locally");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Error; Sending RPC to player null! Aborted \"" + methodName + "\"");
        }

        if (networkingPeer != null)
        {
            networkingPeer.RPC(view, methodName, targetPlayer, parameters);
        }
        else
        {
            Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
        }
    }

    /// <summary>
    /// Remove ALL buffered RPCs of the local player
    /// </summary>
    public static void RemoveRPCs()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }
        RemoveRPCs(player);
    }

    /// <summary>
    /// Remove ALL buffered RPCs of a player
    /// </summary>
    public static void RemoveRPCs(PhotonPlayer targetPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }
        if (!targetPlayer.isLocal && !isMasterClient)
        {
            Debug.LogError("Error; Only the MasterClient can call RemoveRPCs for other players.");
            return;
        }
        networkingPeer.RemoveRPCs(targetPlayer.ID);
    }

    /// <summary>
    /// Remove ALL buffered messages of the local player (RPC's and Instantiation calls)
    /// Note that this only removed the buffered messages on the server, you will still need to remove the Instantiated GameObjects yourself.
    /// </summary>
    public static void RemoveAllBufferedMessages()
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }
        RemoveAllBufferedMessages(player);
    }

    // <summary>
    /// Remove ALL buffered messages of a player (RPC's and Instantiation calls)
    /// Note that this only removed the buffered messages on the server, you will still need to remove the Instantiated GameObjects yourself.
    /// </summary>
    public static void RemoveAllBufferedMessages(PhotonPlayer targetPlayer)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }
        if (!targetPlayer.isLocal && !isMasterClient)
        {
            Debug.LogError("Error; Only the MasterClient can call RemoveAllBufferedMessages for other players.");
            return;
        }
        networkingPeer.RemoveCompleteCacheOfPlayer(targetPlayer.ID);
    }

    /// <summary>
    /// Remove all buffered RPCs on given PhotonView (if they are owned by this player).
    /// </summary>
    /// <param name="view"></param>
    public static void RemoveRPCs(PhotonView view)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveRPCs(view);
    }

    /// <summary>
    /// Remove all buffered RPCs with given group
    /// </summary>
    /// <param name="group"></param>
    public static void RemoveRPCsInGroup(int group)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.RemoveRPCsInGroup(group);
    }

    /// <summary>
    /// Enable/disable receiving on given group (applied to PhotonViews)
    /// </summary>
    /// <param name="group"></param>
    /// <param name="enabled"></param>
    public static void SetReceivingEnabled(int group, bool enabled)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetReceivingEnabled(group, enabled);
    }

    /// <summary>
    /// Enable/disable sending on given group (applied to PhotonViews)
    /// </summary>
    /// <param name="group"></param>
    /// <param name="enabled"></param>
    public static void SetSendingEnabled(int group, bool enabled)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetSendingEnabled(group, enabled);
    }

    /// <summary>
    /// Adds a level prefix to all PhotonViews. If any other client uses a differnt prefix, their messages will be dropped.
    /// They will also drop your messages! Be aware that PUN never resets this value, you'll have to do so yourself.
    /// </summary>
    /// <param name="prefix"></param>
    public static void SetLevelPrefix(int prefix)
    {
        if (!VerifyCanUseNetwork())
        {
            return;
        }

        networkingPeer.SetLevelPrefix(prefix);
    }

    /// <summary>
    /// Helper function which is called inside this class to erify if certain functions can be used (e.g. RPC when not connected)
    /// </summary>
    /// <returns></returns>
    private static bool VerifyCanUseNetwork()
    {
        if (networkingPeer != null && (offlineMode || connected))
        {
            return true;
        }

        Debug.LogError("Cannot send messages when not connected; Either connect to Photon OR use offline mode!");
        return false;
    }
}
