// ----------------------------------------------------------------------------
// <copyright file="LoadbalancingPeer.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Provides the operations needed to use the loadbalancing server app(s).
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;

/// <summary>
/// A LoadbalancingPeer provides the operations and enum definitions needed to use the loadbalancing server app(s).
/// </summary>
/// <remarks>
/// The LoadBalancingPeer does not keep a state, instead this is done by a LoadBalancingClient.
/// </remarks>
public class LoadbalancingPeer : PhotonPeer
{
    public LoadbalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) : base(listener, protocolType)
    {
    }

    /// <summary>
    /// Joins the lobby on the Master Server, where you get a list of RoomInfos of currently open rooms.
    /// This is an async request which triggers a OnOperationResponse() call.
    /// </summary>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpJoinLobby()
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
        }

        return this.OpCustom(OperationCode.JoinLobby, null, true);
    }

    /// <summary>
    /// Leaves the lobby on the Master Server.
    /// This is an async request which triggers a OnOperationResponse() call.
    /// </summary>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpLeaveLobby()
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
        }

        return this.OpCustom(OperationCode.LeaveLobby, null, true);
    }

    /// <summary>
    /// Don't use this method directly, unless you know how to cache and apply customActorProperties.
    /// The PhotonNetwork methods will handle player and room properties for you and call this method.
    /// </summary>
    public virtual bool OpCreateRoom(string gameID, bool isVisible, bool isOpen, byte maxPlayers, bool autoCleanUp, Hashtable customGameProperties, Hashtable customPlayerProperties, string[] customRoomPropertiesForLobby)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
        }

        Hashtable gameProperties = new Hashtable();
        gameProperties[GameProperties.IsOpen] = isOpen;
        gameProperties[GameProperties.IsVisible] = isVisible;
        gameProperties[GameProperties.PropsListedInLobby] = customRoomPropertiesForLobby;
        gameProperties.MergeStringKeys(customGameProperties);
        if (maxPlayers > 0)
        {
            gameProperties[GameProperties.MaxPlayers] = maxPlayers;
        }

        Dictionary<byte, object> op = new Dictionary<byte, object>();
        op[ParameterCode.GameProperties] = gameProperties;
        op[ParameterCode.PlayerProperties] = customPlayerProperties;
        op[ParameterCode.Broadcast] = true;

        if (!string.IsNullOrEmpty(gameID))
        {
            op[ParameterCode.RoomName] = gameID;
        }

        if (autoCleanUp)
        {
            op[ParameterCode.CleanupCacheOnLeave] = autoCleanUp;
            gameProperties[GameProperties.CleanupCacheOnLeave] = autoCleanUp;
        }

        Listener.DebugReturn(DebugLevel.INFO, OperationCode.CreateGame + ": " + SupportClass.DictionaryToString(op));
        return this.OpCustom(OperationCode.CreateGame, op, true);
    }

    /// <summary>
    /// Joins a room by name and sets this player's properties.
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="playerProperties"></param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpJoinRoom(string roomName, Hashtable playerProperties)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRoom()");
        }

        if (string.IsNullOrEmpty(roomName))
        {
            this.Listener.DebugReturn(DebugLevel.ERROR, "OpJoinRoom() failed. Please specify a roomname.");
            return false;
        }

        Dictionary<byte, object> op = new Dictionary<byte, object>();
        op[ParameterCode.RoomName] = roomName;
        op[ParameterCode.Broadcast] = true;
        if (playerProperties != null)
        {
            op[ParameterCode.PlayerProperties] = playerProperties;
        }

        return this.OpCustom(OperationCode.JoinGame, op, true);
    }

    /// <remarks>the hashtable is (optionally) used to filter games: only those that fit the contained custom properties will be matched</remarks>
    public virtual bool OpJoinRandomRoom(Hashtable expectedGameProperties)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
        }

        Dictionary<byte, object> op = new Dictionary<byte, object>();
        if (expectedGameProperties != null && expectedGameProperties.Count > 0)
        {
            op[ParameterCode.GameProperties] = expectedGameProperties;
        }

        Listener.DebugReturn(DebugLevel.INFO, OperationCode.JoinRandomGame + ": " + SupportClass.DictionaryToString(op));
        return this.OpCustom(OperationCode.JoinRandomGame, op, true);
    }

    public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
    {
        return this.OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys(), broadcast, channelId);
    }

    protected bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
        }
            
        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters.Add(ParameterCode.Properties, actorProperties);
        opParameters.Add(ParameterCode.ActorNr, actorNr);
        if (broadcast)
        {
            opParameters.Add(ParameterCode.Broadcast, broadcast);
        }

        return this.OpCustom((byte)OperationCode.SetProperties, opParameters, broadcast, channelId);
    }

    protected void OpSetPropertyOfRoom(byte propCode, object value)
    {
        Hashtable properties = new Hashtable();
        properties[propCode] = value;
        this.OpSetPropertiesOfRoom(properties, true, (byte)0);
    }

    public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
    {
        return this.OpSetPropertiesOfRoom(gameProperties.StripToStringKeys(), broadcast, channelId);
    }

    public bool OpSetPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
        }

        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters.Add(ParameterCode.Properties, gameProperties);
        if (broadcast)
        {
            opParameters.Add(ParameterCode.Broadcast, broadcast);
        }

        return this.OpCustom((byte)OperationCode.SetProperties, opParameters, broadcast, channelId);
    }

    /// <summary>
    /// Sends this app's appId and appVersion to identify this application server side.
    /// </summary>
    /// <remarks>
    /// This operation makes use of encryption, if it's established beforehand.
    /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
    /// </remarks>
    /// <param name="appId"></param>
    /// <param name="appVersion"></param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpAuthenticate(string appId, string appVersion)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
        }

        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters[ParameterCode.AppVersion] = appVersion;
        opParameters[ParameterCode.ApplicationId] = appId;

        return this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, this.IsEncryptionAvailable);
    }

    /// <summary>
    /// Used in a room to raise (send) an event to the other players. 
    /// Multiple overloads expose different parameters to this frequently used operation.
    /// </summary>
    /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
    /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
    /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
    /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId)
    {
        return this.OpRaiseEvent(eventCode, evData, sendReliable, channelId, EventCaching.DoNotCache, ReceiverGroup.Others);
    }

    /// <summary>
    /// Used in a room to raise (send) an event to the other players. 
    /// Multiple overloads expose different parameters to this frequently used operation.
    /// </summary>
    /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
    /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
    /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
    /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
    /// <param name="targetActors">Defines the target players who should receive the event (use only for small target groups).</param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, int[] targetActors)
    {
        return this.OpRaiseEvent(eventCode, evData, sendReliable, channelId, targetActors, EventCaching.DoNotCache);
    }

    /// <summary>
    /// Used in a room to raise (send) an event to the other players. 
    /// Multiple overloads expose different parameters to this frequently used operation.
    /// </summary>
    /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
    /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
    /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
    /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
    /// <param name="targetActors">Defines the target players who should receive the event (use only for small target groups).</param>
    /// <param name="cache">Use EventCaching options to store this event for players who join.</param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, int[] targetActors, EventCaching cache)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpRaiseEvent()");
        }

        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters[ParameterCode.Data] = evData;
        opParameters[ParameterCode.Code] = (byte)eventCode;

        if (cache != EventCaching.DoNotCache)
        {
            opParameters[ParameterCode.Cache] = (byte)cache;
        }

        if (targetActors != null)
        {
            opParameters[ParameterCode.ActorList] = targetActors;
        }

        return this.OpCustom(OperationCode.RaiseEvent, opParameters, sendReliable, channelId);
    }

    /// <summary>
    /// Used in a room to raise (send) an event to the other players. 
    /// Multiple overloads expose different parameters to this frequently used operation.
    /// </summary>
    /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
    /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
    /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
    /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
    /// <param name="cache">Use EventCaching options to store this event for players who join.</param>
    /// <param name="receivers">ReceiverGroup defines to which group of players the event is passed on.</param>
    /// <returns>If the operation could be sent (has to be connected).</returns>
    public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, EventCaching cache, ReceiverGroup receivers)
    {
        if (this.DebugOut >= DebugLevel.INFO)
        {
            this.Listener.DebugReturn(DebugLevel.INFO, "OpRaiseEvent()");
        }

        Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
        opParameters[ParameterCode.Data] = evData;
        opParameters[ParameterCode.Code] = (byte)eventCode;

        if (receivers != ReceiverGroup.Others)
        {
            opParameters[ParameterCode.ReceiverGroup] = (byte)receivers;
        }

        if (cache != EventCaching.DoNotCache)
        {
            opParameters[ParameterCode.Cache] = (byte)cache;
        }

        return this.OpCustom((byte)OperationCode.RaiseEvent, opParameters, sendReliable, channelId);
    }
}

