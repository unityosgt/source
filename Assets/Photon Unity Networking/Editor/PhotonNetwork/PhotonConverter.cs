// ----------------------------------------------------------------------------
// <copyright file="PhotonConverter.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Script to convert a Unity Networking project to PhotonNetwork.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class PhotonConverter : Photon.MonoBehaviour
{

    public static void RunConversion()
    {
        //Ask if user has made a backup.
        bool result = EditorUtility.DisplayDialog("Conversion", "Did you create a backup of your project before converting?", "Yes", "Abort conversion");
        if (!result)
        {
            return;
        }
        //REAAAALY?
        result = EditorUtility.DisplayDialog("Conversion", "Disclaimer: The code conversion feature is quite crude, but should do it's job well (see the sourcecode). A backup is therefore strongly recommended!", "Yes, I've made a backup: GO", "Abort");
        if (!result)
        {
            return;
        }
        Output(EditorApplication.timeSinceStartup + " Started conversion of Unity networking -> Photon");

        //Ask to save current scene (optional)
        EditorApplication.SaveCurrentSceneIfUserWantsTo();

        EditorUtility.DisplayProgressBar("Converting..", "Starting.", 0);

        //Convert NetworkViews to PhotonViews in Project prefabs
        //Ask the user if we can move all prefabs to a resources folder
        bool movePrefabs = EditorUtility.DisplayDialog("Conversion", "Can all prefabs that use a PhotonView be moved to a Resources/ folder? You need this if you use Network.Instantiate.", "Yes", "No");
       

        string[] prefabs = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
        foreach (string prefab in prefabs)
        {
            EditorUtility.DisplayProgressBar("Converting..", "Object:" + prefab, 0.6f);

            Object[] objs = (Object[])AssetDatabase.LoadAllAssetsAtPath(prefab);
            int converted = 0;
            foreach (Object obj in objs)
            {
                if (obj != null && obj.GetType() == typeof(GameObject))
                    converted += ConvertNetworkView(((GameObject)obj).GetComponents<NetworkView>(), false);
            }
            if (movePrefabs && converted > 0)
            {
                //This prefab needs to be under the root of a Resources folder!
                string path = prefab.Replace("\\", "/");
                int lastSlash = path.LastIndexOf("/");
                int resourcesIndex = path.LastIndexOf("/Resources/");
                if (resourcesIndex != lastSlash - 10)
                {
                    if (path.Contains("/Resources/"))
                    {
                        Debug.LogWarning("Warning, prefab [" + prefab + "] was already in a resources folder. But has been placed in the root of another one!");
                    }
                    //This prefab NEEDS to be placed under a resources folder
                    string resourcesFolder = path.Substring(0, lastSlash) + "/Resources/";
                    EnsureFolder(resourcesFolder);
                    string newPath = resourcesFolder + path.Substring(lastSlash + 1);
                    string error = AssetDatabase.MoveAsset(prefab, newPath);
                    if (error != "")
                        Debug.LogError(error);
                    Output("Fixed prefab [" + prefab + "] by moving it into a resources folder.");
                }
            }
        }
        
        //Convert NetworkViews to PhotonViews in scenes
        string[] sceneFiles = Directory.GetFiles("Assets/", "*.unity", SearchOption.AllDirectories);
        foreach (string sceneName in sceneFiles)
        {
            EditorApplication.OpenScene(sceneName);
            EditorUtility.DisplayProgressBar("Converting..", "Scene:" + sceneName, 0.2f);
        
            int converted2 = ConvertNetworkView((NetworkView[])GameObject.FindObjectsOfType(typeof(NetworkView)), true);
            if (converted2 > 0)
            {
                //This will correct all prefabs: The prefabs have gotten new components, but the correct ID's were lost in this case
                PhotonViewInspector.VerifyAllSceneViews();
                
                Output("Replaced " + converted2 + " NetworkViews with PhotonViews in scene: " + sceneName);
                EditorApplication.SaveScene(EditorApplication.currentScene);
            }
            
        }


      


        //Convert C#/JS scripts (API stuff)
        List<string> scripts = new List<string>();
        scripts.AddRange(Directory.GetFiles("Assets/", "*.cs", SearchOption.AllDirectories));
        scripts.AddRange(Directory.GetFiles("Assets/", "*.js", SearchOption.AllDirectories));
        scripts.AddRange(Directory.GetFiles("Assets/", "*.boo", SearchOption.AllDirectories));
        EditorUtility.DisplayProgressBar("Converting..", "Scripts..", 0.9f);
        ConvertScripts(scripts);

        Output(EditorApplication.timeSinceStartup + " Completed conversion!");

        EditorUtility.ClearProgressBar();
    }

    static void ConvertScripts(List<string> scripts)
    {
        foreach (string script in scripts)
        {
            if (script.Contains("PhotonNetwork"))//Don't convert this file (and others)
                continue;
            if (script.Contains("Image Effects"))
                continue;

            string text = File.ReadAllText(script);

            text = ConvertToPhotonAPI(script, text);

            File.WriteAllText(script, text);
        }
        foreach (string script in scripts){
            AssetDatabase.ImportAsset(script, ImportAssetOptions.ForceUpdate);
        }
    }

    static string ConvertToPhotonAPI(string file, string input)
    {
        bool isJS = file.Contains(".js");
        
        file =file.Replace("\\", "/"); // Get Class name for JS
        string className = file.Substring(file.LastIndexOf("/")+1);
        className = className.Substring(0, className.IndexOf("."));
         

        //REGEXP STUFF
        //Valid are: Space { } , /n /r
        //string NOT_VAR = @"([^A-Za-z0-9_\[\]\.]+)";
        string NOT_VAR_WITH_DOT = @"([^A-Za-z0-9_]+)";

        //string VAR_NONARRAY = @"[^A-Za-z0-9_]";
        
    

        //NetworkView
        {
            input = PregReplace(input, NOT_VAR_WITH_DOT + "NetworkView" + NOT_VAR_WITH_DOT, "$1PhotonView$2");
            input = PregReplace(input, NOT_VAR_WITH_DOT + "networkView" + NOT_VAR_WITH_DOT, "$1photonView$2");
            input = PregReplace(input, NOT_VAR_WITH_DOT + "stateSynchronization" + NOT_VAR_WITH_DOT, "$1synchronization$2");
            //.RPC
            input = PregReplace(input, NOT_VAR_WITH_DOT + "RPCMode.Server" + NOT_VAR_WITH_DOT, "$1PhotonTargets.MasterClient$2");
            input = PregReplace(input, NOT_VAR_WITH_DOT + "RPCMode" + NOT_VAR_WITH_DOT, "$1PhotonTargets$2");
        }

        //NetworkMessageInfo: 100%
        {
            input = PregReplace(input, NOT_VAR_WITH_DOT + "NetworkMessageInfo" + NOT_VAR_WITH_DOT, "$1PhotonMessageInfo$2");
            input = PregReplace(input, NOT_VAR_WITH_DOT + "networkView" + NOT_VAR_WITH_DOT, "$1photonView$2");
        }

        //NetworkViewID:
        {
            input = PregReplace(input, NOT_VAR_WITH_DOT + "NetworkViewID" + NOT_VAR_WITH_DOT, "$1PhotonViewID$2");
        }

        //NetworkPlayer
        {
            input = PregReplace(input, NOT_VAR_WITH_DOT + "NetworkPlayer" + NOT_VAR_WITH_DOT, "$1PhotonPlayer$2");
        }

        //Network
        {
            //Monobehaviour callbacks
            {
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnPlayerConnected" + NOT_VAR_WITH_DOT, "$1OnPhotonPlayerConnected$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnPlayerDisconnected" + NOT_VAR_WITH_DOT, "$1OnPhotonPlayerDisconnected$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnNetworkInstantiate" + NOT_VAR_WITH_DOT, "$1OnPhotonInstantiate$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnSerializeNetworkView" + NOT_VAR_WITH_DOT, "$1OnPhotonSerializeView$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "BitStream" + NOT_VAR_WITH_DOT, "$1PhotonStream$2");

                //Not completely the same meaning
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnServerInitialized" + NOT_VAR_WITH_DOT, "$1OnCreatedRoom$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnConnectedToServer" + NOT_VAR_WITH_DOT, "$1OnJoinedRoom$2");

                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnFailedToConnectToMasterServer" + NOT_VAR_WITH_DOT, "$1OnFailedToConnectToPhoton$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "OnFailedToConnect" + NOT_VAR_WITH_DOT, "$1OnFailedToConnect_OBSELETE$2");
            }

            //Variables
            {

                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.connections" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.playerList$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.isServer" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.isMasterClient$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.isClient" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.isNonMasterClientInRoom$2");

                input = PregReplace(input, NOT_VAR_WITH_DOT + "NetworkPeerType" + NOT_VAR_WITH_DOT, "$1ConnectionState$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.peerType" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.connectionState$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "ConnectionState.Server" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.isMasterClient$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "ConnectionState.Client" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.isNonMasterClientInRoom$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "PhotonNetwork.playerList.Length" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.playerList.Count$2");
                
                /*DROPPED:
                    minimumAllocatableViewIDs 
	                natFacilitatorIP is dropped
	                natFacilitatorPort is dropped
	                connectionTesterIP
	                connectionTesterPort
	                proxyIP
	                proxyPort
	                useProxy
	                proxyPassword
                 */
            }

            //Methods
            {
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.InitializeServer" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.JoinRoom$2");//Either Join or Create room
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.Connect" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.JoinRoom$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.GetAveragePing" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.GetPing$2");
                input = PregReplace(input, NOT_VAR_WITH_DOT + "Network.GetLastPing" + NOT_VAR_WITH_DOT, "$1PhotonNetwork.GetPing$2");
                /*DROPPED:
                    TestConnection
                    TestConnectionNAT
                    HavePublicAddress                
                */
            }

            //Overall
            input = PregReplace(input, NOT_VAR_WITH_DOT + "Network" + NOT_VAR_WITH_DOT, "$1PhotonNetwork$2");
        }

        //General
        {
            if (input.Contains("Photon")) //Only use the PhotonMonoBehaviour if we use photonView and friends.
            {
                if (isJS)//JS
                {
                    if (input.Contains("extends MonoBehaviour"))
                        input = PregReplace(input, "extends MonoBehaviour", "extends Photon.MonoBehaviour");
                    else
                        input = "class " + className + " extends Photon.MonoBehaviour {\n" + input + "\n}";
                }
                else //C#
                    input = PregReplace(input, ": MonoBehaviour", ": Photon.MonoBehaviour");
            }
        }


        return input;
    }

    static string PregReplace(string input, string[] pattern, string[] replacements)
    {
        if (replacements.Length != pattern.Length)
            Debug.LogError("Replacement and Pattern Arrays must be balanced");

        for (var i = 0; i < pattern.Length; i++)
        {
            input = Regex.Replace(input, pattern[i], replacements[i]);
        }

        return input;
    }
    static string PregReplace(string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement);

    }

    static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }
    }

    static int ConvertNetworkView(NetworkView[] netViews, bool isScene)
    {
        for (int i = netViews.Length - 1; i >= 0; i--)
        {
            NetworkView netView = netViews[i];
            PhotonView view = netView.gameObject.AddComponent<PhotonView>();
            if (isScene)
            {
                //Get scene ID
                string str = netView.viewID.ToString().Replace("SceneID: ", "");
                int firstSpace = str.IndexOf(" ");
                str = str.Substring(0, firstSpace);
                int oldViewID = int.Parse(str);

                view.viewID = new PhotonViewID(oldViewID, null);
                view.SetSceneID(oldViewID);
                EditorUtility.SetDirty(view);
                EditorUtility.SetDirty(view.gameObject);
            }
            view.observed = netView.observed;
            if (netView.stateSynchronization == NetworkStateSynchronization.Unreliable)
            {
                view.synchronization = ViewSynchronization.Unreliable;
            }
            else if (netView.stateSynchronization == NetworkStateSynchronization.ReliableDeltaCompressed)
            {
                view.synchronization = ViewSynchronization.ReliableDeltaCompressed;
            }
            else
            {
                view.synchronization = ViewSynchronization.Off;
            }
            DestroyImmediate(netView, true);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        return netViews.Length;
    }


    static void Output(string str)
    {
        Debug.Log(((int)EditorApplication.timeSinceStartup) + " " + str);
    }
    static void ConversionError(string file, string str)
    {
        Debug.LogError("Scrip conversion[" + file + "]: " + str);
    }

}
