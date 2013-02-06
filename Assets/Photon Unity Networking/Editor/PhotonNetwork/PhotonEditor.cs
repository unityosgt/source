// ----------------------------------------------------------------------------
// <copyright file="PhotonEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   MenuItems and in-Editor scripts for PhotonNetwork.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.IO;
using System;


[InitializeOnLoad]
public class PhotonEditor : EditorWindow
{
    static string UrlFreeLicense = "http://www.exitgames.com/freelicense";
    static string UrlDevNet = "http://doc.exitgames.com";
    static string UrlForum = "http://forum.exitgames.com";
    static string DocumentationLocation = "Assets/Photon Unity Networking/PhotonNetwork-Documentation.pdf";

    const string UrlCompare = "http://doc.exitgames.com/v3/photoncloud/overview";
    const string UrlHowToSetup = "http://doc.exitgames.com/v3/quickstart/photoninfiveminutes";
    const string UrlAccountService = "http://service.exitgamescloud.com/Account/AccountServicePUN.svc";
    const string UrlAccountPage = "https://cloud.exitgames.com/Account/LogOn?email=";
    const string UrlAppIDExplained = "http://doc.exitgames.com/v3/photoncloud/overview";


    Vector2 scrollPos = Vector2.zero;

    enum GUIState { nullState, Main, Setup }
    enum PhotonSetupStates { _1, _2, _3a, _3b }

    GUIState guiState = GUIState.nullState;
    bool isSetupWizard = false;
    PhotonSetupStates photonSetupState = PhotonSetupStates._1;
    static bool checkedPhotonWizard = false;

    static double lastWarning = 0;

    public static ServerSettings ServerSetting = (ServerSettings)AssetDatabase.LoadAssetAtPath(NetworkingPeer.serverSettingsAssetPath, typeof(ServerSettings));

    string photonIP = "127.0.0.1";
    int photonPort = ServerSettings.DefaultMasterPort;
    string emailAddress = "";
    string cloudAPPID = "";
   
    static PhotonEditor()
    {
        EditorApplication.update += EditorUpdate;
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
    }

