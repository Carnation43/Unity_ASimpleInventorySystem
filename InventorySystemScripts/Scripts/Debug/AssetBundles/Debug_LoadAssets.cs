using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Debug_LoadAssets : MonoBehaviour
{
    

    // ---------------------- A Test for Importing A prefab with material from the AssetBundle ----------------------
    //private void Start()
    //{
    //    var materialAB = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "bundle_a"));
    //    var sharedAB = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "bundle_shared"));
    //    var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath,"AssetBundles","environment","triangle"));

    //    if (materialAB == null)
    //    {
    //        Debug.Log("Failed to load materialAB");
    //        return;
    //    }

    //    if (myLoadedAssetBundle == null)
    //    {
    //        Debug.Log("Failed to load AssetBundle!");
    //        return;
    //    }

    //    var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("Test_Prefab_ForAB");
    //    Instantiate(prefab);
    //}

    AssetBundle myBundle;
    GameObject myPrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            string path = Path.Combine(Application.dataPath, "AssetBundles", "bundle_shared");
            myBundle = AssetBundle.LoadFromFile(path);
            Debug.Log("[Debug_LoadAssets] AssetBundle object loaded!");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (myBundle != null)
            {
                var asset = myBundle.LoadAsset("Texture_Shared");
                Debug.Log("[Debug_LoadAssets] Get the Texture_Shared from bundle_shared.");
            }
        }
    }

    private void Start()
    {
        StartCoroutine(TestLoadForIncrementalUpdate());
    }

    private IEnumerator TestLoadForIncrementalUpdate()
    {
        yield return new WaitForSeconds(3.0f);

        var materialAB = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "bundle_a"));
        var sharedAB = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "bundle_shared"));

        var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "environment", "triangle"));
        var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("Test_Prefab_ForAB");
        Instantiate(prefab);
    }
}
