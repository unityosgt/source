// ----------------------------------------------------------------------------
// <copyright file="PhotonPlayer.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Represents a player, identified by actorID (a.k.a. ActorNumber). 
//   Caches properties of a player.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using ExitGames.Client.Photon;

using UnityEngine;
using System.Collections;

/// <summary>
/// Summarizes a "player" within a room, identified (in that room) by actorID.
/// </summary>
/// <remarks>
/// Each player has a actorID, valid for that room. It's -1 until it's assigned by server.
/// </remarks>
public class PhotonPlayer
{
    /// <summary>This player's actorID</summary>
    public int ID
    {
        get { return this.actorID; }
    }

    /// <summary>Identifier of this player in current room.</summary>
    private int actorID = -1;

    private string nameField = "";

    /// <summary>Nickname of this player.</summary>
    public string name {
        get
        {
            return this.nameField;
        }
        set
        {
            if (!isLocal)
            {
                Debug.LogError("Error: Cannot change the name of a remote player!");
                return;
            }

            this.nameField = value;
        }
    }

    /// <summary>Only one player is controlled by each client. Others are not local.</summary>
    public readonly bool isLocal = false;

    /// <summary>
    /// The player with the lowest actorID is the master and could be used for special tasks. 
    /// </summary>
    public bool isMasterClient
    {
        get { return (PhotonNetwork.networkingPeer.mMasterClient == this); }
    }

    /// <summary>Cache for custom properties of player.</summary>
    public Hashtable customProperties { get; private set; }

    /// <summary>Creates a Hashtable with all properties (custom and "well known" ones).</summary>
    /// <remarks>If used more often, this should be cached.</remarks>
    public Hashtable allProperties
    {
        get
        {
            Hashtable allProps = new Hashtable();
            allProps.Merge(this.customProperties);
            allProps[ActorProperties.PlayerName] = this.name;
            return allProps;
        }
    }

    /// <summary>
    /// Creates a PhotonPlayer instance.
    /// </summary>
    /// <param name="isLocal">If this is the local peer's player (or a remote one).</param>
    /// <param name="actorID">ID or ActorNumber of this player in the current room (a shortcut to identify each player in room)</param>
    /// <param name="name">Name of the player (a "well known property").</param>
    public PhotonPlayer(bool isLocal, int actorID, string name)
    {
        this.customProperties = new Hashtable();
        this.isLocal = isLocal;
        this.actorID = actorID;
        this.nameField = name;
    }

    /// <summary>
    /// Internally used to create players from event Join
    /// </summary>
    internal protected PhotonPlayer(bool isLocal, int actorID, Hashtable properties)
    {
        this.customProperties = new Hashtable();
        this.isLocal = isLocal;
        this.actorID = actorID;

        this.InternalCacheProperties(properties);
    }

    /// <summary>
    /// Caches custom properties for this player.
    /// </summary>
    internal void InternalCacheProperties(Hashtable properties)
    {
        if (properties == null || properties.Count == 0 || this.customProperties.Equals(properties))
        {
            return;
        }

        if (properties.ContainsKey(ActorProperties.PlayerName))
        {
            this.nameField = (string)properties[ActorProperties.PlayerName];
        }

        this.customProperties.MergeStringKeys(properties);
        this.customProperties.StripKeysWithNullValues();
    }

    /// <summary>
    /// Gives the name.
    /// </summary>
    public override string ToString()
    {
        return (this.name == null) ? string.Empty : this.name;    // +" " + SupportClass.HashtableToString(this.CustomProperties);
    }

    /// <summary>
    /// Makes PhotonPlayer comparable
    /// </summary>
    public override bool Equals(object p)
    {
        PhotonPlayer pp = p as PhotonPlayer;
        return (pp != null && this.GetHashCode() == pp.GetHashCode());
    }

    public override int GetHashCode()
    {
        return this.ID;
    }

    /// <summary>
    /// Used internally, to update this client's playerID when assigned.
    /// </summary>
    internal void InternalChangeLocalID(int newID)
    {
        if (!this.isLocal)
        {
            Debug.LogError("ERROR You should never change PhotonPlayer IDs!");
            return;
        }

        this.actorID = newID;
    }

    /// <summary>
    /// Updates the custom properties of this Room with propertiesToSet.
    /// Only string-typed keys are applied, new properties (string keys) are added, existing are updated
    /// and if a value is set to null, this will remove the custom property.
    /// </summary>
    /// <remarks>
    /// This method requires you to be connected and be in a room. Otherwise, the local data is updated only as no remote players are known.
    /// Local cache is updated immediately, other players are updated through Photon with a fitting operation.
    /// </remarks>
    /// <param name="propertiesToSet"></param>
    public void SetCustomProperties(Hashtable propertiesToSet)
    {
        if (propertiesToSet == null)
        {
            return;
        }

        // merge (delete null-values)
        this.customProperties.MergeStringKeys(propertiesToSet); // includes a Equals check (simplifying things)
        this.customProperties.StripKeysWithNullValues();

        // send (sync) these new values
        Hashtable customProps = propertiesToSet.StripToStringKeys() as Hashtable;
        PhotonNetwork.networkingPeer.OpSetCustomPropertiesOfActor(this.actorID, customProps, true, 0);
    }

    /// <summary>
    /// Try to get a specific player by id. 
    /// </summary>
    /// <param name="ID">ActorID</param>
    /// <returns>The player with matching actorID or null, if the actorID is not in use.</returns>
    public static PhotonPlayer Find(int ID)
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.ID == ID)
                return player;
        }
        return null;
    }
}