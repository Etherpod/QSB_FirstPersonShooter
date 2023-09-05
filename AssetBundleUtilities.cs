using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace qsbFPS
{
    public static class AssetBundleUtilities
    {
        public static Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();

        public static void ClearCache()
        {
            foreach (var pair in AssetBundles)
            {
                if (pair.Value == null) qsbFPS.Instance.ModHelper.Console.WriteLine($"The asset bundle for {pair.Key} was null when trying to unload", MessageType.Error);
                else pair.Value.Unload(true);
            }
            AssetBundles.Clear();
        }

        public static T Load<T>(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod) where T : UnityEngine.Object
        {
            string key = Path.GetFileName(assetBundleRelativeDir);
            T obj;

            try
            {
                AssetBundle bundle;

                if (AssetBundles.ContainsKey(key))
                {
                    bundle = AssetBundles[key];
                }
                else
                {
                    var completePath = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, assetBundleRelativeDir);
                    bundle = AssetBundle.LoadFromFile(completePath);
                    if (bundle == null)
                    {
                        qsbFPS.Instance.ModHelper.Console.WriteLine($"Couldn't load AssetBundle at [{completePath}] for [{mod.ModHelper.Manifest.Name}]", MessageType.Error);
                        return null;
                    }

                    AssetBundles[key] = bundle;
                }

                obj = bundle.LoadAsset<T>(pathInBundle);
            }
            catch (Exception e)
            {
                qsbFPS.Instance.ModHelper.Console.WriteLine($"Couldn't load asset {pathInBundle} from AssetBundle {assetBundleRelativeDir}:\n{e}", MessageType.Error);
                return null;
            }

            return obj;
        }

        public static GameObject LoadPrefab(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod)
        {
            var prefab = Load<GameObject>(assetBundleRelativeDir, pathInBundle, mod);

            prefab.SetActive(false);

            ReplaceShaders(prefab);

            return prefab;
        }

        public static void ReplaceShaders(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    var replacementShader = Shader.Find(material.shader.name);
                    if (replacementShader == null) continue;

                    // preserve override tag and render queue (for Standard shader)
                    // keywords and properties are already preserved
                    if (material.renderQueue != material.shader.renderQueue)
                    {
                        var renderType = material.GetTag("RenderType", false);
                        var renderQueue = material.renderQueue;
                        material.shader = replacementShader;
                        material.SetOverrideTag("RenderType", renderType);
                        material.renderQueue = renderQueue;
                    }
                    else
                    {
                        material.shader = replacementShader;
                    }
                }
            }
        }
    }
}