public class ErrorCode
{
    /// <summary>(0) is always "OK", anything else an error or specific situation.</summary>
    public const int Ok = 0;

    // server - Photon low(er) level: <= 0
    /// <summary>
    /// Operation can't be executed yet (e.g. OpJoin can't be called before being authenticated, RaiseEvent cant be used before getting into a room).
    /// </summary>
    /// <remarks>
    /// Before you call any operations on the Cloud servers, the automated client workflow must complete its authorization.
    /// In PUN, wait until State is: JoinedLobby (with AutoJoinLobby = true) or ConnectedToMaster (AutoJoinLobby = false)
    /// </remarks>
    public const int OperationNotAllowedInCurrentState = -3;
    /// <summary>The operation you called is not implemented on the server (application) you connect to. Make sure you run the fitting applications.</summary>
    public const int InvalidOperationCode = -2;
    /// <summary>Something went wrong in the server. Try to reproduce and contact Exit Games.</summary>
    public const int InternalServerError = -1;

    // server - PhotonNetwork: 0x7FFF and down
    // logic-level error codes start with short.max

    /// <summary>Authentication failed. Possible cause: AppId is unknown to Photon (in cloud service).</summary>
    public const int InvalidAuthentication = 0x7FFF;
    /// <summary>GameId (name) already in use (can't create another). Change name.</summary>
    public const int GameIdAlreadyExists = 0x7FFF - 1;
    /// <summary>Game is full. This can when players took over while you joined the game.</summary>
    public const int GameFull = 0x7FFF - 2;
    /// <summary>Game is closed and can't be joined. Join another game.</summary>
    public const int GameClosed = 0x7FFF - 3;
    [Obsolete("No longer used, cause random matchmaking is no longer a process.")]
    public const int AlreadyMatched = 0x7FFF - 4;
    /// <summary>Not in use currently.</summary>
    public const int ServerFull = 0x7FFF - 5;
    /// <summary>Not in use currently.</summary>
    public const int UserBlocked = 0x7FFF - 6;
    /// <summary>Random matchmaking only succeeds if a room exists thats neither closed nor full. Repeat in a few seconds or create a new room.</summary>
    public const int NoRandomMatchFound = 0x7FFF - 7;
    /// <summary>Join can fail if the room (name) is not existing (anymore). This can happen when players leave while you join.</summary>
    public const int GameDoesNotExist = 0x7FFF - 9;
}


