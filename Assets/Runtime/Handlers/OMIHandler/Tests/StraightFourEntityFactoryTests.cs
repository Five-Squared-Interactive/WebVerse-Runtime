// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using FiveSQD.StraightFour.Entity;
using OMI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Unit tests for StraightFourEntityFactory extension detection.
/// </summary>
public class StraightFourEntityFactoryTests
{
    /// <summary>
    /// Helper method to create a test context with glTF JSON data.
    /// </summary>
    private OMIImportContext CreateTestContext(JObject gltfJson)
    {
        var context = new OMIImportContext
        {
            CustomData = new System.Collections.Generic.Dictionary<string, object>
            {
                ["SF_GltfJson"] = gltfJson
            }
        };
        return context;
    }

    /// <summary>
    /// Helper to create glTF JSON with specific node extensions.
    /// </summary>
    private JObject CreateGltfWithExtensions(int nodeIndex, string[] extensions, Quaternion? rotation = null)
    {
        var nodes = new JArray();

        // Create nodes up to and including the target node
        for (int i = 0; i <= nodeIndex; i++)
        {
            var node = new JObject();

            if (i == nodeIndex && extensions != null && extensions.Length > 0)
            {
                var extensionsObj = new JObject();
                foreach (var ext in extensions)
                {
                    extensionsObj[ext] = new JObject(); // Empty extension data
                }
                node["extensions"] = extensionsObj;
            }

            // Add rotation if specified and this is the target node
            if (i == nodeIndex && rotation.HasValue)
            {
                var q = rotation.Value;
                node["rotation"] = new JArray(q.x, q.y, q.z, q.w);
            }

            nodes.Add(node);
        }

        return new JObject
        {
            ["nodes"] = nodes
        };
    }

    /// <summary>
    /// Helper to create glTF JSON with parent-child relationship.
    /// </summary>
    private JObject CreateGltfWithChildren(int parentIndex, int[] childIndices, string parentExtension, string childExtension, Quaternion? childRotation = null)
    {
        var nodes = new JArray();

        int maxIndex = Mathf.Max(parentIndex, childIndices.Length > 0 ? childIndices[childIndices.Length - 1] : 0);

        for (int i = 0; i <= maxIndex; i++)
        {
            var node = new JObject();

            // Parent node
            if (i == parentIndex)
            {
                if (!string.IsNullOrEmpty(parentExtension))
                {
                    node["extensions"] = new JObject
                    {
                        [parentExtension] = new JObject()
                    };
                }

                // Add children array
                var childrenArray = new JArray();
                foreach (int childIdx in childIndices)
                {
                    childrenArray.Add(childIdx);
                }
                node["children"] = childrenArray;
            }
            // Child nodes
            else if (System.Array.IndexOf(childIndices, i) >= 0)
            {
                if (!string.IsNullOrEmpty(childExtension))
                {
                    node["extensions"] = new JObject
                    {
                        [childExtension] = new JObject()
                    };
                }

                // Add rotation to child if specified
                if (childRotation.HasValue)
                {
                    var q = childRotation.Value;
                    node["rotation"] = new JArray(q.x, q.y, q.z, q.w);
                }
            }

            nodes.Add(node);
        }

        return new JObject
        {
            ["nodes"] = nodes
        };
    }

    [Test]
    public void TestHasExtension_NodeWithExtension_ReturnsTrue()
    {
        // Arrange
        var gltf = CreateGltfWithExtensions(0, new[] { "OMI_vehicle_body" });
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("Test"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsTrue(entity is AutomobileEntity || entity is AirplaneEntity,
            "Node with OMI_vehicle_body should create a vehicle entity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestHasWheels_NodeWithWheelChildren_CreatesAutomobile()
    {
        // Arrange: Parent node (0) has vehicle_body, child node (1) has vehicle_wheel
        var gltf = CreateGltfWithChildren(
            parentIndex: 0,
            childIndices: new[] { 1 },
            parentExtension: "OMI_vehicle_body",
            childExtension: "OMI_vehicle_wheel"
        );
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("VehicleWithWheels"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<AutomobileEntity>(entity,
            "Node with vehicle_body + wheels should create AutomobileEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestHasVerticalThrusters_NodeWithUpwardThruster_CreatesAirplane()
    {
        // Arrange: Parent node (0) has vehicle_body, child node (1) has vertical thruster
        // Rotation that makes forward (+Z) point up (+Y): rotate -90 degrees around X axis
        Quaternion verticalRotation = Quaternion.Euler(-90, 0, 0);

        var gltf = CreateGltfWithChildren(
            parentIndex: 0,
            childIndices: new[] { 1 },
            parentExtension: "OMI_vehicle_body",
            childExtension: "OMI_vehicle_thruster",
            childRotation: verticalRotation
        );
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("VehicleWithVerticalThruster"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<AirplaneEntity>(entity,
            "Node with vehicle_body + vertical thrusters should create AirplaneEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestIsAircraft_NoWheelsNoThrusters_CreatesAirplane()
    {
        // Arrange: Node with vehicle_body but no wheels or thrusters (glider)
        var gltf = CreateGltfWithExtensions(0, new[] { "OMI_vehicle_body" });
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("Glider"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<AirplaneEntity>(entity,
            "Node with vehicle_body but no wheels should create AirplaneEntity (glider)");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestHasExtension_AudioEmitter_CreatesAudioEntity()
    {
        // Arrange
        var gltf = CreateGltfWithExtensions(0, new[] { "KHR_audio_emitter" });
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("Audio"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<AudioEntity>(entity,
            "Node with KHR_audio_emitter should create AudioEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestNoExtensions_WithMesh_CreatesMeshEntity()
    {
        // Arrange
        var gltf = CreateGltfWithExtensions(0, new string[] { });
        var context = CreateTestContext(gltf);

        var go = new GameObject("MeshNode");
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();
        go.AddComponent<MeshRenderer>();

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(go, 0, context);

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<MeshEntity>(entity,
            "Node with mesh but no extensions should create MeshEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestNoExtensions_NoMesh_CreatesContainerEntity()
    {
        // Arrange
        var gltf = CreateGltfWithExtensions(0, new string[] { });
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("Empty"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<ContainerEntity>(entity,
            "Empty node with no extensions should create ContainerEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestHorizontalThruster_CreatesAutomobile()
    {
        // Arrange: Thruster pointing forward (default rotation = horizontal)
        var gltf = CreateGltfWithChildren(
            parentIndex: 0,
            childIndices: new[] { 1 },
            parentExtension: "OMI_vehicle_body",
            childExtension: "OMI_vehicle_thruster",
            childRotation: Quaternion.identity // Forward thrust (horizontal)
        );
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("VehicleWithHorizontalThruster"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<AutomobileEntity>(entity,
            "Node with horizontal thruster (no vertical component) should create AutomobileEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }

    [Test]
    public void TestPersonalityExtension_CreatesCharacterEntity()
    {
        // Arrange
        var gltf = CreateGltfWithExtensions(0, new[] { "OMI_personality" });
        var context = CreateTestContext(gltf);

        // Act
        var entity = StraightFourEntityFactory.CreateEntityFromNode(
            new GameObject("Character"),
            0,
            context
        );

        // Assert
        Assert.IsNotNull(entity);
        Assert.IsInstanceOf<CharacterEntity>(entity,
            "Node with OMI_personality should create CharacterEntity");

        // Cleanup
        Object.DestroyImmediate(entity.gameObject);
    }
}
#endif
