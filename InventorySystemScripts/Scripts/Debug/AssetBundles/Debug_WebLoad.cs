using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Debug_WebLoad : MonoBehaviour
{
    public string bundleName = "triangle";

    private void Start()
    {
        // StartCoroutine(InstantiateObject());

        // LoadAssetManifest();
    }

    private IEnumerator InstantiateObject()
    {
        string uri = "file:///" + Application.dataPath + "/AssetBundles/" + "environment/" + bundleName;

        Debug.Log("Downloading from the server: " + uri);

        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);

        yield return request.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);


        //Object[] objects = bundle.LoadAllAssets();

        //foreach (var obj in objects)
        //{
        //    Instantiate(obj);
        //    yield return new WaitForSeconds(1.0f);
        //}

        AssetBundleRequest requestAsync = bundle.LoadAssetAsync<GameObject>("Test_Prefab_ForAB");

        yield return requestAsync;
        var loadedAsset = requestAsync.asset;

        Instantiate(loadedAsset);
    }

    private void LoadAssetManifest()
    {
        string manifestFilePath = Path.Combine(Application.dataPath, "AssetBundles", "AssetBundles");

        AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestFilePath);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] dependencies = manifest.GetAllDependencies("environment/triangle");
        foreach (string dependency in dependencies)
        {
            AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Application.dataPath, "AssetBundles"), dependency));
        }
    }
}