    [MenuItem("Window/Photon Unity Networking")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(PhotonEditor), false, "Photon Unity Networking");
    }

    void SetSetupWizard()
    {
        isSetupWizard = true;
        OpenSetup();
    }

    void SwitchMenuState(GUIState newS)
    {
        guiState = newS;
        if (isSetupWizard && newS != GUIState.Setup)
            this.Close();
    }

    static void PlaymodeStateChanged()
    {
        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (ServerSetting.HostType == ServerSettings.HostingOption.NotSet)
            {
                EditorUtility.DisplayDialog("Warning", "You have not yet run the Photon setup wizard! Your game won't be able to connect. See Windows -> Photon Unity Networking.", "Ok");
            }
        }
    }

    private static int lastPhotonViewListLength = -1;
    private static UnityEngine.Object lastFirstElement;

    static void EditorUpdate()
    {
        if (ServerSetting == null || (!checkedPhotonWizard && ServerSetting.HostType == ServerSettings.HostingOption.NotSet))
        {            
            //Debug.Log("EditorUpdate open Wizard. " + EditorApplication.isCompiling+ " SS: " + ServerSetting + " checkedPhotonWizard " + checkedPhotonWizard);
            checkedPhotonWizard = true;
            PhotonEditor window = (PhotonEditor)GetWindow(typeof(PhotonEditor), false, "PUN Setup wizard", true);
            window.ReloadHostingSettingsFromFile();
            if(ServerSetting.HostType == ServerSettings.HostingOption.NotSet)
                window.SetSetupWizard();
            window.Show();
        }


        // Workaround for TCP crash. Plus this surpresses any other recompile errors.
        if (EditorApplication.isCompiling)
        {
            if (PhotonNetwork.connected)
            {
                if (lastWarning > EditorApplication.timeSinceStartup - 3)
                {   //Prevent error spam
                    Debug.LogWarning("Unity recompile forced a Photon Disconnect");
                    lastWarning = EditorApplication.timeSinceStartup;
                }
                PhotonNetwork.Disconnect();
            }
        }
        else if (!EditorApplication.isPlaying)
        {
            //The following code could be optimized if Unity provides the right callbacks.
            // The current performance is 'OK' as we do check if the list changes (add/remove/duplicate should always change the length)

            //We are currently checking all selected PhotonViews every editor-frame
            //Instead, we only want to check this when an NEW asset is placed in a scene (at editor time)
            //We need some sort of "OnCreated" call for scene objects.
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(PhotonView), SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.Deep);
            if (objs.Length>0 && (
                objs.Length != lastPhotonViewListLength ||
                (lastFirstElement != objs[0])  
                ))
            {
                
                bool changed = false;
                foreach (UnityEngine.Object obj in objs)
                {
                    PhotonView view = obj as PhotonView;
                    if (!PhotonViewInspector.VerifySceneView(view))
                       changed = true;
                }
                if (changed)
                {
                    Debug.Log("PUN: Corrected one or more scene-PhotonViews.");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                lastPhotonViewListLength = objs.Length;
                lastFirstElement = objs[0];
            }

            // Check the newly opened scene for wrong PhotonViews
            // This can happen when changing a prefab while viewing scene A. Instances in scene B will not be corrected.
            if (lastScene != EditorApplication.currentScene && EditorApplication.currentScene!="")
            {
                lastScene = EditorApplication.currentScene;
                PhotonViewInspector.VerifyAllSceneViews();
            }
            
        }
    }

    static string lastScene = "";
    // SETUP GUI

    void OpenSetup()
    {
        SwitchMenuState(GUIState.Setup);

        this.ReloadHostingSettingsFromFile();

        switch (ServerSetting.HostType)
        {
            case ServerSettings.HostingOption.PhotonCloud:
                photonSetupState = PhotonSetupStates._3a;
                break;
            case ServerSettings.HostingOption.SelfHosted:
                photonSetupState = PhotonSetupStates._3b;
                break;
            case ServerSettings.HostingOption.NotSet:
            default:
                photonSetupState = PhotonSetupStates._1;
                break;
        }
    }


    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        if (guiState == GUIState.nullState)
        {
            ReloadHostingSettingsFromFile();
            if (ServerSetting.HostType == ServerSettings.HostingOption.NotSet)
            {
                guiState = GUIState.Setup;
            }else
                guiState = GUIState.Main;
        }

        if (guiState == GUIState.Main)
        {
            // converter
            GUILayout.BeginHorizontal();
            GUILayout.Label("Settings", EditorStyles.boldLabel, GUILayout.Width(100));
            if (GUILayout.Button(new GUIContent("Setup", "Setup wizard for setting up your own server or the cloud.")))
            {
                OpenSetup();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // converter
            GUILayout.BeginHorizontal();
            GUILayout.Label("Converter", EditorStyles.boldLabel, GUILayout.Width(100));
            if (GUILayout.Button(new GUIContent("Start", "Converts pure Unity Networking to Photon Unity Networking.")))
            {
                PhotonConverter.RunConversion();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12);


            // add PhotonView
            GUILayout.BeginHorizontal();
            GUILayout.Label("Component", EditorStyles.boldLabel, GUILayout.Width(100));
            if (GUILayout.Button(new GUIContent("Add PhotonView", "Also in menu: Component, Miscellaneous")))
            {
                if (Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.AddComponent<PhotonView>();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(22);

            // license
            GUILayout.BeginHorizontal();
            GUILayout.Label("Licenses", EditorStyles.boldLabel, GUILayout.Width(100));

            if (GUILayout.Button(new GUIContent("Download Free", "Get your free license for up to 100 concurrent players.")))
            {
                EditorUtility.OpenWithDefaultApp(UrlFreeLicense);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            // documentation
            GUILayout.BeginHorizontal();
            GUILayout.Label("Documentation", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.BeginVertical();
            if (GUILayout.Button(new GUIContent("Open PDF", "Opens the local documentation pdf.")))
            {
                EditorUtility.OpenWithDefaultApp(DocumentationLocation);
            }
            if (GUILayout.Button(new GUIContent("Open DevNet", "Online documentation for Photon.")))
            {
                EditorUtility.OpenWithDefaultApp(UrlDevNet);
            }
            if (GUILayout.Button(new GUIContent("Open Cloud Dashboard", "Review cloud information and statistics.")))
            {
                EditorUtility.OpenWithDefaultApp(UrlAccountPage + emailAddress);
            } 
            if (GUILayout.Button(new GUIContent("Open Forum", "Online support for Photon.")))
            {
                EditorUtility.OpenWithDefaultApp(UrlForum);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        else
        {
            SetupGUI();
        }
        GUILayout.EndScrollView();
    }

    void SetupGUI()
    {
        GUI.skin.label.wordWrap = true;
        if (!isSetupWizard)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close setup", GUILayout.ExpandWidth(false)))
                SwitchMenuState(GUIState.Main);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
        }

        if (photonSetupState == PhotonSetupStates._1)
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Connect to Photon Cloud");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            GUILayout.Label("Your e-mail address is required to access your own free app.");
            emailAddress = EditorGUILayout.TextField("Email:", emailAddress);

            if (GUILayout.Button("Send"))
            {
                GUIUtility.keyboardControl = 0;
                RegisterWithEmail(emailAddress);
            }

            EditorGUILayout.Separator();

            GUILayout.Label("I am already signed up. Get me to setup.");
            if (GUILayout.Button("Setup"))
                photonSetupState = PhotonSetupStates._3a;

            EditorGUILayout.Separator();
        }
        else if (photonSetupState == PhotonSetupStates._2)
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Oops!");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            GUILayout.Label("The provided e-mail-address has already been registered.");

            if (GUILayout.Button("Mh, see my account page"))
            {
                Application.OpenURL(UrlAccountPage + emailAddress);
            }

            EditorGUILayout.Separator();

            GUILayout.Label("Ah, I know my Application ID. Get me to setup.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                photonSetupState = PhotonSetupStates._1;
            if (GUILayout.Button("Setup"))
                photonSetupState = PhotonSetupStates._3a;
            GUILayout.EndHorizontal();
        }
        else if (photonSetupState == PhotonSetupStates._3a)
        {
            // cloud setup
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Connect to Photon Cloud");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            EditorGUILayout.Separator();
            this.SetupCloudAppIdGui();
            this.CompareAndHelpOptionsGui();

        }
        else if (photonSetupState == PhotonSetupStates._3b)
        {
            // self-hosting setup
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Setup own Photon Host");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            EditorGUILayout.Separator();

            this.SetupSelfhostingGui();
            this.CompareAndHelpOptionsGui();
        }
    }

    private void CompareAndHelpOptionsGui()
    {
        EditorGUILayout.Separator();
        GUILayout.Label("I am not quite sure how 'my own host' compares to 'cloud'.");
        if (GUILayout.Button("See comparison page"))
            Application.OpenURL(UrlCompare);

        EditorGUILayout.Separator();

        GUILayout.Label("Questions? Need help or want to give us feedback? You are most welcome!");
        if (GUILayout.Button("See the Photon Forum"))
            Application.OpenURL(UrlForum);
    }

    //3a
    void SetupCloudAppIdGui()
    {
        GUILayout.Label("Your APP ID:");

        cloudAPPID = EditorGUILayout.TextField(cloudAPPID);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            GUIUtility.keyboardControl = 0;
            this.ReloadHostingSettingsFromFile();
        }
        if (GUILayout.Button("Save"))
        {
            GUIUtility.keyboardControl = 0;

            ServerSetting.UseCloud(this.cloudAPPID);
            this.SavePhotonSettings();

            EditorUtility.DisplayDialog("Success", "Saved your settings.", "ok");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.Label("Running my app in the cloud was fun but...\nLet me setup my own Photon server.");

        if (GUILayout.Button("Switch to own host"))
        {
            this.photonIP = ServerSettings.DefaultServerAddress;
            this.photonPort = ServerSettings.DefaultMasterPort;
            photonSetupState = PhotonSetupStates._3b;
        }
    }

    //3b
    void SetupSelfhostingGui()
    {
        GUILayout.Label("Your Photon Host");

        photonIP = EditorGUILayout.TextField("IP:", photonIP);
        photonPort = EditorGUILayout.IntField("Port:", photonPort);
        //photonProtocol = (ExitGames.Client.Photon.ConnectionProtocol)EditorGUILayout.EnumPopup("Protocol:", photonProtocol);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            GUIUtility.keyboardControl = 0;
            this.ReloadHostingSettingsFromFile();
        }
        if (GUILayout.Button("Save"))
        {
            GUIUtility.keyboardControl = 0;

            ServerSetting.UseMyServer(this.photonIP, this.photonPort, null);
            this.SavePhotonSettings();

            EditorUtility.DisplayDialog("Success", "Saved your settings.", "ok");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.Label("Running my own server is too much hassle..\nI want to give Photon's free app a try.");

        if (GUILayout.Button("Get the free cloud app"))
        {
            this.cloudAPPID = "";
            photonSetupState = PhotonSetupStates._1;
        }

    }

    void RegisterWithEmail(string email)
    {
        EditorUtility.DisplayProgressBar("Connecting", "Connecting to the account service..", 0.5f);
        var client = new AccountService(UrlAccountService);
        Result defaultResult = new Result() { ReturnCode = -1, Message = "Account Service not available. Please use web-access and 'manual' setup." };
        Result result = defaultResult;

        try
        {
            result = client.RegisterByEmail(email);
        }
        catch (Exception e)
        {
            Debug.LogError("Error trying to reach the cloud account service. Please use web-access." + e);
        }

        if (result == null)
        {
            result = defaultResult;
        }

        EditorUtility.ClearProgressBar();
        if (result.ReturnCode == 0)
        {
            ServerSetting.UseCloud(result.Message);
            this.SavePhotonSettings();
            this.ReloadHostingSettingsFromFile();
            this.photonSetupState = PhotonSetupStates._3a;
        }
        else
        {
            if (result.Message.Contains("Email already registered"))
                this.photonSetupState = PhotonSetupStates._2;
            else
                EditorUtility.DisplayDialog("Error", result.Message, "OK");
        }
    }

    #region SettingsFileHandling

    private void SavePhotonSettings()
    {
        EditorUtility.SetDirty(ServerSetting);
    }


    void ReloadHostingSettingsFromFile()
    {
        //First try if we simply need to load the settingfile
        if (ServerSetting == null)
        {
            ServerSetting = (ServerSettings)AssetDatabase.LoadAssetAtPath(NetworkingPeer.serverSettingsAssetPath, typeof(ServerSettings));
        }
        if (ServerSetting == null)
        {
            ServerSetting = (ServerSettings)ScriptableObject.CreateInstance(typeof(ServerSettings));
            string settingsPath = Path.GetDirectoryName(NetworkingPeer.serverSettingsAssetPath);
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
                AssetDatabase.ImportAsset(settingsPath);
            }

            AssetDatabase.CreateAsset(ServerSetting, NetworkingPeer.serverSettingsAssetPath);
        }
        this.cloudAPPID = (ServerSetting.AppID == null) ? string.Empty : ServerSetting.AppID;
        this.photonIP = (ServerSetting.ServerAddress == null) ? ServerSetting.ServerAddress : string.Empty;
        this.photonPort = ServerSetting.ServerPort;
    }

        #endregion
}
