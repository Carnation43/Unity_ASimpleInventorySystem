using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Debug_IncrementalUpdate : MonoBehaviour
{
    string serverUrl = "file:///" + Application.dataPath + "/AssetBundles/";

    string mainManifest = "AssetBundles";

    private IEnumerator Start()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        Debug.Log("---- Game Starts, Checking Updates ----");

        string mainfestUrl = serverUrl + mainManifest;

        Debug.Log($"Downloading manifest: {mainfestUrl}");
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(mainfestUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("The Server disconnected!" + request.error);
            yield break;
        }

        AssetBundle manifestBundle = DownloadHandlerAssetBundle.GetContent(request);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifestBundle.Unload(false);

        string[] allBundles = manifest.GetAllAssetBundles();
        Debug.Log($"The Server has {allBundles.Length} bundles...");

        foreach (string bundleName in allBundles)
        {
            Hash128 serverHash = manifest.GetAssetBundleHash(bundleName);

            string bundleUrl = serverUrl + bundleName;
            UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, serverHash, 0);

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                bool isCached = Caching.IsVersionCached(bundleName, serverHash);

                if (uwr.downloadedBytes > 0)
                {
                    Debug.Log($"<color=yellow>[Updating...]</color> {bundleName}(has been updated)");
                }
                else
                {
                    Debug.Log($"<color=green>[Loading from cache...]</color> {bundleName}(is the latest!)");
                }

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                
                if (bundle != null) bundle.Unload(true);
            }
            else
            {
                Debug.LogError($"Download Failed! {bundleName} - {uwr.error}");
            }
        }

        Debug.Log("--- Checking for updating is completed, all resources ready to load");

    }
}
