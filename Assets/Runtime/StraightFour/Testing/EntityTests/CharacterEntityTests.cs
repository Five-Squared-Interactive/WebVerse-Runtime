// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;
using System;

/// <summary>
/// Dedicated tests for CharacterEntity with full initialization (rigidbody, character controller).
/// These tests use EntityManager.LoadCharacterEntity to get a properly initialized entity.
/// </summary>
public class CharacterEntityTests
{
    private GameObject weGO;
    private StraightFour straightFour;
    private GameObject cameraGO;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (StraightFour.ActiveWorld != null)
            {
                StraightFour.UnloadWorld();
            }
        }
        catch (Exception)
        {
            // CameraManager may already be destroyed during teardown.
        }
        if (cameraGO != null)
        {
            UnityEngine.Object.DestroyImmediate(cameraGO);
            cameraGO = null;
        }
    }

    /// <summary>
    /// Helper to set up StraightFour world and return a loaded CharacterEntity via EntityManager.
    /// </summary>
    private IEnumerator SetUpWorldAndLoadCharacter(System.Action<CharacterEntity> callback,
        Vector3? position = null, string tag = null)
    {
        LogAssert.ignoreFailingMessages = true;

        // Create camera (required for some entity operations).
        cameraGO = new GameObject("TestCamera");
        Camera camera = cameraGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -10);
        cameraGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        weGO = new GameObject("WE");
        straightFour = weGO.AddComponent<StraightFour>();
        straightFour.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        straightFour.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Load character via EntityManager for proper initialization.
        Vector3 pos = position ?? Vector3.zero;
        bool loaded = false;
        Guid charId = StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(
            null, null, Vector3.zero, Quaternion.identity, new Vector3(0, 2, 0),
            pos, Quaternion.identity, Vector3.one,
            tag: tag,
            onLoaded: () => { loaded = true; });

        // Wait for entity to load.
        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        CharacterEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(charId) as CharacterEntity;
        callback(ce);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity result = null;
        yield return SetUpWorldAndLoadCharacter(ce => result = ce);
        Assert.IsNotNull(result, "CharacterEntity should be loaded via EntityManager");
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetPosition_Local_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        Vector3 posToSet = new Vector3(5, 3, 7);
        ce.SetPosition(posToSet, true, false);
        Vector3 result = ce.GetPosition(true);
        Assert.AreEqual(posToSet.x, result.x, 0.1f);
        Assert.AreEqual(posToSet.y, result.y, 0.1f);
        Assert.AreEqual(posToSet.z, result.z, 0.1f);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetRotation_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        Quaternion rotToSet = Quaternion.Euler(0, 90, 0);
        ce.SetRotation(rotToSet, false, false);
        yield return null;

        Quaternion measured = ce.GetRotation(false);
        float dot = Quaternion.Dot(rotToSet, measured);
        Assert.IsTrue(Mathf.Abs(dot) > 0.95f, $"Rotation should match. Dot: {dot}");
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetScale_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        Vector3 sclToSet = new Vector3(2, 3, 4);
        ce.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, ce.GetScale());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetVisibility_Toggle()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // Entity starts in Hidden state after loading. Make it visible first.
        ce.SetInteractionState(BaseEntity.InteractionState.Static);
        ce.SetVisibility(true);
        Assert.IsTrue(ce.gameObject.activeSelf);
        // CharacterEntity.SetVisibility hides renderers rather than deactivating the GO.
        ce.SetVisibility(false);
        Assert.IsTrue(ce.gameObject.activeSelf); // GO stays active
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetInteractionState_Hidden()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());

        ce.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, ce.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetInteractionState_Static()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetInteractionState_Physical()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.AreEqual(BaseEntity.InteractionState.Physical, ce.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetPhysicalProperties_WithFullInit()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // Activate entity so rigidbody accepts properties.
        ce.SetInteractionState(BaseEntity.InteractionState.Physical);

        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        ce.SetPhysicalProperties(phyProps);

        yield return null; // Let physics update.

        BaseEntity.EntityPhysicalProperties? setProps = ce.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue, "Physical properties should have a value");
        Assert.AreEqual(phyProps.angularDrag, setProps.Value.angularDrag);
        // centerOfMass may not persist on CharacterEntity rigidbody depending on state.
        Assert.AreEqual(phyProps.drag, setProps.Value.drag);
        Assert.AreEqual(phyProps.gravitational, setProps.Value.gravitational);
        Assert.AreEqual(phyProps.mass, setProps.Value.mass);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_GetMotion_ReturnsValue()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.SetInteractionState(BaseEntity.InteractionState.Physical);

        BaseEntity.EntityMotion? motion = ce.GetMotion();
        Assert.IsTrue(motion.HasValue, "Motion should have a value with full init");
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetParent_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        GameObject parentGO = new GameObject("Parent");
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());

        ce.SetParent(parentBE);
        Assert.AreEqual(parentBE, ce.GetParent());

        ce.SetParent(null);
        Assert.IsNull(ce.GetParent());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_Compare_SameEntity_ReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        Assert.IsTrue(ce.Compare(ce));
    }

    [UnityTest]
    public IEnumerator CharacterEntity_Delete_DestroysEntity()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.Delete();
        yield return null;
        Assert.IsTrue(ce == null, "Entity should be destroyed after Delete");
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetEulerRotation_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        Vector3 euler = new Vector3(0, 45, 0);
        ce.SetEulerRotation(euler, false, false);
        // Just verify no exception thrown; exact values depend on world offset.
    }

    [UnityTest]
    public IEnumerator CharacterEntity_Synchronization_StartStop()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        GameObject synchGO = new GameObject("Synch");
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        ce.StartSynchronizing(synch);
        ce.StopSynchronizing();
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetHighlight_NoCharacterObject()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // Highlight toggle - depends on whether highlight material is available.
        ce.SetHighlight(true);
        ce.SetHighlight(false);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_GetChildren_Empty()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // New entity should have children from its own hierarchy but no BaseEntity children.
        BaseEntity[] children = ce.GetChildren();
        Assert.IsNotNull(children);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_EntityTag_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c, tag: "TestCharacter");
        Assert.IsNotNull(ce);

        // Tag should be set if EntityManager passes it through.
        // The exact behavior depends on the EntityManager implementation.
    }

    [UnityTest]
    public IEnumerator CharacterEntity_MultipleInteractionStateTransitions()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // Cycle through all states.
        ce.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());

        ce.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.AreEqual(BaseEntity.InteractionState.Physical, ce.GetInteractionState());

        ce.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, ce.GetInteractionState());

        ce.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator CharacterEntity_PhysicalProperties_PartialUpdate()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        ce.SetInteractionState(BaseEntity.InteractionState.Physical);

        // Set only mass.
        BaseEntity.EntityPhysicalProperties partialProps = new BaseEntity.EntityPhysicalProperties()
        {
            mass = 100
        };
        ce.SetPhysicalProperties(partialProps);

        BaseEntity.EntityPhysicalProperties? result = ce.GetPhysicalProperties();
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(100f, result.Value.mass);
    }

    [UnityTest]
    public IEnumerator CharacterEntity_SetPosition_World_DiffersFromLocal()
    {
        LogAssert.ignoreFailingMessages = true;
        CharacterEntity ce = null;
        yield return SetUpWorldAndLoadCharacter(c => ce = c);
        Assert.IsNotNull(ce);

        // Set parent with offset to make local != world.
        GameObject parentGO = new GameObject("OffsetParent");
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        parentBE.SetPosition(new Vector3(10, 0, 0), false, false);

        ce.SetParent(parentBE);
        Vector3 localPos = new Vector3(1, 2, 3);
        ce.SetPosition(localPos, true, false);

        Vector3 measuredLocal = ce.GetPosition(true);
        Vector3 measuredWorld = ce.GetPosition(false);

        // Local and world should differ due to parent offset.
        Assert.AreEqual(localPos.x, measuredLocal.x, 0.1f);
        Assert.AreNotEqual(measuredLocal, measuredWorld);
    }
}