/// <summary>
/// These (byte) values define "well known" properties for an Actor / Player.
/// </summary>
/// <remarks>
/// "Custom properties" have to use a string-type as key. They can be assigned at will.
/// </remarks>
public class ActorProperties
{
    /// <summary>(255) Name of a player/actor.</summary>
    public const byte PlayerName = 255; // was: 1
}

/// <summary>
/// These (byte) values are for "well known" room/game properties used in Photon Loadbalancing.
/// </summary>
/// <remarks>
/// "Custom properties" have to use a string-type as key. They can be assigned at will.
/// </remarks>
public class GameProperties
{
    /// <summary>(255) Max number of players that "fit" into this room. 0 is for "unlimited".</summary>
    public const byte MaxPlayers = 255;
    /// <summary>(254) Makes this room listed or not in the lobby on master.</summary>
    public const byte IsVisible = 254;
    /// <summary>(253) Allows more players to join a room (or not).</summary>
    public const byte IsOpen = 253;
    /// <summary>(252) Current count od players in the room. Used only in the lobby on master.</summary>
    public const byte PlayerCount = 252;
    /// <summary>(251) True if the room is to be removed from room listing (used in update to room list in lobby on master)</summary>
    public const byte Removed = 251;
    /// <summary>(250) A list of the room properties to pass to the RoomInfo list in a lobby. This is used in CreateRoom, which defines this list once per room.</summary>
    public const byte PropsListedInLobby = 250;
    /// <summary>Equivalent of Operation Join parameter CleanupCacheOnLeave.</summary>
    public const byte CleanupCacheOnLeave = 249;
}

