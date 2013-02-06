using System;

using UnityEngine;
using System.Collections;

[System.Serializable]
public class ServerSettings : ScriptableObject
{
    public static string DefaultCloudServerUrl = "app.exitgamescloud.com";
    public static string DefaultServerAddress = "127.0.0.1";
    public static int DefaultMasterPort = 5055;  // default port for master server
    public static string DefaultAppID = "Master";


    public enum HostingOption { NotSet, PhotonCloud, SelfHosted, OfflineMode }

    public HostingOption HostType = HostingOption.NotSet;
    public string ServerAddress = DefaultServerAddress;
    public int ServerPort = 5055;
    public string AppID = "";

    public override string ToString()
    {
        return "ServerSettings: " + HostType + " " + ServerAddress;
    }

    public void UseCloud(string cloudAppid)
    {
        this.HostType = HostingOption.PhotonCloud;
        this.AppID = cloudAppid;
        this.ServerAddress = DefaultCloudServerUrl;
        this.ServerPort = DefaultMasterPort;
    }

    public void UseMyServer(string serverAddress, int serverPort, string application)
    {
        this.HostType = HostingOption.SelfHosted;
        this.AppID = (application != null) ? application : DefaultAppID;
        this.ServerAddress = serverAddress;
        this.ServerPort = serverPort;
    }
}
