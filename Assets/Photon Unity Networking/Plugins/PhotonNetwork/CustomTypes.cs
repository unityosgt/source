// ----------------------------------------------------------------------------
// <copyright file="CustomTypes.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using System;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

public static class CustomTypes
{

    public static void Register()
    {
        PhotonPeer.RegisterType(typeof(Vector2), (byte)'W', SerializeVector2, DeserializeVector2);
        PhotonPeer.RegisterType(typeof(Vector3), (byte)'V', SerializeVector3, DeserializeVector3);
        PhotonPeer.RegisterType(typeof(Transform), (byte)'T', SerializeTransform, DeserializeTransform);
        PhotonPeer.RegisterType(typeof(Quaternion), (byte)'Q', SerializeQuaternion, DeserializeQuaternion);
        PhotonPeer.RegisterType(typeof(PhotonPlayer), (byte)'P', SerializePhotonPlayer, DeserializePhotonPlayer);
        PhotonPeer.RegisterType(typeof(PhotonViewID), (byte)'I', SerializePhotonViewID, DeserializePhotonViewID);
    }

    #region Custom De/Serializer Methods

    private static byte[] SerializeTransform(object customobject)
    {
        Transform t = (Transform)customobject;

        Vector3[] parts = new Vector3[2];
        parts[0] = t.position;
        parts[1] = t.eulerAngles;

        return Protocol.Serialize(parts);
    }

    private static object DeserializeTransform(byte[] serializedcustomobject)
    {
        object x = Protocol.Deserialize(serializedcustomobject);
        return x;
    }

    private static byte[] SerializeVector3(object customobject)
    {
        Vector3 vo = (Vector3)customobject;
        MemoryStream ms = new MemoryStream(3 * 4);

        ms.Write(BitConverter.GetBytes(vo.x), 0, 4);
        ms.Write(BitConverter.GetBytes(vo.y), 0, 4);
        ms.Write(BitConverter.GetBytes(vo.z), 0, 4);
        return ms.ToArray();
    }

    private static object DeserializeVector3(byte[] bytes)
    {
        Vector3 vo = new Vector3();
        vo.x = BitConverter.ToSingle(bytes, 0);
        vo.y = BitConverter.ToSingle(bytes, 4);
        vo.z = BitConverter.ToSingle(bytes, 8);

        return vo;
    }

    private static byte[] SerializeVector2(object customobject)
    {
        Vector2 vo = (Vector2)customobject;
        MemoryStream ms = new MemoryStream(2 * 4);

        ms.Write(BitConverter.GetBytes(vo.x), 0, 4);
        ms.Write(BitConverter.GetBytes(vo.y), 0, 4);
        return ms.ToArray();
    }

    private static object DeserializeVector2(byte[] bytes)
    {
        Vector2 vo = new Vector2();
        vo.x = BitConverter.ToSingle(bytes, 0);
        vo.y = BitConverter.ToSingle(bytes, 4);
        return vo;
    }
    public static byte[] SerializeQuaternion(object obj)
    {
        Quaternion o = (Quaternion)obj;
        MemoryStream ms = new MemoryStream(3 * 4);

        ms.Write(BitConverter.GetBytes(o.w), 0, 4);
        ms.Write(BitConverter.GetBytes(o.x), 0, 4);
        ms.Write(BitConverter.GetBytes(o.y), 0, 4);
        ms.Write(BitConverter.GetBytes(o.z), 0, 4);
        return ms.ToArray();
    }

    public static object DeserializeQuaternion(byte[] bytes)
    {
        Quaternion o = new Quaternion();
        o.w = BitConverter.ToSingle(bytes, 0);
        o.x = BitConverter.ToSingle(bytes, 4);
        o.y = BitConverter.ToSingle(bytes, 8);
        o.z = BitConverter.ToSingle(bytes, 12);

        return o;
    }

    private static byte[] SerializePhotonPlayer(object customobject)
    {
        int ID = ((PhotonPlayer)customobject).ID;
        return BitConverter.GetBytes(ID);
    }

    private static object DeserializePhotonPlayer(byte[] bytes)
    {
        int ID = BitConverter.ToInt32(bytes, 0);
        if(PhotonNetwork.networkingPeer.mActors.ContainsKey(ID))
        {
            return PhotonNetwork.networkingPeer.mActors[ID];
        }
        else
        {
            return null;
        }
    }

    private static byte[] SerializePhotonViewID(object customobject)
    {
        int ID = ((PhotonViewID)customobject).ID;
        return BitConverter.GetBytes(ID);
    }

    private static object DeserializePhotonViewID(byte[] bytes)
    {
        int ID = BitConverter.ToInt32(bytes, 0);
        int internalID = ID % PhotonNetwork.MAX_VIEW_IDS;
        int actorID = ID / PhotonNetwork.MAX_VIEW_IDS;
        PhotonPlayer owner = null;
        if (actorID > 0)
        {
            owner = PhotonPlayer.Find(actorID);
        }

        return new PhotonViewID(internalID, owner);
    }

    #endregion
}