/// <summary>
/// These values are for events defined by Photon Loadbalancing.
/// </summary>
/// <remarks>They start at 255 and go DOWN. Your own in-game events can start at 0.</remarks>
public class EventCode
{
    /// <summary>(230) Initial list of RoomInfos (in lobby on Master)</summary>
    public const byte GameList = 230;
    /// <summary>(229) Update of RoomInfos to be merged into "initial" list (in lobby on Master)</summary>
    public const byte GameListUpdate = 229;
    /// <summary>(228) Currently not used. State of queueing in case of server-full</summary>
    public const byte QueueState = 228;
    /// <summary>(227) Currently not used. Event for matchmaking</summary>
    public const byte Match = 227;
    /// <summary>(226) Event with stats about this application (players, rooms, etc)</summary>
    public const byte AppStats = 226;
    /// <summary>(210) Internally used in case of hosting by Azure</summary>
    public const byte AzureNodeInfo = 210;
    /// <summary>(255) Event Join: someone joined the game. The new actorNumber is provided as well as the properties of that actor (if set in OpJoin).</summary>
    public const byte Join = (byte)LiteEventCode.Join;
    /// <summary>(254) Event Leave: The player who left the game can be identified by the actorNumber.</summary>
    public const byte Leave = (byte)LiteEventCode.Leave;
    /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
    public const byte PropertiesChanged = (byte)LiteEventCode.PropertiesChanged;
    /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
    [Obsolete("Use PropertiesChanged now.")]
    public const byte SetProperties = (byte)LiteEventCode.PropertiesChanged;
}

/// <summary>Codes for parameters of Operations and Events.</summary>
public class ParameterCode
{
    /// <summary>(230) Address of a (game) server to use.</summary>
    public const byte Address = 230;
    /// <summary>(229) Count of players in this application in a rooms (used in stats event)</summary>
    public const byte PeerCount = 229;
    /// <summary>(228) Count of games in this application (used in stats event)</summary>
    public const byte GameCount = 228;
    /// <summary>(227) Count of players on the master server (in this app, looking for rooms)</summary>
    public const byte MasterPeerCount = 227;
    /// <summary>(225) User's ID</summary>
    public const byte UserId = 225;
    /// <summary>(224) Your application's ID: a name on your own Photon or a GUID on the Photon Cloud</summary>
    public const byte ApplicationId = 224;
    /// <summary>(223) Not used currently. If you get queued before connect, this is your position</summary>
    public const byte Position = 223;
    /// <summary>(222) List of RoomInfos about open / listed rooms</summary>
    public const byte GameList = 222;
    /// <summary>(221) Internally used to establish encryption</summary>
    public const byte Secret = 221;
    /// <summary>(220) Version of your application</summary>
    public const byte AppVersion = 220;
    /// <summary>(210) Internally used in case of hosting by Azure</summary>
    public const byte AzureNodeInfo = 210;	// only used within events, so use: EventCode.AzureNodeInfo
    /// <summary>(209) Internally used in case of hosting by Azure</summary>
    public const byte AzureLocalNodeId = 209;
    /// <summary>(208) Internally used in case of hosting by Azure</summary>
    public const byte AzureMasterNodeId = 208;

