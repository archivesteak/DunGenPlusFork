using DunGenPlus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;
using LethalLevelLoader;
using DunGen.Graph;
using DunGen;
using System.Reflection;
using Unity.Netcode;

namespace DunGenPlus {

  internal class Assets {

    public static void LoadAssets() {
      foreach (string text in Directory.GetFiles(Paths.PluginPath, "*.lethalbundle", SearchOption.AllDirectories)) {
        FileInfo fileInfo = new FileInfo(text);
        LethalLevelLoader.AssetBundleLoader.AddOnLethalBundleLoadedListener(AutoAddLethalBundle, fileInfo.Name);
      }
    }

    static void AutoAddLethalBundle(AssetBundle assetBundle) {
      if (assetBundle.isStreamedSceneAssetBundle) return;

      var extenders = assetBundle.LoadAllAssets<DunGenExtender>();
      var content = assetBundle.LoadAllAssets<ExtendedContent>();

      if (content.Length == 0 && extenders.Length > 0) {
        Plugin.logger.LogWarning($".lethalbundle does not contain any ExtendedContent. Unless you are manually creating and adding your ExtendedDungeonFlow with code, the DunGenExtender will probably not work.");
      }

      foreach (var e in extenders) {
        API.AddDunGenExtender(e);
      }
    }

    public static AssetBundle MainAssetBundle = null;
    public static GameObject DevDebugPrefab;

    public static T Load<T>(string name, bool onlyReportErrors = true) where T : UnityEngine.Object {
      if (MainAssetBundle == null) {
        Plugin.logger.LogError($"Trying to load in asset but asset bundle is missing");
        return null;
      }

      var asset = MainAssetBundle.LoadAsset<T>(name);
      var missingasset = asset == null;

      if (missingasset || onlyReportErrors == true) {
        Plugin.logger.LogDebug($"Loading asset {name}");
      }

      if (missingasset) {
        Plugin.logger.LogError($"...but it was not found");
      }
      return asset;
    }

    public static void LoadAssetBundle() {
      if (MainAssetBundle == null) {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        if (resourceNames.Length >= 1) {
          var name = resourceNames[0];
          using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)) {
            Plugin.logger.LogDebug($"Loading resource {name}");
            MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
          }
        }
      }

      DevDebugPrefab = Load<GameObject>("DevDebug");

    }

  }
}
