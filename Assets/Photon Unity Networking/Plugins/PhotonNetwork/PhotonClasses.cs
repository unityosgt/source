// ----------------------------------------------------------------------------
// <copyright file="PhotonClasses.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

// Enums and classes

class PhotonNetworkMessages
{
    public const byte RPC = 200; 
    public const byte SendSerialize = 201;
    public const byte Instantiation = 202;
    public const byte CloseConnection = 203;
    public const byte Destroy = 204;
    public const byte RemoveCachedRPCs = 205;

}

public enum PhotonTargets { All, Others, MasterClient, AllBuffered, OthersBuffered } //.MasterClientBuffered? .Server?
public enum PhotonLogLevel { ErrorsOnly, Informational, Full }


namespace Photon
{
    public class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        public PhotonView photonView
        {
            get
            {
                return PhotonView.Get(this);
            }
        }
        new public PhotonView networkView
        {
            get
            {
                Debug.LogWarning("Why are you still using networkView? should be PhotonView?");
                return PhotonView.Get(this);
            }
        }
    }
}

public class PhotonViewID 
{
    private PhotonPlayer internalOwner;
    private int internalID = -1; // 1-256 (1-MAX_NETWORKVIEWS)
    
    public PhotonViewID(int ID, PhotonPlayer owner)
    {
        internalID = ID;
        internalOwner = owner;
    }

    public int ID
    {   
        // PLAYERNR*MAX_NETWORKVIEWS + internalID
        get
        {
            if(internalOwner == null)
            {
                //Scene ID
                return internalID;
            }
            else
            {
                return (internalOwner.ID*PhotonNetwork.MAX_VIEW_IDS) + internalID;
            }
        }
    }

    public bool isMine
    {
        get { return owner.isLocal; }
    }

    public PhotonPlayer owner
    {
        get
        {
            int ownerNR = ID / PhotonNetwork.MAX_VIEW_IDS;
            return PhotonPlayer.Find(ownerNR);
        }
    }

    public override string ToString()
    {
        return this.ID.ToString();
    }

    public override bool Equals(object p)
    {
        PhotonViewID pp = p as PhotonViewID;
        return (pp != null && this.ID == pp.ID);
    }

    public override int GetHashCode()
    {
        return this.ID;
    }

    [System.Obsolete("Used for compatibility with Unity networking only.")]
    public static PhotonViewID unassigned
    {
        get
        {
            return new PhotonViewID(-1, null);
        }
    }
}

public class PhotonMessageInfo
{
    private int timeInt;
    public PhotonPlayer sender;
    public PhotonView photonView;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotonMessageInfo"/> class. 
    /// To create an empty messageinfo only!
    /// </summary>
    public PhotonMessageInfo()
    {
        this.sender = PhotonNetwork.player;
        this.timeInt = (int)(PhotonNetwork.time * 1000);
        this.photonView = null;
    }

    public PhotonMessageInfo(PhotonPlayer player, int timestamp, PhotonView view)
    {
        this.sender = player;
        this.timeInt = timestamp;
        this.photonView = view;
    }

    public double timestamp
    {
        get { return ((double)(uint)this.timeInt) / 1000.0f; }
    }

    public override string ToString()
    {
        return string.Format("[PhotonMessageInfo: player='{1}' timestamp={0}]", this.timestamp, this.sender);
    }
}

public class PhotonStream
{
    bool write = false;
    List<object> data;
    byte currentItem = 0; //Used to track the next item to receive.

    public PhotonStream(bool write, object[] incomingData)
    {
        this.write = write;
        if (incomingData == null)
        {
            this.data = new List<object>();
        }
        else
        {
            this.data = new List<object>(incomingData);
        }
    }

    public bool isWriting
    {
        get { return this.write; }
    }

    public bool isReading
    {
        get { return !this.write; }
    }

    public int Count
    {
        get
        {
            return data.Count;
        }
    }

    public object ReceiveNext()
    {
        if (this.write)
        {
            Debug.LogError("Error: you cannot read this stream that you are writing!");
            return null;
        }

        object obj = this.data[this.currentItem];
        this.currentItem++;
        return obj;
    }

    public void SendNext(object obj)
    {
        if (!this.write)
        {
            Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
            return;
        }

        this.data.Add(obj);
    }

    public object[] ToArray()
    {
        return this.data.ToArray();
    }

    public void Serialize(ref bool myBool)
    {
        if (this.write)
        {
            this.data.Add(myBool);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                myBool = (bool)data[currentItem];
                this.currentItem++;
            }
        }
    }

    public void Serialize(ref int myInt)
    {
        if (write)
        {
            this.data.Add(myInt);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                myInt = (int)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref string value)
    {
        if (write)
        {
            this.data.Add(value);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                value = (string)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref char value)
    {
        if (write)
        {
            this.data.Add(value);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                value = (char)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref short value)
    {
        if (write)
        {
            this.data.Add(value);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                value = (short)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref float obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                obj = (float)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref PhotonPlayer obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                obj = (PhotonPlayer)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref Vector3 obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                obj = (Vector3)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref Vector2 obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                obj = (Vector2)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref Quaternion obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            if (this.data.Count > currentItem)
            {
                obj = (Quaternion)data[currentItem];
                currentItem++;
            }
        }
    }

    public void Serialize(ref PhotonViewID obj)
    {
        if (write)
        {
            this.data.Add(obj);
        }
        else
        {
            int ID = (int)data[currentItem];
            currentItem++;
            
            int internalID = ID % PhotonNetwork.MAX_VIEW_IDS;
            int actorID = ID / PhotonNetwork.MAX_VIEW_IDS;
            PhotonPlayer owner = null;
            if (actorID > 0)
            {
                owner = PhotonPlayer.Find(actorID);
            }

            obj = new PhotonViewID(internalID, owner);
        }
    }
}