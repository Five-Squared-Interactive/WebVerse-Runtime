// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using Newtonsoft.Json;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for JSON entity data classes in the JSONEntityHandler.
/// Tests serialization and deserialization of all JSON entity types
/// using Newtonsoft.Json round-trip patterns.
/// </summary>
public class JSONEntityHandlerTests
{
    #region JSONVector3 Tests

    [Test]
    public void JSONVector3_DefaultConstructor_HasZeroValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var v = new JSONVector3();

        Assert.AreEqual(0f, v.x);
        Assert.AreEqual(0f, v.y);
        Assert.AreEqual(0f, v.z);
    }

    [Test]
    public void JSONVector3_FromUnityVector3_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var unity = new Vector3(1.5f, 2.5f, 3.5f);
        var json = new JSONVector3(unity);

        Assert.AreEqual(1.5f, json.x);
        Assert.AreEqual(2.5f, json.y);
        Assert.AreEqual(3.5f, json.z);
    }

    [Test]
    public void JSONVector3_ToVector3_ReturnsCorrectUnityVector()
    {
        LogAssert.ignoreFailingMessages = true;

        var json = new JSONVector3 { x = 10f, y = 20f, z = 30f };
        var unity = json.ToVector3();

        Assert.AreEqual(10f, unity.x);
        Assert.AreEqual(20f, unity.y);
        Assert.AreEqual(30f, unity.z);
    }

    [Test]
    public void JSONVector3_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var original = new JSONVector3 { x = 1.1f, y = 2.2f, z = 3.3f };
        string serialized = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<JSONVector3>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.x, deserialized.x);
        Assert.AreEqual(original.y, deserialized.y);
        Assert.AreEqual(original.z, deserialized.z);
    }

    [Test]
    public void JSONVector3_DeserializeFromJsonString_ParsesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        string json = "{\"x\":5.0,\"y\":10.0,\"z\":15.0}";
        var v = JsonConvert.DeserializeObject<JSONVector3>(json);

        Assert.IsNotNull(v);
        Assert.AreEqual(5.0f, v.x);
        Assert.AreEqual(10.0f, v.y);
        Assert.AreEqual(15.0f, v.z);
    }

    #endregion

    #region JSONQuaternion Tests

    [Test]
    public void JSONQuaternion_DefaultConstructor_HasZeroValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var q = new JSONQuaternion();

        Assert.AreEqual(0f, q.x);
        Assert.AreEqual(0f, q.y);
        Assert.AreEqual(0f, q.z);
        Assert.AreEqual(0f, q.w);
    }

    [Test]
    public void JSONQuaternion_FromUnityQuaternion_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var unity = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        var json = new JSONQuaternion(unity);

        Assert.AreEqual(0.1f, json.x);
        Assert.AreEqual(0.2f, json.y);
        Assert.AreEqual(0.3f, json.z);
        Assert.AreEqual(0.9f, json.w);
    }

    [Test]
    public void JSONQuaternion_ToQuaternion_ReturnsCorrectUnityQuaternion()
    {
        LogAssert.ignoreFailingMessages = true;

        var json = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f };
        var unity = json.ToQuaternion();

        Assert.AreEqual(0f, unity.x);
        Assert.AreEqual(0f, unity.y);
        Assert.AreEqual(0f, unity.z);
        Assert.AreEqual(1f, unity.w);
    }

    [Test]
    public void JSONQuaternion_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var original = new JSONQuaternion { x = 0.1f, y = 0.2f, z = 0.3f, w = 0.9f };
        string serialized = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<JSONQuaternion>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.x, deserialized.x);
        Assert.AreEqual(original.y, deserialized.y);
        Assert.AreEqual(original.z, deserialized.z);
        Assert.AreEqual(original.w, deserialized.w);
    }

    #endregion

    #region JSONColor Tests

    [Test]
    public void JSONColor_DefaultConstructor_HasAlphaOne()
    {
        LogAssert.ignoreFailingMessages = true;

        var c = new JSONColor();

        Assert.AreEqual(0f, c.r);
        Assert.AreEqual(0f, c.g);
        Assert.AreEqual(0f, c.b);
        Assert.AreEqual(1f, c.a);
    }

    [Test]
    public void JSONColor_FromUnityColor_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var unity = new Color(0.5f, 0.6f, 0.7f, 0.8f);
        var json = new JSONColor(unity);

        Assert.AreEqual(0.5f, json.r);
        Assert.AreEqual(0.6f, json.g);
        Assert.AreEqual(0.7f, json.b);
        Assert.AreEqual(0.8f, json.a);
    }

    [Test]
    public void JSONColor_ToColor_ReturnsCorrectUnityColor()
    {
        LogAssert.ignoreFailingMessages = true;

        var json = new JSONColor { r = 1f, g = 0f, b = 0f, a = 1f };
        var unity = json.ToColor();

        Assert.AreEqual(1f, unity.r);
        Assert.AreEqual(0f, unity.g);
        Assert.AreEqual(0f, unity.b);
        Assert.AreEqual(1f, unity.a);
    }

    [Test]
    public void JSONColor_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var original = new JSONColor { r = 0.1f, g = 0.2f, b = 0.3f, a = 0.4f };
        string serialized = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<JSONColor>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.r, deserialized.r);
        Assert.AreEqual(original.g, deserialized.g);
        Assert.AreEqual(original.b, deserialized.b);
        Assert.AreEqual(original.a, deserialized.a);
    }

    #endregion

    #region JSONVector2 Tests

    [Test]
    public void JSONVector2_DefaultConstructor_HasZeroValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var v = new JSONVector2();

        Assert.AreEqual(0f, v.x);
        Assert.AreEqual(0f, v.y);
    }

    [Test]
    public void JSONVector2_ParameterizedConstructor_SetsValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var v = new JSONVector2(3.5f, 7.5f);

        Assert.AreEqual(3.5f, v.x);
        Assert.AreEqual(7.5f, v.y);
    }

    [Test]
    public void JSONVector2_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var original = new JSONVector2(4.4f, 5.5f);
        string serialized = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<JSONVector2>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.x, deserialized.x);
        Assert.AreEqual(original.y, deserialized.y);
    }

    #endregion

    #region JSONContainerEntity Tests

    [Test]
    public void JSONContainerEntity_DefaultConstructor_InitializesChildrenList()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONContainerEntity();

        Assert.IsNotNull(entity.children);
        Assert.AreEqual(0, entity.children.Count);
    }

    [Test]
    public void JSONContainerEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONContainerEntity
        {
            id = "container-001",
            tag = "TestContainer",
            position = new JSONVector3 { x = 1f, y = 2f, z = 3f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 1f, y = 1f, z = 1f },
            isSize = false,
            parentId = "parent-001"
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONContainerEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("container-001", deserialized.id);
        Assert.AreEqual("TestContainer", deserialized.tag);
        Assert.AreEqual(1f, deserialized.position.x);
        Assert.AreEqual(2f, deserialized.position.y);
        Assert.AreEqual(3f, deserialized.position.z);
        Assert.AreEqual(1f, deserialized.rotation.w);
        Assert.AreEqual(1f, deserialized.scale.x);
        Assert.AreEqual(false, deserialized.isSize);
        Assert.AreEqual("parent-001", deserialized.parentId);
    }

    [Test]
    public void JSONContainerEntity_WithNestedChildren_SerializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var parent = new JSONContainerEntity
        {
            id = "parent",
            tag = "Parent",
            position = new JSONVector3 { x = 0f, y = 0f, z = 0f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 1f, y = 1f, z = 1f }
        };

        var child = new JSONContainerEntity
        {
            id = "child",
            tag = "Child",
            position = new JSONVector3 { x = 5f, y = 5f, z = 5f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 1f, y = 1f, z = 1f },
            parentId = "parent"
        };

        parent.children.Add(child);

        string serialized = JsonConvert.SerializeObject(parent);
        var deserialized = JsonConvert.DeserializeObject<JSONContainerEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1, deserialized.children.Count);
        Assert.AreEqual("child", deserialized.children[0].id);
        Assert.AreEqual(5f, deserialized.children[0].position.x);
    }

    [Test]
    public void JSONContainerEntity_DeserializeFromJsonString_ParsesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        string json = @"{
            ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
            ""tag"": ""MyContainer"",
            ""position"": { ""x"": 0, ""y"": 1, ""z"": 0 },
            ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
            ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
            ""isSize"": false,
            ""parentId"": null,
            ""children"": []
        }";

        var entity = JsonConvert.DeserializeObject<JSONContainerEntity>(json);

        Assert.IsNotNull(entity);
        Assert.AreEqual("550e8400-e29b-41d4-a716-446655440000", entity.id);
        Assert.AreEqual("MyContainer", entity.tag);
        Assert.AreEqual(1f, entity.position.y);
        Assert.AreEqual(1f, entity.rotation.w);
        Assert.IsNotNull(entity.children);
        Assert.AreEqual(0, entity.children.Count);
    }

    #endregion

    #region JSONMeshEntity Tests

    [Test]
    public void JSONMeshEntity_DefaultConstructor_InitializesDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONMeshEntity();

        Assert.IsNotNull(entity.meshResources);
        Assert.AreEqual(0, entity.meshResources.Length);
        Assert.IsNotNull(entity.children);
        Assert.AreEqual(0, entity.children.Count);
    }

    [Test]
    public void JSONMeshEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONMeshEntity
        {
            id = "mesh-001",
            tag = "TestMesh",
            position = new JSONVector3 { x = 2f, y = 1f, z = 0f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 1f, y = 1f, z = 1f },
            isSize = false,
            parentId = "container-001",
            meshType = "primitive",
            meshSource = "cube",
            meshResources = new string[] { "texture1.png", "texture2.png" },
            color = new JSONColor { r = 1f, g = 0f, b = 0f, a = 1f }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONMeshEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("mesh-001", deserialized.id);
        Assert.AreEqual("TestMesh", deserialized.tag);
        Assert.AreEqual("primitive", deserialized.meshType);
        Assert.AreEqual("cube", deserialized.meshSource);
        Assert.AreEqual(2, deserialized.meshResources.Length);
        Assert.AreEqual("texture1.png", deserialized.meshResources[0]);
        Assert.IsNotNull(deserialized.color);
        Assert.AreEqual(1f, deserialized.color.r);
        Assert.AreEqual(0f, deserialized.color.g);
    }

    [Test]
    public void JSONMeshEntity_DeserializeFromJsonString_ParsesPrimitiveEntity()
    {
        LogAssert.ignoreFailingMessages = true;

        string json = @"{
            ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
            ""tag"": ""MyCube"",
            ""position"": { ""x"": 2, ""y"": 1, ""z"": 0 },
            ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
            ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
            ""isSize"": false,
            ""meshType"": ""primitive"",
            ""meshSource"": ""cube"",
            ""color"": { ""r"": 1, ""g"": 0, ""b"": 0, ""a"": 1 },
            ""children"": []
        }";

        var entity = JsonConvert.DeserializeObject<JSONMeshEntity>(json);

        Assert.IsNotNull(entity);
        Assert.AreEqual("MyCube", entity.tag);
        Assert.AreEqual("primitive", entity.meshType);
        Assert.AreEqual("cube", entity.meshSource);
        Assert.AreEqual(1f, entity.color.r);
    }

    [Test]
    public void JSONMeshEntityCollection_RoundTripSerialization_PreservesEntities()
    {
        LogAssert.ignoreFailingMessages = true;

        var collection = new JSONMeshEntityCollection();
        collection.meshEntities = new JSONMeshEntity[]
        {
            new JSONMeshEntity
            {
                id = "mesh-a",
                tag = "MeshA",
                meshType = "primitive",
                meshSource = "sphere"
            },
            new JSONMeshEntity
            {
                id = "mesh-b",
                tag = "MeshB",
                meshType = "gltf",
                meshSource = "model.glb"
            }
        };

        string serialized = JsonConvert.SerializeObject(collection);
        var deserialized = JsonConvert.DeserializeObject<JSONMeshEntityCollection>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(2, deserialized.meshEntities.Length);
        Assert.AreEqual("mesh-a", deserialized.meshEntities[0].id);
        Assert.AreEqual("mesh-b", deserialized.meshEntities[1].id);
        Assert.AreEqual("gltf", deserialized.meshEntities[1].meshType);
    }

    #endregion

    #region JSONTerrainEntity Tests

    [Test]
    public void JSONTerrainEntity_DefaultConstructor_InitializesDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTerrainEntity();

        Assert.IsNotNull(entity.position);
        Assert.IsNotNull(entity.rotation);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
        Assert.AreEqual(1f, entity.scale.y);
        Assert.AreEqual(1f, entity.scale.z);
        Assert.AreEqual("heightmap", entity.terrainType);
        Assert.AreEqual(false, entity.stitchTerrains);
        Assert.IsNotNull(entity.children);
        Assert.AreEqual(0, entity.children.Length);
    }

    [Test]
    public void JSONTerrainEntity_RoundTripSerialization_PreservesHeightsAndLayers()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTerrainEntity
        {
            id = "terrain-001",
            tag = "TestTerrain",
            terrainType = "hybrid",
            length = 100f,
            width = 100f,
            height = 50f,
            heights = new float[][]
            {
                new float[] { 1f, 2f, 3f },
                new float[] { 4f, 5f, 6f }
            }
        };

        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        string serialized = JsonConvert.SerializeObject(entity, settings);
        var deserialized = JsonConvert.DeserializeObject<JSONTerrainEntity>(serialized, settings);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("terrain-001", deserialized.id);
        Assert.AreEqual("hybrid", deserialized.terrainType);
        Assert.AreEqual(100f, deserialized.length);
        Assert.AreEqual(100f, deserialized.width);
        Assert.AreEqual(50f, deserialized.height);
        Assert.IsNotNull(deserialized.heights);
        Assert.AreEqual(2, deserialized.heights.Length);
        Assert.AreEqual(3, deserialized.heights[0].Length);
        Assert.AreEqual(5f, deserialized.heights[1][1]);
    }

    [Test]
    public void JSONTerrainEntity_WithLayerMasks_SerializesObjectFormat()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTerrainEntity
        {
            id = "terrain-lm",
            terrainType = "heightmap",
            layerMasks = new JSONTerrainEntityLayerMask[]
            {
                new JSONTerrainEntityLayerMask
                {
                    heights = new float[][]
                    {
                        new float[] { 1.0f, 0.8f },
                        new float[] { 0.5f, 0.3f }
                    }
                }
            }
        };

        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        string serialized = JsonConvert.SerializeObject(entity, settings);
        var deserialized = JsonConvert.DeserializeObject<JSONTerrainEntity>(serialized, settings);

        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.layerMasks);
        Assert.AreEqual(1, deserialized.layerMasks.Length);
        Assert.AreEqual(1.0f, deserialized.layerMasks[0].heights[0][0]);
        Assert.AreEqual(0.3f, deserialized.layerMasks[0].heights[1][1]);
    }

    [Test]
    public void JSONTerrainEntityLayerMask_DeserializeDirectArrayFormat_ParsesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        string json = "[[1.0, 0.8, 0.5], [0.9, 0.7, 0.4]]";
        var mask = JsonConvert.DeserializeObject<JSONTerrainEntityLayerMask>(json);

        Assert.IsNotNull(mask);
        Assert.IsNotNull(mask.heights);
        Assert.AreEqual(2, mask.heights.Length);
        Assert.AreEqual(3, mask.heights[0].Length);
        Assert.AreEqual(1.0f, mask.heights[0][0]);
        Assert.AreEqual(0.4f, mask.heights[1][2]);
    }

    [Test]
    public void JSONTerrainEntityLayer_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var layer = new JSONTerrainEntityLayer();

        Assert.IsNotNull(layer.specular);
        Assert.AreEqual(0.2f, layer.specular.r);
        Assert.AreEqual(0.0f, layer.metallic);
        Assert.AreEqual(0.5f, layer.smoothness);
        Assert.AreEqual(1, layer.sizeFactor);
    }

    [Test]
    public void JSONTerrainEntityModification_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var mod = new JSONTerrainEntityModification();

        Assert.AreEqual("build", mod.operation);
        Assert.IsNotNull(mod.position);
        Assert.AreEqual("sphere", mod.brushType);
        Assert.AreEqual(1.0f, mod.size);
        Assert.AreEqual(0, mod.layer);
    }

    [Test]
    public void JSONTerrainEntityModification_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var mod = new JSONTerrainEntityModification
        {
            operation = "dig",
            x = 200f,
            y = 146f,
            z = 408f,
            brushtype = "roundedCube",
            layer = 2,
            brushsize = 1f
        };

        string serialized = JsonConvert.SerializeObject(mod);
        var deserialized = JsonConvert.DeserializeObject<JSONTerrainEntityModification>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("dig", deserialized.operation);
        Assert.AreEqual(200f, deserialized.x);
        Assert.AreEqual(146f, deserialized.y);
        Assert.AreEqual(408f, deserialized.z);
        Assert.AreEqual("roundedCube", deserialized.brushtype);
        Assert.AreEqual(2, deserialized.layer);
    }

    #endregion

    #region JSONLightEntity Tests

    [Test]
    public void JSONLightEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var light = new JSONLightEntity();

        Assert.AreEqual("light", light.type);
        Assert.AreEqual("point", light.lightType);
        Assert.IsNotNull(light.position);
        Assert.IsNotNull(light.rotation);
        Assert.AreEqual(1f, light.rotation.w);
        Assert.IsNotNull(light.color);
        Assert.AreEqual(1f, light.color.r);
        Assert.AreEqual(1f, light.color.g);
        Assert.AreEqual(1f, light.color.b);
        Assert.AreEqual(1f, light.color.a);
        Assert.AreEqual(6500, light.temperature);
        Assert.AreEqual(1.0f, light.intensity);
        Assert.AreEqual(10.0f, light.range);
        Assert.AreEqual(21.8f, light.innerSpotAngle);
        Assert.AreEqual(30.0f, light.outerSpotAngle);
    }

    [Test]
    public void JSONLightEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var light = new JSONLightEntity
        {
            id = "light-001",
            tag = "SpotLight",
            position = new JSONVector3 { x = 0f, y = 10f, z = 0f },
            rotation = new JSONQuaternion { x = 0.7f, y = 0f, z = 0f, w = 0.7f },
            lightType = "spot",
            color = new JSONColor { r = 1f, g = 0.9f, b = 0.8f, a = 1f },
            temperature = 4000,
            intensity = 2.5f,
            range = 20.0f,
            innerSpotAngle = 15.0f,
            outerSpotAngle = 45.0f
        };

        string serialized = JsonConvert.SerializeObject(light);
        var deserialized = JsonConvert.DeserializeObject<JSONLightEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("light-001", deserialized.id);
        Assert.AreEqual("SpotLight", deserialized.tag);
        Assert.AreEqual("spot", deserialized.lightType);
        Assert.AreEqual(10f, deserialized.position.y);
        Assert.AreEqual(4000, deserialized.temperature);
        Assert.AreEqual(2.5f, deserialized.intensity);
        Assert.AreEqual(20.0f, deserialized.range);
        Assert.AreEqual(15.0f, deserialized.innerSpotAngle);
        Assert.AreEqual(45.0f, deserialized.outerSpotAngle);
        Assert.AreEqual(0.9f, deserialized.color.g);
    }

    [Test]
    public void JSONLightEntity_DeserializeFromJsonString_ParsesPointLight()
    {
        LogAssert.ignoreFailingMessages = true;

        string json = @"{
            ""type"": ""light"",
            ""id"": ""light-point-01"",
            ""tag"": ""PointLight"",
            ""position"": { ""x"": 5, ""y"": 3, ""z"": -2 },
            ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
            ""lightType"": ""point"",
            ""color"": { ""r"": 1, ""g"": 1, ""b"": 0.5, ""a"": 1 },
            ""temperature"": 5500,
            ""intensity"": 1.5,
            ""range"": 15.0,
            ""innerSpotAngle"": 21.8,
            ""outerSpotAngle"": 30.0
        }";

        var light = JsonConvert.DeserializeObject<JSONLightEntity>(json);

        Assert.IsNotNull(light);
        Assert.AreEqual("light-point-01", light.id);
        Assert.AreEqual("point", light.lightType);
        Assert.AreEqual(5f, light.position.x);
        Assert.AreEqual(3f, light.position.y);
        Assert.AreEqual(-2f, light.position.z);
        Assert.AreEqual(5500, light.temperature);
        Assert.AreEqual(1.5f, light.intensity);
        Assert.AreEqual(0.5f, light.color.b);
    }

    #endregion

    #region JSONNamedTerrainLayer Tests

    [Test]
    public void JSONNamedTerrainLayer_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var layer = new JSONNamedTerrainLayer();

        Assert.AreEqual(0, layer.layer);
        Assert.IsNotNull(layer.specular);
        Assert.AreEqual(0.2f, layer.specular.r);
        Assert.AreEqual(0.0f, layer.metallic);
        Assert.AreEqual(0.5f, layer.smoothness);
        Assert.AreEqual(1, layer.sizeFactor);
    }

    [Test]
    public void JSONNamedTerrainLayer_ToStandardLayer_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var named = new JSONNamedTerrainLayer
        {
            layer = 1,
            color_texture = "Grass_001_BaseColor.png",
            normal_texture = "Grass_001_Normal.png",
            mask_texture = "Grass_001_Mask.png",
            metallic = 0.1f,
            smoothness = 0.7f,
            sizeFactor = 2
        };

        var standard = named.ToStandardLayer();

        Assert.IsNotNull(standard);
        Assert.AreEqual("Grass_001_BaseColor.png", standard.diffuseTexture);
        Assert.AreEqual("Grass_001_Normal.png", standard.normalTexture);
        Assert.AreEqual("Grass_001_Mask.png", standard.maskTexture);
        Assert.AreEqual(0.1f, standard.metallic);
        Assert.AreEqual(0.7f, standard.smoothness);
        Assert.AreEqual(2, standard.sizeFactor);
    }

    [Test]
    public void JSONNamedTerrainLayer_RoundTripSerialization_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var original = new JSONNamedTerrainLayer
        {
            layer = 0,
            color_texture = "Rock_047_BaseColor.png",
            normal_texture = "Rock_047_Normal.png",
            metallic = 0.0f,
            smoothness = 0.5f,
            sizeFactor = 1
        };

        string serialized = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<JSONNamedTerrainLayer>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(0, deserialized.layer);
        Assert.AreEqual("Rock_047_BaseColor.png", deserialized.color_texture);
        Assert.AreEqual("Rock_047_Normal.png", deserialized.normal_texture);
        Assert.AreEqual(0.5f, deserialized.smoothness);
    }

    #endregion

    #region JSONBaseGround and Cross-Type Tests

    [Test]
    public void JSONTerrainBaseGround_RoundTripSerialization_PreservesHeightsAndLayers()
    {
        LogAssert.ignoreFailingMessages = true;

        var baseGround = new JSONTerrainBaseGround
        {
            heights = new float[][]
            {
                new float[] { 0f, 1f, 2f },
                new float[] { 3f, 4f, 5f }
            },
            layers = new float[][][]
            {
                new float[][]
                {
                    new float[] { 1f, 1f, 1f },
                    new float[] { 1f, 1f, 1f }
                }
            }
        };

        string serialized = JsonConvert.SerializeObject(baseGround);
        var deserialized = JsonConvert.DeserializeObject<JSONTerrainBaseGround>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.heights);
        Assert.AreEqual(2, deserialized.heights.Length);
        Assert.AreEqual(4f, deserialized.heights[1][1]);
        Assert.IsNotNull(deserialized.layers);
        Assert.AreEqual(1, deserialized.layers.Length);
    }

    [Test]
    public void JSONMeshEntityCollection_EmptyCollection_SerializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var collection = new JSONMeshEntityCollection();

        string serialized = JsonConvert.SerializeObject(collection);
        var deserialized = JsonConvert.DeserializeObject<JSONMeshEntityCollection>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.meshEntities);
        Assert.AreEqual(0, deserialized.meshEntities.Length);
    }

    #endregion

    #region JSONAirplaneEntity Tests

    [Test]
    public void JSONAirplaneEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAirplaneEntity();

        Assert.AreEqual(1000f, entity.mass);
        Assert.AreEqual(0f, entity.throttle);
        Assert.AreEqual(0f, entity.pitch);
        Assert.AreEqual(0f, entity.roll);
        Assert.AreEqual(0f, entity.yaw);
        Assert.IsTrue(entity.checkForUpdateIfCached);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
        Assert.AreEqual(1f, entity.scale.y);
        Assert.AreEqual(1f, entity.scale.z);
        Assert.IsFalse(entity.isSize);
    }

    [Test]
    public void JSONAirplaneEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAirplaneEntity
        {
            id = "airplane-001",
            tag = "TestPlane",
            position = new JSONVector3 { x = 0f, y = 100f, z = 0f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 2f, y = 2f, z = 2f },
            meshObject = "airplane.glb",
            meshResources = new string[] { "engine.png" },
            mass = 5000f,
            throttle = 0.8f,
            pitch = 0.1f,
            roll = -0.2f,
            yaw = 0.05f,
            checkForUpdateIfCached = false
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONAirplaneEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("airplane-001", deserialized.id);
        Assert.AreEqual("TestPlane", deserialized.tag);
        Assert.AreEqual(100f, deserialized.position.y);
        Assert.AreEqual("airplane.glb", deserialized.meshObject);
        Assert.AreEqual(5000f, deserialized.mass);
        Assert.AreEqual(0.8f, deserialized.throttle);
        Assert.AreEqual(0.1f, deserialized.pitch);
        Assert.AreEqual(-0.2f, deserialized.roll);
        Assert.AreEqual(0.05f, deserialized.yaw);
        Assert.IsFalse(deserialized.checkForUpdateIfCached);
        Assert.AreEqual(1, deserialized.meshResources.Length);
    }

    #endregion

    #region JSONAutomobileEntity Tests

    [Test]
    public void JSONAutomobileEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAutomobileEntity();

        Assert.AreEqual(1500f, entity.mass);
        Assert.AreEqual("Default", entity.automobileType);
        Assert.AreEqual(0f, entity.throttle);
        Assert.AreEqual(0f, entity.steer);
        Assert.AreEqual(0f, entity.brake);
        Assert.AreEqual(0f, entity.handBrake);
        Assert.IsFalse(entity.horn);
        Assert.AreEqual(0, entity.gear);
        Assert.IsFalse(entity.engineStartStop);
        Assert.IsTrue(entity.checkForUpdateIfCached);
        Assert.IsFalse(entity.isSize);
    }

    [Test]
    public void JSONAutomobileEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAutomobileEntity
        {
            id = "car-001",
            tag = "TestCar",
            position = new JSONVector3 { x = 10f, y = 0f, z = 20f },
            meshObject = "sedan.glb",
            mass = 2000f,
            automobileType = "Sport",
            throttle = 0.5f,
            steer = 0.3f,
            brake = 0.1f,
            gear = 3,
            wheels = new JSONAutomobileEntityWheel[]
            {
                new JSONAutomobileEntityWheel("FrontLeft", 0.4f),
                new JSONAutomobileEntityWheel("FrontRight", 0.4f)
            }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONAutomobileEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("car-001", deserialized.id);
        Assert.AreEqual(2000f, deserialized.mass);
        Assert.AreEqual("Sport", deserialized.automobileType);
        Assert.AreEqual(0.5f, deserialized.throttle);
        Assert.AreEqual(3, deserialized.gear);
        Assert.AreEqual(2, deserialized.wheels.Length);
        Assert.AreEqual("FrontLeft", deserialized.wheels[0].wheelSubMesh);
        Assert.AreEqual(0.4f, deserialized.wheels[0].wheelRadius);
    }

    [Test]
    public void JSONAutomobileEntityWheel_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var wheel = new JSONAutomobileEntityWheel();

        Assert.AreEqual(0.5f, wheel.wheelRadius);
        Assert.IsNull(wheel.wheelSubMesh);
    }

    [Test]
    public void JSONAutomobileEntityWheel_ParameterizedConstructor_SetsValues()
    {
        LogAssert.ignoreFailingMessages = true;

        var wheel = new JSONAutomobileEntityWheel("RearLeft", 0.35f);

        Assert.AreEqual("RearLeft", wheel.wheelSubMesh);
        Assert.AreEqual(0.35f, wheel.wheelRadius);
    }

    #endregion

    #region JSONCanvasEntity Tests

    [Test]
    public void JSONCanvasEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONCanvasEntity();

        Assert.AreEqual("screen", entity.canvasType);
        Assert.IsFalse(entity.isSize);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
    }

    [Test]
    public void JSONCanvasEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONCanvasEntity
        {
            id = "canvas-001",
            tag = "UICanvas",
            canvasType = "world",
            position = new JSONVector3 { x = 0f, y = 2f, z = 5f },
            rotation = new JSONQuaternion { x = 0f, y = 0f, z = 0f, w = 1f },
            scale = new JSONVector3 { x = 3f, y = 2f, z = 1f }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONCanvasEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("canvas-001", deserialized.id);
        Assert.AreEqual("world", deserialized.canvasType);
        Assert.AreEqual(2f, deserialized.position.y);
    }

    #endregion

    #region JSONButtonEntity Tests

    [Test]
    public void JSONButtonEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONButtonEntity();

        Assert.AreEqual("Button", entity.text);
        Assert.AreEqual(14, entity.fontSize);
        Assert.IsNotNull(entity.positionPercent);
        Assert.AreEqual(0.5f, entity.positionPercent.x);
        Assert.AreEqual(0.5f, entity.positionPercent.y);
        Assert.IsNotNull(entity.sizePercent);
        Assert.AreEqual(0.2f, entity.sizePercent.x);
        Assert.AreEqual(0.1f, entity.sizePercent.y);
        Assert.IsNotNull(entity.textColor);
        Assert.AreEqual(0f, entity.textColor.r);
        Assert.IsNotNull(entity.backgroundColor);
        Assert.AreEqual(1f, entity.backgroundColor.r);
    }

    [Test]
    public void JSONButtonEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONButtonEntity
        {
            id = "btn-001",
            tag = "SubmitBtn",
            text = "Submit",
            fontSize = 18,
            onClick = "onSubmitClicked()",
            textColor = new JSONColor { r = 1f, g = 1f, b = 1f, a = 1f },
            backgroundColor = new JSONColor { r = 0f, g = 0.5f, b = 1f, a = 1f }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONButtonEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("Submit", deserialized.text);
        Assert.AreEqual(18, deserialized.fontSize);
        Assert.AreEqual("onSubmitClicked()", deserialized.onClick);
        Assert.AreEqual(1f, deserialized.textColor.r);
        Assert.AreEqual(0.5f, deserialized.backgroundColor.g);
    }

    #endregion

    #region JSONTextEntity Tests

    [Test]
    public void JSONTextEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTextEntity();

        Assert.AreEqual("Text", entity.text);
        Assert.AreEqual(14, entity.fontSize);
        Assert.AreEqual("normal", entity.fontStyle);
        Assert.AreEqual("center", entity.alignment);
        Assert.IsTrue(entity.wordWrap);
        Assert.IsNotNull(entity.color);
        Assert.AreEqual(0f, entity.color.r);
    }

    [Test]
    public void JSONTextEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTextEntity
        {
            id = "text-001",
            text = "Hello World",
            fontSize = 24,
            fontStyle = "bold",
            alignment = "left",
            wordWrap = false,
            color = new JSONColor { r = 0f, g = 1f, b = 0f, a = 1f }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONTextEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("Hello World", deserialized.text);
        Assert.AreEqual(24, deserialized.fontSize);
        Assert.AreEqual("bold", deserialized.fontStyle);
        Assert.AreEqual("left", deserialized.alignment);
        Assert.IsFalse(deserialized.wordWrap);
    }

    #endregion

    #region JSONInputEntity Tests

    [Test]
    public void JSONInputEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONInputEntity();

        Assert.AreEqual("Enter text...", entity.placeholder);
        Assert.AreEqual("", entity.text);
        Assert.AreEqual("text", entity.inputType);
        Assert.AreEqual(0, entity.characterLimit);
        Assert.IsFalse(entity.readOnly);
    }

    [Test]
    public void JSONInputEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONInputEntity
        {
            id = "input-001",
            placeholder = "Username",
            text = "admin",
            inputType = "password",
            characterLimit = 32,
            readOnly = true,
            onValueChanged = "onInputChange(?)",
            onEndEdit = "onInputDone(?)"
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONInputEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("Username", deserialized.placeholder);
        Assert.AreEqual("admin", deserialized.text);
        Assert.AreEqual("password", deserialized.inputType);
        Assert.AreEqual(32, deserialized.characterLimit);
        Assert.IsTrue(deserialized.readOnly);
        Assert.AreEqual("onInputChange(?)", deserialized.onValueChanged);
    }

    #endregion

    #region JSONDropdownEntity Tests

    [Test]
    public void JSONDropdownEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONDropdownEntity();

        Assert.AreEqual(0, entity.selectedIndex);
        Assert.AreEqual("Select...", entity.captionText);
        Assert.IsNotNull(entity.options);
        Assert.AreEqual(3, entity.options.Length);
        Assert.AreEqual("Option 1", entity.options[0]);
        Assert.AreEqual("Option 2", entity.options[1]);
        Assert.AreEqual("Option 3", entity.options[2]);
    }

    [Test]
    public void JSONDropdownEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONDropdownEntity
        {
            id = "dd-001",
            options = new string[] { "Red", "Green", "Blue" },
            selectedIndex = 1,
            captionText = "Pick a color",
            onChange = "onColorSelected(?)"
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONDropdownEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(3, deserialized.options.Length);
        Assert.AreEqual("Green", deserialized.options[1]);
        Assert.AreEqual(1, deserialized.selectedIndex);
        Assert.AreEqual("Pick a color", deserialized.captionText);
    }

    #endregion

    #region JSONImageEntity Tests

    [Test]
    public void JSONImageEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONImageEntity();

        Assert.AreEqual("image", entity.type);
        Assert.AreEqual("", entity.imageFile);
        Assert.IsFalse(entity.stretchToParent);
        Assert.AreEqual("center", entity.alignment);
    }

    [Test]
    public void JSONImageEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONImageEntity
        {
            id = "img-001",
            tag = "Logo",
            imageFile = "logo.png",
            stretchToParent = true,
            alignment = "left",
            positionPercent = new JSONVector2(0.1f, 0.1f),
            sizePercent = new JSONVector2(0.5f, 0.5f)
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONImageEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("logo.png", deserialized.imageFile);
        Assert.IsTrue(deserialized.stretchToParent);
        Assert.AreEqual("left", deserialized.alignment);
    }

    #endregion

    #region JSONHTMLEntity Tests

    [Test]
    public void JSONHTMLEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONHTMLEntity();

        Assert.AreEqual("html", entity.type);
        Assert.IsFalse(entity.isSize);
        Assert.IsFalse(entity.isCanvasElement);
    }

    [Test]
    public void JSONHTMLEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONHTMLEntity
        {
            id = "html-001",
            tag = "WebPanel",
            url = "https://example.com",
            html = "<h1>Hello</h1>",
            isCanvasElement = true,
            onMessage = "onWebMessage(?)",
            positionPercent = new JSONVector2(0.5f, 0.5f),
            sizePercent = new JSONVector2(0.8f, 0.6f)
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONHTMLEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("https://example.com", deserialized.url);
        Assert.AreEqual("<h1>Hello</h1>", deserialized.html);
        Assert.IsTrue(deserialized.isCanvasElement);
        Assert.AreEqual("onWebMessage(?)", deserialized.onMessage);
    }

    #endregion

    #region JSONVoxelEntity Tests

    [Test]
    public void JSONVoxelEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONVoxelEntity();

        Assert.AreEqual("voxel", entity.type);
        Assert.IsNotNull(entity.position);
        Assert.IsNotNull(entity.rotation);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
    }

    [Test]
    public void JSONVoxelBlockSubType_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var sub = new JSONVoxelBlockSubType();

        Assert.IsFalse(sub.invisible);
        Assert.AreEqual("", sub.topTexture);
        Assert.AreEqual("", sub.bottomTexture);
        Assert.AreEqual("", sub.leftTexture);
        Assert.AreEqual("", sub.rightTexture);
        Assert.AreEqual("", sub.frontTexture);
        Assert.AreEqual("", sub.backTexture);
    }

    [Test]
    public void JSONVoxelEntity_RoundTripSerialization_PreservesBlockData()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONVoxelEntity
        {
            id = "voxel-001",
            blockInfos = new JSONVoxelBlockInfo[]
            {
                new JSONVoxelBlockInfo
                {
                    id = 1,
                    subTypes = new JSONVoxelBlockSubType[]
                    {
                        new JSONVoxelBlockSubType
                        {
                            id = 0,
                            topTexture = "grass_top.png",
                            bottomTexture = "dirt.png"
                        }
                    }
                }
            },
            blocks = new JSONVoxelBlock[]
            {
                new JSONVoxelBlock { x = 0, y = 0, z = 0, type = 1, subType = 0 },
                new JSONVoxelBlock { x = 1, y = 0, z = 0, type = 1, subType = 0 }
            }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONVoxelEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1, deserialized.blockInfos.Length);
        Assert.AreEqual("grass_top.png", deserialized.blockInfos[0].subTypes[0].topTexture);
        Assert.AreEqual(2, deserialized.blocks.Length);
        Assert.AreEqual(1, deserialized.blocks[1].x);
    }

    #endregion

    #region JSONWaterEntity Tests

    [Test]
    public void JSONWaterEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONWaterEntity();

        Assert.AreEqual("water", entity.type);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(10f, entity.scale.x);
        Assert.AreEqual(1f, entity.scale.y);
        Assert.AreEqual(10f, entity.scale.z);
        Assert.IsNotNull(entity.shallowColor);
        Assert.AreEqual(0.4f, entity.shallowColor.r);
        Assert.IsNotNull(entity.deepColor);
        Assert.AreEqual(0.1f, entity.deepColor.r);
        Assert.AreEqual(0.5f, entity.deepStart);
        Assert.AreEqual(10.0f, entity.deepEnd);
        Assert.AreEqual(32.0f, entity.distortion);
        Assert.AreEqual(0.8f, entity.smoothness);
        Assert.AreEqual(0.7f, entity.intensity);
        Assert.AreEqual(4.0f, entity.numWaves);
        Assert.AreEqual(0.3f, entity.waveAmplitude);
        Assert.AreEqual(0.5f, entity.waveSteepness);
        Assert.AreEqual(1.0f, entity.waveSpeed);
        Assert.AreEqual(10.0f, entity.waveLength);
        Assert.AreEqual(1.0f, entity.waveScale);
    }

    [Test]
    public void JSONWaterEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONWaterEntity
        {
            id = "water-001",
            tag = "Ocean",
            scale = new JSONVector3 { x = 500f, y = 1f, z = 500f },
            shallowColor = new JSONColor { r = 0.2f, g = 0.7f, b = 0.9f, a = 0.7f },
            deepColor = new JSONColor { r = 0.05f, g = 0.2f, b = 0.5f, a = 1f },
            waveAmplitude = 0.8f,
            waveSpeed = 2.0f,
            numWaves = 8f
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONWaterEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("water-001", deserialized.id);
        Assert.AreEqual(500f, deserialized.scale.x);
        Assert.AreEqual(0.7f, deserialized.shallowColor.a);
        Assert.AreEqual(0.8f, deserialized.waveAmplitude);
        Assert.AreEqual(2.0f, deserialized.waveSpeed);
        Assert.AreEqual(8f, deserialized.numWaves);
    }

    #endregion

    #region JSONWaterBlockerEntity Tests

    [Test]
    public void JSONWaterBlockerEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONWaterBlockerEntity();

        Assert.AreEqual("waterBlocker", entity.type);
        Assert.IsNotNull(entity.position);
        Assert.IsNotNull(entity.rotation);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
    }

    [Test]
    public void JSONWaterBlockerEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONWaterBlockerEntity
        {
            id = "blocker-001",
            tag = "Dam",
            position = new JSONVector3 { x = 50f, y = 0f, z = 100f },
            scale = new JSONVector3 { x = 20f, y = 5f, z = 2f }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONWaterBlockerEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("blocker-001", deserialized.id);
        Assert.AreEqual("Dam", deserialized.tag);
        Assert.AreEqual(50f, deserialized.position.x);
        Assert.AreEqual(20f, deserialized.scale.x);
    }

    #endregion

    #region JSONAudioEntity Tests

    [Test]
    public void JSONAudioEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAudioEntity();

        Assert.AreEqual("audio", entity.type);
        Assert.IsFalse(entity.loop);
        Assert.AreEqual(128, entity.priority);
        Assert.AreEqual(1.0f, entity.volume);
        Assert.AreEqual(1.0f, entity.pitch);
        Assert.AreEqual(0.0f, entity.stereoPan);
        Assert.IsFalse(entity.playOnLoad);
    }

    [Test]
    public void JSONAudioEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONAudioEntity
        {
            id = "audio-001",
            tag = "BGM",
            audioFile = "background.ogg",
            loop = true,
            priority = 64,
            volume = 0.7f,
            pitch = 1.2f,
            stereoPan = -0.5f,
            playOnLoad = true
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONAudioEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("audio-001", deserialized.id);
        Assert.AreEqual("background.ogg", deserialized.audioFile);
        Assert.IsTrue(deserialized.loop);
        Assert.AreEqual(64, deserialized.priority);
        Assert.AreEqual(0.7f, deserialized.volume);
        Assert.AreEqual(1.2f, deserialized.pitch);
        Assert.AreEqual(-0.5f, deserialized.stereoPan);
        Assert.IsTrue(deserialized.playOnLoad);
    }

    #endregion

    #region JSONCharacterEntity Tests

    [Test]
    public void JSONCharacterEntity_DefaultConstructor_HasCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONCharacterEntity();

        Assert.AreEqual("character", entity.type);
        Assert.IsFalse(entity.isSize);
        Assert.IsTrue(entity.fixHeight);
        Assert.IsTrue(entity.checkForUpdateIfCached);
        Assert.IsNotNull(entity.scale);
        Assert.AreEqual(1f, entity.scale.x);
        Assert.IsNotNull(entity.avatarLabelOffset);
        Assert.AreEqual(2f, entity.avatarLabelOffset.y);
    }

    [Test]
    public void JSONCharacterEntity_RoundTripSerialization_PreservesAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONCharacterEntity
        {
            id = "char-001",
            tag = "Player",
            position = new JSONVector3 { x = 5f, y = 0f, z = 10f },
            rotation = new JSONQuaternion { x = 0f, y = 0.7f, z = 0f, w = 0.7f },
            scale = new JSONVector3 { x = 1f, y = 1f, z = 1f },
            meshObject = "avatar.glb",
            meshResources = new string[] { "skin.png", "hair.png" },
            meshOffset = new JSONVector3 { x = 0f, y = -0.5f, z = 0f },
            avatarLabelOffset = new JSONVector3 { x = 0f, y = 2.5f, z = 0f },
            fixHeight = false,
            checkForUpdateIfCached = false
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONCharacterEntity>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("char-001", deserialized.id);
        Assert.AreEqual("avatar.glb", deserialized.meshObject);
        Assert.AreEqual(2, deserialized.meshResources.Length);
        Assert.AreEqual(-0.5f, deserialized.meshOffset.y);
        Assert.AreEqual(2.5f, deserialized.avatarLabelOffset.y);
        Assert.IsFalse(deserialized.fixHeight);
    }

    #endregion

    #region JSONEntityHandler MonoBehaviour Tests

    [UnityTest]
    public IEnumerator JSONEntityHandler_Initialize_Terminate_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        handler.Terminate();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_IsReady_FalseWithoutWorld()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        Assert.IsFalse(handler.IsReady());

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_GetHandlerStats_ReturnsNonNullDictionary()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        var stats = handler.GetHandlerStats();

        Assert.IsNotNull(stats);
        Assert.IsTrue(stats.ContainsKey("IsInitialized"));
        Assert.IsTrue(stats.ContainsKey("IsReady"));

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ValidateJSONFormat_ValidJson_ReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        Assert.IsTrue(handler.ValidateJSONFormat("{\"id\":\"test\",\"tag\":\"Test\"}"));

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ValidateJSONFormat_InvalidJson_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        Assert.IsFalse(handler.ValidateJSONFormat("{invalid json}"));

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ValidateJSONFormat_EmptyString_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        Assert.IsFalse(handler.ValidateJSONFormat(""));

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ValidateJSONFormat_NullString_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        Assert.IsFalse(handler.ValidateJSONFormat(null));

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseEntityFromJSON_ValidJson_ReturnsEntity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        string json = @"{
            ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
            ""tag"": ""TestParse"",
            ""position"": { ""x"": 1, ""y"": 2, ""z"": 3 },
            ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
            ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
            ""children"": []
        }";

        var entity = handler.ParseEntityFromJSON(json);
        Assert.IsNotNull(entity);
        Assert.AreEqual("TestParse", entity.tag);
        Assert.AreEqual(2f, entity.position.y);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseEntityFromJSON_InvalidJson_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        var entity = handler.ParseEntityFromJSON("{broken json}");
        Assert.IsNull(entity);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseEntityFromJSON_NullJson_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        var entity = handler.ParseEntityFromJSON(null);
        Assert.IsNull(entity);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseMeshEntityFromJSON_ValidJson_ReturnsEntity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        string json = @"{
            ""id"": ""550e8400-e29b-41d4-a716-446655440002"",
            ""meshType"": ""primitive"",
            ""meshSource"": ""sphere"",
            ""position"": { ""x"": 0, ""y"": 5, ""z"": 0 },
            ""children"": []
        }";

        var entity = handler.ParseMeshEntityFromJSON(json);
        Assert.IsNotNull(entity);
        Assert.AreEqual("primitive", entity.meshType);
        Assert.AreEqual("sphere", entity.meshSource);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseTerrainEntityFromJSON_ValidJson_ReturnsEntity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        string json = @"{
            ""id"": ""terrain-parse-01"",
            ""terrainType"": ""heightmap"",
            ""length"": 100,
            ""width"": 100,
            ""height"": 50,
            ""heights"": [[0,1],[2,3]]
        }";

        var entity = handler.ParseTerrainEntityFromJSON(json);
        Assert.IsNotNull(entity);
        Assert.AreEqual("heightmap", entity.terrainType);
        Assert.AreEqual(100f, entity.length);
        Assert.AreEqual(2, entity.heights.Length);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseCharacterEntityFromJSON_ValidJson_ReturnsEntity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        string json = @"{
            ""type"": ""character"",
            ""id"": ""550e8400-e29b-41d4-a716-446655440003"",
            ""tag"": ""TestChar"",
            ""meshObject"": ""avatar.glb"",
            ""fixHeight"": true
        }";

        var entity = handler.ParseCharacterEntityFromJSON(json);
        Assert.IsNotNull(entity);
        Assert.AreEqual("avatar.glb", entity.meshObject);
        Assert.IsTrue(entity.fixHeight);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator JSONEntityHandler_ParseMeshEntityCollectionFromJSON_ValidJson_ReturnsCollection()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("JSONHandler");
        JSONEntityHandler handler = go.AddComponent<JSONEntityHandler>();
        yield return null;

        handler.Initialize();
        string json = @"{""mesh-entities"":[
            {""id"":""550e8400-e29b-41d4-a716-446655440004"",""meshType"":""primitive"",""meshSource"":""cube"",""children"":[]},
            {""id"":""550e8400-e29b-41d4-a716-446655440005"",""meshType"":""primitive"",""meshSource"":""sphere"",""children"":[]}
        ]}";

        var collection = handler.ParseMeshEntityCollectionFromJSON(json);
        Assert.IsNotNull(collection);
        Assert.AreEqual(2, collection.meshEntities.Length);
        Assert.AreEqual("cube", collection.meshEntities[0].meshSource);
        Assert.AreEqual("sphere", collection.meshEntities[1].meshSource);

        handler.Terminate();
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region Error and Edge Case Tests

    [Test]
    public void JSONVector3_NegativeValues_PreservedInRoundTrip()
    {
        LogAssert.ignoreFailingMessages = true;

        var v = new JSONVector3 { x = -100f, y = -0.001f, z = -999.9f };
        string serialized = JsonConvert.SerializeObject(v);
        var deserialized = JsonConvert.DeserializeObject<JSONVector3>(serialized);

        Assert.AreEqual(-100f, deserialized.x);
        Assert.AreEqual(-0.001f, deserialized.y, 0.0001f);
        Assert.AreEqual(-999.9f, deserialized.z, 0.01f);
    }

    [Test]
    public void JSONColor_ClampedValues_PreservedInRoundTrip()
    {
        LogAssert.ignoreFailingMessages = true;

        var c = new JSONColor { r = 0f, g = 0f, b = 0f, a = 0f };
        string serialized = JsonConvert.SerializeObject(c);
        var deserialized = JsonConvert.DeserializeObject<JSONColor>(serialized);

        Assert.AreEqual(0f, deserialized.r);
        Assert.AreEqual(0f, deserialized.a);
    }

    [Test]
    public void JSONContainerEntity_DeeplyNestedChildren_SerializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var root = new JSONContainerEntity { id = "root", tag = "Root" };
        var level1 = new JSONContainerEntity { id = "l1", tag = "L1", parentId = "root" };
        var level2 = new JSONContainerEntity { id = "l2", tag = "L2", parentId = "l1" };
        level1.children.Add(level2);
        root.children.Add(level1);

        string serialized = JsonConvert.SerializeObject(root);
        var deserialized = JsonConvert.DeserializeObject<JSONContainerEntity>(serialized);

        Assert.AreEqual(1, deserialized.children.Count);
        Assert.AreEqual(1, deserialized.children[0].children.Count);
        Assert.AreEqual("l2", deserialized.children[0].children[0].id);
    }

    [Test]
    public void JSONMeshEntity_GltfType_SerializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONMeshEntity
        {
            id = "gltf-mesh",
            meshType = "gltf",
            meshSource = "model.glb",
            meshResources = new string[] { "texture1.png", "texture2.png", "normal.png" }
        };

        string serialized = JsonConvert.SerializeObject(entity);
        var deserialized = JsonConvert.DeserializeObject<JSONMeshEntity>(serialized);

        Assert.AreEqual("gltf", deserialized.meshType);
        Assert.AreEqual("model.glb", deserialized.meshSource);
        Assert.AreEqual(3, deserialized.meshResources.Length);
    }

    [Test]
    public void JSONTerrainEntity_HybridType_SerializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var entity = new JSONTerrainEntity
        {
            terrainType = "hybrid",
            stitchTerrains = true,
            length = 256f,
            width = 256f,
            height = 100f
        };

        string serialized = JsonConvert.SerializeObject(entity);
        Assert.IsTrue(serialized.Contains("\"hybrid\""));
        Assert.IsTrue(serialized.Contains("true"));
    }

    #endregion
}
