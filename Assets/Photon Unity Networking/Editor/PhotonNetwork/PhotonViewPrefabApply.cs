using UnityEngine;
using UnityEditor;

using System.Collections;

public class PhotonViewPrefabApply : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool weChangedPhotonViews = false;

        // Strips any scene settings from PhotonViews in prefabs.
        // (i.e.: assigned viewIDs are removed)
        foreach (string str in importedAssets)
        {
            if (str.EndsWith(".prefab"))
            {
                Object[] objs = (Object[])AssetDatabase.LoadAllAssetsAtPath(str);
                foreach (Object obj in objs)
                {
                    if (obj != null && obj.GetType() == typeof(GameObject))
                    {
                        PhotonView[] views = ((GameObject)obj).GetComponents<PhotonView>();
                        foreach (PhotonView view in views)
                            PhotonViewInspector.MakeProjectView(view);

                        if (views.Length > 0)
                            weChangedPhotonViews = true;
                    }
                }
            }
        }


        // Problem here: VerifyAllSceneViews will only fix the prefabs instances in the current open scene, not for other scenes!
        // See PhotonEditor.EditorUpdate: Here we will check all newly opened scenes for this possible issue.
        // No known issues as of 5 March 2011 this seems to work fine with changing viewIDs on prefabs etc. (Even for scenes that are not open) - Mike/Leepo
        if (weChangedPhotonViews)
        {
            PhotonViewInspector.VerifyAllSceneViews();
        }

    }
}

