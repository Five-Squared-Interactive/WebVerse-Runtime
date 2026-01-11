// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Materials;
using UnityEditor;

public class MaterialManagerTests
{
    [TearDown]
    public void TearDown()
    {
        // Clean up any loaded world after each test
        if (StraightFour.ActiveWorld != null)
        {
            StraightFour.UnloadWorld();
        }
    }

    [UnityTest]
    public IEnumerator MaterialManagerTests_Initialization()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        we.highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        we.previewMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        yield return null;
        StraightFour.LoadWorld("test");

        // Verify MaterialManager is initialized and accessible
        Assert.IsNotNull(StraightFour.ActiveWorld.materialManager);
        
        // Test that materials can be accessed (these may be null if not set, which is valid)
        Material highlight = MaterialManager.HighlightMaterial;
        Material preview = MaterialManager.PreviewMaterial;
        
        // The materials should be accessible without throwing exceptions
        // (They may be null if not configured, which is a valid state)
        Assert.DoesNotThrow(() => { var h = MaterialManager.HighlightMaterial; });
        Assert.DoesNotThrow(() => { var p = MaterialManager.PreviewMaterial; });
    }
}