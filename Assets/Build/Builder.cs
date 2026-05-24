// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FiveSQD.WebVerse.Building
{
    /// <summary>
    /// Preprocess build hook that removes Meta XR AIBlocks from the package cache
    /// before compilation. AIBlocks uses UnityEngine.Microphone, which is unavailable
    /// on WebGL and on non-VR Android player scripting contexts, causing CS0103.
    /// 
    /// Runs automatically as part of BuildPipeline.BuildPlayer, so any build
    /// initiated through Builder.cs (or any other path) is covered.
    /// </summary>
    public class StripIncompatiblePackages : IPreprocessBuildWithReport
    {
        // Run very early so the strip happens before script compilation.
        public int callbackOrder => -10000;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildTarget target = report.summary.platform;

            // AIBlocks is incompatible with WebGL and standard (non-Quest) Android.
            // The iOS, Mac, and Windows builds compile it fine, so leave them alone.
            bool shouldStrip = target == BuildTarget.WebGL
                            || target == BuildTarget.Android;

            if (!shouldStrip)
            {
                return;
            }

            StripAIBlocks(target);
        }

        private static void StripAIBlocks(BuildTarget target)
        {
            const string packageCacheRoot = "Library/PackageCache";

            if (!Directory.Exists(packageCacheRoot))
            {
                Debug.LogWarning($"[StripIncompatiblePackages] {packageCacheRoot} not found; skipping AIBlocks strip.");
                return;
            }

            string[] metaCoreDirs = Directory.GetDirectories(packageCacheRoot, "com.meta.xr.sdk.core@*");
            if (metaCoreDirs.Length == 0)
            {
                Debug.Log("[StripIncompatiblePackages] Meta XR Core package not found in cache; nothing to strip.");
                return;
            }

            bool stripped = false;
            foreach (string metaCoreDir in metaCoreDirs)
            {
                string aiBlocksPath = Path.Combine(metaCoreDir, "Scripts", "BuildingBlocks", "AIBlocks");
                string aiBlocksMeta = aiBlocksPath + ".meta";

                if (Directory.Exists(aiBlocksPath))
                {
                    Directory.Delete(aiBlocksPath, recursive: true);
                    Debug.Log($"[StripIncompatiblePackages] Removed for {target}: {aiBlocksPath}");
                    stripped = true;
                }

                if (File.Exists(aiBlocksMeta))
                {
                    File.Delete(aiBlocksMeta);
                }
            }

            if (stripped)
            {
                // Force Unity to rescan so the deleted files don't get picked up
                // by the upcoming script compilation pass.
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.Log($"[StripIncompatiblePackages] AIBlocks folder already absent for {target}; no strip needed.");
            }
        }
    }
}
#endif