    /// <summary>(255) Code for the gameId/roomName (a unique name per room). Used in OpJoin and similar.</summary>
    public const byte RoomName = (byte)LiteOpKey.GameId;
    /// <summary>(250) Code for broadcast parameter of OpSetProperties method.</summary>
    public const byte Broadcast = (byte)LiteOpKey.Broadcast;
    /// <summary>(252) Code for list of players in a room. Currently not used.</summary>
    public const byte ActorList = (byte)LiteOpKey.ActorList;
    /// <summary>(254) Code of the Actor of an operation. Used for property get and set.</summary>
    public const byte ActorNr = (byte)LiteOpKey.ActorNr;
    /// <summary>(249) Code for property set (Hashtable).</summary>
    public const byte PlayerProperties = (byte)LiteOpKey.ActorProperties;
    /// <summary>(245) Code of data/custom content of an event. Used in OpRaiseEvent.</summary>
    public const byte CustomEventContent = (byte)LiteOpKey.Data;
    /// <summary>(245) Code of data of an event. Used in OpRaiseEvent.</summary>
    public const byte Data = (byte)LiteOpKey.Data;
    /// <summary>(244) Code used when sending some code-related parameter, like OpRaiseEvent's event-code.</summary>
    /// <remarks>This is not the same as the Operation's code, which is no longer sent as part of the parameter Dictionary in Photon 3.</remarks>
    public const byte Code = (byte)LiteOpKey.Code;
    /// <summary>(248) Code for property set (Hashtable).</summary>
    public const byte GameProperties = (byte)LiteOpKey.GameProperties;
    /// <summary>
    /// (251) Code for property-set (Hashtable). This key is used when sending only one set of properties.
    /// If either ActorProperties or GameProperties are used (or both), check those keys.
    /// </summary>
    public const byte Properties = (byte)LiteOpKey.Properties;
    /// <summary>(253) Code of the target Actor of an operation. Used for property set. Is 0 for game</summary>
    public const byte TargetActorNr = (byte)LiteOpKey.TargetActorNr;
    /// <summary>(246) Code to select the receivers of events (used in Lite, Operation RaiseEvent).</summary>
    public const byte ReceiverGroup = (byte)LiteOpKey.ReceiverGroup;
    /// <summary>(247) Code for caching events while raising them.</summary>
    public const byte Cache = (byte)LiteOpKey.Cache;
    /// <summary>(241) Bool parameter of CreateGame Operation. If true, server cleans up roomcache of leaving players (their cached events get removed).</summary>
    public const byte CleanupCacheOnLeave = (byte)241;
}

public class OperationCode
{
    /// <summary>(230) Authenticates this peer and connects to a virtual application</summary>
    public const byte Authenticate = 230;
    /// <summary>(229) Joins lobby (on master)</summary>
    public const byte JoinLobby = 229;
    /// <summary>(228) Leaves lobby (on master)</summary>
    public const byte LeaveLobby = 228;
    /// <summary>(227) Creates a game (or fails if name exists)</summary>
    public const byte CreateGame = 227;
    /// <summary>(226) Join game (by name)</summary>
    public const byte JoinGame = 226;
    /// <summary>(225) Joins random game (on master)</summary>
    public const byte JoinRandomGame = 225;

    // public const byte CancelJoinRandom = 224; // obsolete, cause JoinRandom no longer is a "process". now provides result immediately

    public const byte Leave = (byte)LiteOpCode.Leave;
    /// <summary>(253) Raise event (in a room, for other actors/players)</summary>
    public const byte RaiseEvent = (byte)LiteOpCode.RaiseEvent;
    /// <summary>(252) Set Properties (of room or actor/player)</summary>
    public const byte SetProperties = (byte)LiteOpCode.SetProperties;
    /// <summary>(251) Get Properties</summary>
    public const byte GetProperties = (byte)LiteOpCode.GetProperties;
}
