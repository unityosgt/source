// ----------------------------------------------------------------------------
// <copyright file="PhotonViewInspector.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Custom inspector for the PhotonView component.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(PhotonView))]
public class PhotonViewInspector : Editor
{
    private bool doubleView = false;

    private List<Component> FindAllComponents(System.Type typ, Transform trans)
    {
        List<Component> comps = new List<Component>();
        comps.AddRange((Component[])trans.GetComponents(typ));
        foreach (Transform child in trans)
        {
            comps.AddRange(FindAllComponents(typ, child));
        }
        return comps;
    }

    private static PhotonView lastView;

    private static GameObject GetPrefabParent(GameObject mp)
    {
#if UNITY_2_6_1 || UNITY_2_6 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4
        return (EditorUtility.GetPrefabParent(mp.gameObject) as GameObject);
#else
        //Introduced in 3.5
        return (PrefabUtility.GetPrefabObject(mp.gameObject) as GameObject);
#endif
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        EditorGUI.indentLevel = 1;

        PhotonView mp = (PhotonView)target;
        bool isProjectPrefab = EditorUtility.IsPersistent(mp.gameObject);

        if (!EditorApplication.isPlaying)
        {
            if (mp != lastView)
            {
                //First opening of this viewID

                if (!isProjectPrefab)
                {
                    if (!IsSceneViewIDFree(mp.viewID.ID, mp))
                    {
                        Debug.LogWarning("PhotonView: Wrong view ID(" + mp.viewID.ID + ") on " + mp.name + ", checking entire scene for fixes...");
                        VerifyAllSceneViews();
                    }
                }
                lastView = mp;
            }
        }



        SerializedObject sObj = new SerializedObject(mp);
        SerializedProperty sceneProp = sObj.FindProperty("sceneViewID");
        //SerializedProperty sceneProp2 = sObj.FindProperty("isSceneView");

        //FIX for an issue where a prefab(with photon view) is dragged to the scene and its changes APPLIED
        //This means that the scene assigns a ID, but this ID may not be saved to the prefab.
        //Unity's prefab AssetImporter doesn't seem to work (3.4), hence this nasty workaround.
        //Desired values:
        //scene = true true     proj = false false. Thus, error case=


        if (sceneProp.isInstantiatedPrefab && !sceneProp.prefabOverride)
        {
            //Fix the assignment
            //EDIT: THIS ISSUE HAS BEEN FIXED IN PHOTONVIEW.CS BY CHECKING FOR THE PhotonViewSetup_FindMatchingRoot in Setup();
            //#if !UNITY_3_5
            //            sceneProp.prefabOverride = true;
            //#endif


            sObj.ApplyModifiedProperties();

            //FIX THE EDITOR PREFAB: set it to 0
            GameObject pPrefab = GetPrefabParent(mp.gameObject).transform.root.gameObject;
            List<Component> views = FindAllComponents(typeof(PhotonView), pPrefab.transform);
            foreach (Component viewX in views)
            {
                MakeProjectView((PhotonView)viewX);
            }

            //Force reimport of prefab
            GameObject pPrefab2 = GetPrefabParent(mp.gameObject);
            if (pPrefab2 != null)
            {
                pPrefab2 = pPrefab2.transform.root.gameObject;
                string assetPath2 = AssetDatabase.GetAssetPath(pPrefab2);
                if (assetPath2 == "") Debug.LogError("No assetpath for " + pPrefab2);
                AssetDatabase.ImportAsset(assetPath2, ImportAssetOptions.ForceUpdate);
            }

            //Assign the desired scene IDs back (they were reset to 0 by applying)
            PhotonView[] views2 = mp.transform.root.GetComponentsInChildren<PhotonView>();
            foreach (Component viewX in views2)
            {
                PhotonView view = (PhotonView)viewX;
                int wantedID = view.viewID.ID;
                view.SetSceneID(wantedID);
                EditorUtility.SetDirty(view);
            }
        }


        //Setup

        if (!isProjectPrefab)
        {

            if (mp.viewID.ID == 0)
                SetViewID(mp, GetFreeSceneID(mp));

        }
        else
        {

            if (mp.viewID.ID != 0 || mp.isSceneView)
            {
                //Correct the settings
                Debug.LogWarning("Correcting view ID on project prefab (should be unassigned, but it was " + mp.viewID.ID + ")");
                MakeProjectView(mp);
            }
        }

        //OWNER
        if (isProjectPrefab)
        {
            EditorGUILayout.LabelField("Owner:", "Set at runtime");
        }
        else if (mp.owner != null)
        {
            EditorGUILayout.LabelField("Owner:", "[" + mp.owner.ID + "] " + mp.owner.name);
        }
        else
        {
            EditorGUILayout.LabelField("Owner:", "Scene");
        }

        //View ID
        if (isProjectPrefab)
        {
            EditorGUILayout.LabelField("View ID", "Set at runtime");
        }
        else if (EditorApplication.isPlaying)
        {
            if (mp.owner != null)
                EditorGUILayout.LabelField("View ID", "[" + mp.owner.ID + "] " + mp.viewID);
            else
                EditorGUILayout.LabelField("View ID", mp.viewID + "");
        }
        else
        {
            int newID = EditorGUILayout.IntField("View ID", mp.viewID.ID);
            if (GUI.changed)
            {
                SetViewID(mp, newID);
            }


            if (doubleView)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("ERROR:", "Invalid view ID");
                GUI.color = Color.white;
            }

            if (GUI.changed)
            {
                ChangedSetting();
                doubleView = false;
                PhotonView[] photonViews = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];
                foreach (PhotonView view in photonViews)
                {
                    if (view.isSceneView && view.viewID == mp.viewID && view != mp)
                    {
                        doubleView = true;
                        EditorUtility.DisplayDialog("Error", "There is already a viewID with ID=" + view.viewID, "OK");
                    }
                }
            }
        }

        //OBSERVING    

        EditorGUILayout.BeginHorizontal();
        //Using a lower version then 3.4? Remove the TRUE in the next line to fix an compile error

        string title = "";
        int firstOpen = 0;
        if (mp.observed != null) firstOpen = (mp.observed).ToString().IndexOf('(');
        if (firstOpen > 0)
            title = mp.observed.ToString().Substring(firstOpen - 1);
        mp.observed = (Component)EditorGUILayout.ObjectField("Observe: " + title, mp.observed, typeof(Component), true);
        if (GUI.changed)
        {
            ChangedSetting();
            if (mp.observed != null)
            {
                mp.synchronization = ViewSynchronization.ReliableDeltaCompressed;
            }
            else
            {
                mp.synchronization = ViewSynchronization.Off;
            }
        }
        EditorGUILayout.EndHorizontal();


        if (mp.synchronization == ViewSynchronization.Off)
        {
            GUI.color = Color.grey;
        }

        mp.synchronization = (ViewSynchronization)EditorGUILayout.EnumPopup("Observe option:", mp.synchronization);
        if (GUI.changed)
        {
            ChangedSetting();
            if (mp.synchronization != ViewSynchronization.Off && mp.observed == null)
                EditorUtility.DisplayDialog("Warning", "Setting the synchronization option only makes sense if you observe something.", "OK, I will fix it.");

        }
        if (mp.observed != null)
        {
            System.Type type = mp.observed.GetType();
            if (type == typeof(Transform))
            {
                mp.onSerializeTransformOption = (OnSerializeTransform)EditorGUILayout.EnumPopup("Serialization:", mp.onSerializeTransformOption);
            }
            else if (type == typeof(Rigidbody))
            {
                mp.onSerializeRigidBodyOption = (OnSerializeRigidBody)EditorGUILayout.EnumPopup("Serialization:", mp.onSerializeRigidBodyOption);
            }
        }

        GUI.color = Color.white;
        EditorGUIUtility.LookLikeControls();
    }

    void ChangedSetting()
    {
        PhotonView mp = (PhotonView)target;
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(mp);
        }
    }

    public static void MakeProjectView(PhotonView view)
    {
        view.viewID = new PhotonViewID(0, null);
        view.SetSceneID(0);
        EditorUtility.SetDirty(view);
    }

    static void SetViewID(PhotonView mp, int ID)
    {
        ID = Mathf.Clamp(ID, 1, PhotonNetwork.MAX_VIEW_IDS - 1);

        if (!IsSceneViewIDFree(ID, mp))
            ID = GetFreeSceneID(mp);

        if (mp.viewID.ID != ID)
        {
            mp.viewID = new PhotonViewID(ID, null);
        }
        if (!EditorApplication.isPlaying)
        {
            mp.SetSceneID(mp.viewID.ID);
            GameObject pPrefab = GetPrefabParent(mp.gameObject);
            if (pPrefab != null)
            {
                pPrefab = pPrefab.transform.root.gameObject;
                string assetPath = AssetDatabase.GetAssetPath(pPrefab);
                if (assetPath == "") Debug.LogError("No assetpath for " + pPrefab);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
        EditorUtility.SetDirty(mp);
    }


    static int GetFreeSceneID(PhotonView targetView)
    {
        //No need for bit shifting as scene is "player 0".
        /* Hashtable takenIDs = new Hashtable();
         PhotonView[] views = (PhotonView[])GameObject.FindObjectsOfType(typeof(PhotonView));
         foreach (PhotonView view in views)
         {
             takenIDs[view.viewID] = view;
         }*/

        for (int i = 1; i < PhotonNetwork.MAX_VIEW_IDS; i++)
        {
            if (IsSceneViewIDFree(i, targetView))
                return i;
        }
        EditorUtility.DisplayDialog("Error", "You ran out of view ID's (" + PhotonNetwork.MAX_VIEW_IDS + "). Something is seriously wrong!", "OK");
        return 1;
    }

    static bool IsSceneViewIDFree(int ID, PhotonView targetView)
    {
        if (ID <= 0)
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
            if (view != targetView && view.viewID != null && view.viewID.ID == ID)
            {
                return false;
            }
        }
        return true;
    }

    public static void VerifyAllSceneViews()
    {
        int correctedViews = 0;
        PhotonView[] photonViews = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];
        foreach (PhotonView view in photonViews)
        {
            if (!VerifySceneView(view)) correctedViews++;
        }
        if (correctedViews > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public static bool VerifySceneView(PhotonView view)
    {
        if (!EditorUtility.IsPersistent(view.gameObject) && !IsSceneViewIDFree(view.viewID.ID, view))
        {
            SetViewID(view, GetFreeSceneID(view));
            return false;
        }
        return true;
    }
}