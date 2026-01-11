// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using System;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;
using FiveSQD.StraightFour.Entity.Terrain;
using System.Collections.Generic;

public class EntityTests
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
    public IEnumerator EntityTests_BaseEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        BaseEntity be = go.AddComponent<BaseEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        be.Initialize(entityID);
        Assert.AreEqual(entityID, be.id);
        Assert.Throws<InvalidOperationException>(() => be.id = Guid.NewGuid());
        Assert.AreEqual(entityID, be.id);

        // Set Visibility.
        be.SetVisibility(true);
        Assert.IsTrue(be.gameObject.activeSelf);
        Assert.IsTrue(be.GetVisibility());
        be.SetVisibility(false);
        Assert.IsFalse(be.gameObject.activeSelf);
        Assert.IsFalse(be.GetVisibility());

        // Set Highlight.
        be.SetHighlight(true);
        Assert.IsFalse(be.GetHighlight());
        be.SetHighlight(false);
        Assert.IsFalse(be.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        be.SetParent(parentBE);
        Assert.AreEqual(be.GetParent(), parentBE);
        be.SetParent(null);
        Assert.IsNull(be.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(be);
        BaseEntity[] children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(be);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        be.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        be.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, be.GetPosition(false));
        Assert.AreNotEqual(posToSet, be.GetPosition(true));
        be.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, be.GetPosition(true));
        Assert.AreNotEqual(posToSet, be.GetPosition(false));

        // Set Rotation/Get Rotation.
        be.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        be.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        be.SetRotation(rotToSet, true, false);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        be.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        be.SetEulerRotation(eRotToSet, false, false);
        be.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        be.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, be.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        Assert.Throws<NotImplementedException>(() => be.SetSize(sizeToSet, false));

        // Compare.
        Assert.IsTrue(be.Compare(be));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        be.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = be.GetPhysicalProperties();
        /*Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(phyProps.angularDrag, setProps.Value.angularDrag);
        Assert.AreEqual(phyProps.centerOfMass, setProps.Value.centerOfMass);
        Assert.AreEqual(phyProps.drag, setProps.Value.drag);
        Assert.AreEqual(phyProps.gravitational, setProps.Value.gravitational);
        Assert.AreEqual(phyProps.mass, setProps.Value.mass);*/

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        be.SetInteractionState(interactionState);
        //Assert.AreEqual(interactionState, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        be.SetInteractionState(interactionState);
        //Assert.AreEqual(interactionState, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        be.SetInteractionState(interactionState);
        //Assert.AreEqual(interactionState, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        be.SetInteractionState(interactionState);
        //Assert.AreEqual(interactionState, be.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        be.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = be.GetMotion();
        //Assert.IsTrue(setMotion.HasValue);
        //Assert.AreEqual(entityMotion.angularVelocity, setMotion.Value.angularVelocity);
        //Assert.AreEqual(entityMotion.stationary, setMotion.Value.stationary);
        //Assert.AreEqual(entityMotion.velocity, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        be.StartSynchronizing(synch);
        be.StopSynchronizing();

        // Delete Entity.
        be.Delete();
        yield return null;
        Assert.True(be == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_UIEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        UIEntity uie = go.AddComponent<UIEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        uie.Initialize(entityID);
        Assert.AreEqual(entityID, uie.id);
        Assert.Throws<InvalidOperationException>(() => uie.id = Guid.NewGuid());
        Assert.AreEqual(entityID, uie.id);

        // Set Visibility.
        uie.SetVisibility(true);
        Assert.IsTrue(uie.gameObject.activeSelf);
        Assert.IsFalse(uie.GetHighlight());
        uie.SetVisibility(false);
        Assert.IsFalse(uie.gameObject.activeSelf);
        Assert.IsFalse(uie.GetHighlight());

        // Set Highlight.
        uie.SetHighlight(true);
        Assert.IsFalse(uie.GetHighlight());
        uie.SetHighlight(false);
        Assert.IsFalse(uie.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject("parentGO");
        UIEntity parentUIE = parentGO.AddComponent<UIEntity>();
        parentUIE.Initialize(Guid.NewGuid());
        uie.SetParent(parentUIE);
        Assert.AreEqual(parentUIE, uie.GetParent());
        uie.SetParent(null);
        Assert.AreEqual(null, uie.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(uie);
        BaseEntity[] children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(uie);
        children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        uie.SetParent(parentUIE);
        parentUIE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        uie.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, uie.GetPosition(false));
        Assert.AreNotEqual(posToSet, uie.GetPosition(true));
        uie.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, uie.GetPosition(true));
        Assert.AreNotEqual(posToSet, uie.GetPosition(false));

        // Set Rotation/Get Rotation.
        uie.SetParent(parentUIE);
        parentUIE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        uie.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = uie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = uie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        uie.SetRotation(rotToSet, true, false);
        measured = uie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = uie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        uie.SetParent(parentUIE);
        parentUIE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        uie.SetEulerRotation(eRotToSet, false, false);
        uie.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        uie.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, uie.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        Assert.Throws<NotImplementedException>(() => uie.SetSize(sizeToSet, false));

        // Compare.
        Assert.IsTrue(uie.Compare(uie));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        uie.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = uie.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, uie.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        uie.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = uie.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        uie.StartSynchronizing(synch);
        uie.StopSynchronizing();

        // Delete Entity.
        uie.Delete();
        yield return null;
        Assert.True(uie == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_UIElementEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        UIElementEntity uie = go.AddComponent<UIElementEntity>();

        // Set up Canvas Entity.
        GameObject cGO = new GameObject();
        CanvasEntity ce = cGO.AddComponent<CanvasEntity>();
        Guid canvasEntityID = Guid.NewGuid();
        ce.Initialize(canvasEntityID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        LogAssert.Expect(LogType.Error, "[UIElementEntity->Initialize] UI element entity must be initialized with a parent canvas.");
        uie.Initialize(entityID);
        uie.Initialize(entityID, ce);
        Assert.AreEqual(entityID, uie.id);
        Assert.Throws<InvalidOperationException>(() => uie.id = Guid.NewGuid());
        Assert.AreEqual(entityID, uie.id);

        // Set Visibility.
        uie.SetVisibility(true);
        Assert.IsTrue(uie.gameObject.activeSelf);
        Assert.IsTrue(uie.GetVisibility());
        uie.SetVisibility(false);
        Assert.IsFalse(uie.gameObject.activeSelf);
        Assert.IsFalse(uie.GetVisibility());

        // Set Highlight.
        uie.SetHighlight(true);
        uie.SetHighlight(false);

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject("parentGO");
        UIElementEntity parentUIE = parentGO.AddComponent<UIElementEntity>();
        parentUIE.Initialize(Guid.NewGuid(), ce);
        uie.SetParent(parentUIE);
        Assert.AreEqual(uie.GetParent(), parentUIE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetParent] UI Element cannot have a null parent.");
        uie.SetParent(null);
        Assert.AreEqual(uie.GetParent(), parentUIE);

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(uie);
        BaseEntity[] children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(uie);
        children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = uie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        uie.SetParent(parentUIE);
        parentUIE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        uie.SetPosition(posToSet, false, false);
        Assert.AreNotEqual(posToSet, uie.GetPosition(false));
        Assert.AreNotEqual(posToSet, uie.GetPosition(true));
        uie.SetPosition(posToSet, true, false);
        Assert.AreNotEqual(posToSet, uie.GetPosition(true));
        Assert.AreNotEqual(posToSet, uie.GetPosition(false));

        // Set Rotation/Get Rotation.
        uie.SetParent(parentUIE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        uie.SetRotation(rotToSet, false, false);
        Quaternion measured = uie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = uie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        uie.SetRotation(rotToSet, true, false);
        measured = uie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = uie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        uie.SetParent(parentUIE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        parentUIE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        uie.SetEulerRotation(eRotToSet, false, false);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        uie.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetScale] Cannot set scale of a UI element entity.");
        uie.SetScale(sclToSet, false);
        Assert.AreNotEqual(sclToSet, uie.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        Assert.Throws<NotImplementedException>(() => uie.SetSize(sizeToSet, false));

        // Compare.
        Assert.IsTrue(uie.Compare(uie));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        uie.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = uie.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, uie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        uie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, uie.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        uie.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = uie.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        uie.StartSynchronizing(synch);
        uie.StopSynchronizing();

        // Delete Entity.
        uie.Delete();
        yield return null;
        Assert.True(uie == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_CanvasEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        CanvasEntity ce = go.AddComponent<CanvasEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        ce.Initialize(entityID);
        Assert.AreEqual(entityID, ce.id);
        Assert.Throws<InvalidOperationException>(() => ce.id = Guid.NewGuid());
        Assert.AreEqual(entityID, ce.id);

        // Set Visibility.
        ce.SetVisibility(true);
        Assert.IsTrue(ce.gameObject.activeSelf);
        Assert.IsTrue(ce.GetVisibility());
        ce.SetVisibility(false);
        Assert.IsFalse(ce.gameObject.activeSelf);
        Assert.IsFalse(ce.GetVisibility());

        // Set Highlight.
        ce.SetHighlight(true);
        Assert.IsFalse(ce.GetHighlight());
        ce.SetHighlight(false);
        Assert.IsFalse(ce.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        ce.SetParent(parentBE);
        Assert.AreEqual(ce.GetParent(), parentBE);
        ce.SetParent(null);
        Assert.IsNull(ce.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(ce);
        BaseEntity[] children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(ce);
        children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        ce.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        ce.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, ce.GetPosition(false));
        Assert.AreNotEqual(posToSet, ce.GetPosition(true));
        ce.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, ce.GetPosition(true));
        Assert.AreNotEqual(posToSet, ce.GetPosition(false));

        // Set Rotation/Get Rotation.
        ce.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        ce.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = ce.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ce.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        ce.SetRotation(rotToSet, true, false);
        measured = ce.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ce.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        ce.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        ce.SetEulerRotation(eRotToSet, false, false);
        ce.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        ce.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, ce.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSetV3 = new Vector3(1, 2, 3);
        ce.SetSize(sizeToSetV3, false);
        Assert.AreNotEqual(sizeToSetV3, ce.GetSize());
        Vector2 sizeToSetV2 = new Vector2(25, 39);
        ce.SetSize(sizeToSetV2, false);
        Assert.AreEqual(new Vector3(sizeToSetV2.x, sizeToSetV2.y, 0), ce.GetSize());

        // Compare.
        Assert.IsTrue(ce.Compare(ce));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        ce.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = ce.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        ce.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = ce.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        ce.StartSynchronizing(synch);
        ce.StopSynchronizing();

        // Make World Canvas/Make Screen Canvas/IsScreenCanvas.
        ce.MakeWorldCanvas();
        Assert.IsFalse(ce.IsScreenCanvas());
        ce.MakeScreenCanvas();
        Assert.IsTrue(ce.IsScreenCanvas());

        // Delete Entity.
        ce.Delete();
        yield return null;
        Assert.True(ce == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_ContainerEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        ContainerEntity be = go.AddComponent<ContainerEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        be.Initialize(entityID);
        Assert.AreEqual(entityID, be.id);
        Assert.Throws<InvalidOperationException>(() => be.id = Guid.NewGuid());
        Assert.AreEqual(entityID, be.id);

        // Set Visibility.
        be.SetVisibility(true);
        Assert.IsTrue(be.gameObject.activeSelf);
        Assert.IsTrue(be.GetVisibility());
        be.SetVisibility(false);
        Assert.IsFalse(be.gameObject.activeSelf);
        Assert.IsFalse(be.GetVisibility());

        // Set Highlight.
        be.SetHighlight(true);
        Assert.IsFalse(be.GetHighlight());
        be.SetHighlight(false);
        Assert.IsFalse(be.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        be.SetParent(parentBE);
        Assert.AreEqual(be.GetParent(), parentBE);
        be.SetParent(null);
        Assert.IsNull(be.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(be);
        BaseEntity[] children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(be);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        be.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        be.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, be.GetPosition(false));
        Assert.AreNotEqual(posToSet, be.GetPosition(true));
        be.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, be.GetPosition(true));
        Assert.AreNotEqual(posToSet, be.GetPosition(false));

        // Set Rotation/Get Rotation.
        be.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        be.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        be.SetRotation(rotToSet, true, false);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        be.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        be.SetEulerRotation(eRotToSet, false, false);
        be.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        be.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, be.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        Assert.Throws<NotImplementedException>(() => be.SetSize(sizeToSet, false));

        // Compare.
        Assert.IsTrue(be.Compare(be));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        be.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = be.GetPhysicalProperties();

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        be.SetInteractionState(interactionState);
        interactionState = BaseEntity.InteractionState.Static;
        be.SetInteractionState(interactionState);
        interactionState = BaseEntity.InteractionState.Placing;
        be.SetInteractionState(interactionState);
        interactionState = BaseEntity.InteractionState.Physical;
        be.SetInteractionState(interactionState);

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        be.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = be.GetMotion();

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        be.StartSynchronizing(synch);
        be.StopSynchronizing();

        // Delete Entity.
        be.Delete();
        yield return null;
        Assert.True(be == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_ButtonEntity()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject("WEGO");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject("BE");
        ButtonEntity be = go.AddComponent<ButtonEntity>();

        // Set up Canvas Entity.
        GameObject cGO = new GameObject();
        CanvasEntity ce = cGO.AddComponent<CanvasEntity>();
        Guid canvasEntityID = Guid.NewGuid();
        ce.Initialize(canvasEntityID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        LogAssert.Expect(LogType.Error, "[UIElementEntity->Initialize] UI element entity must be initialized with a parent canvas.");
        be.Initialize(entityID);
        be.Initialize(entityID, ce);
        Assert.AreEqual(entityID, be.id);
        Assert.Throws<InvalidOperationException>(() => be.id = Guid.NewGuid());
        Assert.AreEqual(entityID, be.id);

        // Set Visibility.
        be.SetVisibility(true);
        Assert.IsTrue(be.gameObject.activeSelf);
        Assert.IsTrue(be.GetVisibility());
        be.SetVisibility(false);
        Assert.IsFalse(be.gameObject.activeSelf);
        Assert.IsFalse(be.GetVisibility());

        // Set Highlight.
        be.SetHighlight(true);
        Assert.IsFalse(be.GetHighlight());
        be.SetHighlight(false);
        Assert.IsFalse(be.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject("parentGO");
        ButtonEntity parentBE = parentGO.AddComponent<ButtonEntity>();
        parentBE.Initialize(Guid.NewGuid(), ce);
        be.SetParent(parentBE);
        Assert.AreEqual(be.GetParent(), parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetParent] UI Element cannot have a null parent.");
        be.SetParent(null);
        Assert.AreEqual(be.GetParent(), parentBE);

        // Get Children.
        GameObject childGO1 = new GameObject("childGO1");
        GameObject childGO2 = new GameObject("childGO2");
        ButtonEntity childBE1 = childGO1.AddComponent<ButtonEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid(), ce);
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(be);
        BaseEntity[] children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(be);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(parentBE);
        childBE2.SetParent(parentBE);
        children = be.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        be.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        be.SetPosition(posToSet, false, false);
        Assert.AreNotEqual(posToSet, be.GetPosition(false));
        be.SetPosition(posToSet, true, false);
        Assert.AreNotEqual(posToSet, be.GetPosition(true));

        // Set Position Percent/Get Position Percent.
        Vector2 posPercentToSet = new Vector2(0.25f, 0.5f);
        be.SetPositionPercent(posPercentToSet, false);
        Assert.AreEqual(posPercentToSet, be.GetPositionPercent());

        // Set Rotation/Get Rotation.
        be.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        be.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        be.SetRotation(rotToSet, true, false);
        measured = be.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = be.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        be.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        be.SetEulerRotation(eRotToSet, false, false);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        be.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetScale] Cannot set scale of a UI element entity.");
        be.SetScale(sclToSet, false);
        Assert.AreNotEqual(sclToSet, be.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 0);
        Assert.Throws<NotImplementedException>(() => be.SetSize(sizeToSet, false));
        Assert.Throws<NotImplementedException>(() => be.GetSize());

        // Set Size Percent/Get Size Percent.
        be.SetPosition(Vector3.zero, false, false);
        Vector2 sizePercentToSet = new Vector2(0.25f, 0.5f);
        be.SetSizePercent(sizePercentToSet, false);
        Assert.AreEqual(sizePercentToSet, be.GetSizePercent());

        // Set OnClick.
        Action onClickEvent = new Action(() => { });
        be.SetOnClick(onClickEvent);

        // Compare.
        Assert.IsTrue(be.Compare(be));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        be.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = be.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        be.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        be.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing; // Not valid for button entity.
        be.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, be.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        be.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, be.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        be.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = be.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        be.StartSynchronizing(synch);
        be.StopSynchronizing();

        // Delete Entity.
        be.Delete();
        yield return null;
        Assert.True(be == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_InputEntity()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject("WEGO");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        yield return null;
        we.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject("IE");
        InputEntity ie = go.AddComponent<InputEntity>();

        // Set up Canvas Entity.
        GameObject cGO = new GameObject();
        CanvasEntity ce = cGO.AddComponent<CanvasEntity>();
        Guid canvasEntityID = Guid.NewGuid();
        ce.Initialize(canvasEntityID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        LogAssert.Expect(LogType.Error, "[UIElementEntity->Initialize] UI element entity must be initialized with a parent canvas.");
        ie.Initialize(entityID);
        ie.Initialize(entityID, ce);
        Assert.AreEqual(entityID, ie.id);
        Assert.Throws<InvalidOperationException>(() => ie.id = Guid.NewGuid());
        Assert.AreEqual(entityID, ie.id);

        // Set Visibility.
        ie.SetVisibility(true);
        Assert.IsTrue(ie.gameObject.activeSelf);
        Assert.IsTrue(ie.GetVisibility());
        ie.SetVisibility(false);
        Assert.IsFalse(ie.gameObject.activeSelf);
        Assert.IsFalse(ie.GetVisibility());

        // Set Highlight.
        ie.SetHighlight(true);
        Assert.IsFalse(ie.GetHighlight());
        ie.SetHighlight(false);
        Assert.IsFalse(ie.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject("parentGO");
        ButtonEntity parentBE = parentGO.AddComponent<ButtonEntity>();
        parentBE.Initialize(Guid.NewGuid(), ce);
        ie.SetParent(parentBE);
        Assert.AreEqual(ie.GetParent(), parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetParent] UI Element cannot have a null parent.");
        ie.SetParent(null);
        Assert.AreEqual(ie.GetParent(), parentBE);

        // Get Children.
        GameObject childGO1 = new GameObject("childGO1");
        GameObject childGO2 = new GameObject("childGO2");
        ButtonEntity childBE1 = childGO1.AddComponent<ButtonEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid(), ce);
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(ie);
        BaseEntity[] children = ie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(ie);
        children = ie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(parentBE);
        childBE2.SetParent(parentBE);
        children = ie.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        ie.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        ie.SetPosition(posToSet, false, false);
        Assert.AreNotEqual(posToSet, ie.GetPosition(false));
        ie.SetPosition(posToSet, true, false);
        Assert.AreNotEqual(posToSet, ie.GetPosition(true));

        // Set Position Percent/Get Position Percent.
        Vector2 posPercentToSet = new Vector2(0.25f, 0.5f);
        ie.SetPositionPercent(posPercentToSet, false);
        Assert.AreEqual(posPercentToSet, ie.GetPositionPercent());

        // Set Rotation/Get Rotation.
        ie.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        ie.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = ie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = ie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        ie.SetRotation(rotToSet, true, false);
        measured = ie.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = ie.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        ie.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        ie.SetEulerRotation(eRotToSet, false, false);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        ie.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetScale] Cannot set scale of a UI element entity.");
        ie.SetScale(sclToSet, false);
        Assert.AreNotEqual(sclToSet, ie.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 0);
        Assert.Throws<NotImplementedException>(() => ie.SetSize(sizeToSet, false));
        Assert.Throws<NotImplementedException>(() => ie.GetSize());

        // Set Size Percent/Get Size Percent.
        ie.SetPosition(Vector3.zero, false, false);
        Vector2 sizePercentToSet = new Vector2(0.25f, 0.5f);
        ie.SetSizePercent(sizePercentToSet, false);
        Assert.AreEqual(sizePercentToSet, ie.GetSizePercent());

        // Compare.
        Assert.IsTrue(ie.Compare(ie));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        ie.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = ie.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        ie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        ie.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing; // Not valid for button entity.
        ie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ie.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        ie.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ie.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        ie.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = ie.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        ie.StartSynchronizing(synch);
        ie.StopSynchronizing();

        // Delete Entity.
        ie.Delete();
        yield return null;
        Assert.True(ie == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_TextEntity()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject("WEGO");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject("TE");
        TextEntity te = go.AddComponent<TextEntity>();

        // Set up Canvas Entity.
        GameObject cGO = new GameObject();
        CanvasEntity ce = cGO.AddComponent<CanvasEntity>();
        Guid canvasEntityID = Guid.NewGuid();
        ce.Initialize(canvasEntityID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        LogAssert.Expect(LogType.Error, "[UIElementEntity->Initialize] UI element entity must be initialized with a parent canvas.");
        te.Initialize(entityID);
        te.Initialize(entityID, ce);
        Assert.AreEqual(entityID, te.id);
        Assert.Throws<InvalidOperationException>(() => te.id = Guid.NewGuid());
        Assert.AreEqual(entityID, te.id);

        // Set Visibility.
        te.SetVisibility(true);
        Assert.IsTrue(te.gameObject.activeSelf);
        Assert.IsTrue(te.GetVisibility());
        te.SetVisibility(false);
        Assert.IsFalse(te.gameObject.activeSelf);
        Assert.IsFalse(te.GetVisibility());

        // Set Highlight.
        te.SetHighlight(true);
        Assert.IsFalse(te.GetHighlight());
        te.SetHighlight(false);
        Assert.IsFalse(te.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject("parentGO");
        ButtonEntity parentBE = parentGO.AddComponent<ButtonEntity>();
        parentBE.Initialize(Guid.NewGuid(), ce);
        te.SetParent(parentBE);
        Assert.AreEqual(te.GetParent(), parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetParent] UI Element cannot have a null parent.");
        te.SetParent(null);
        Assert.AreEqual(te.GetParent(), parentBE);

        // Get Children.
        GameObject childGO1 = new GameObject("childGO1");
        GameObject childGO2 = new GameObject("childGO2");
        ButtonEntity childBE1 = childGO1.AddComponent<ButtonEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid(), ce);
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(te);
        BaseEntity[] children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(te);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(parentBE);
        childBE2.SetParent(parentBE);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        te.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        te.SetPosition(posToSet, false, false);
        Assert.AreNotEqual(posToSet, te.GetPosition(false));
        te.SetPosition(posToSet, true, false);
        Assert.AreNotEqual(posToSet, te.GetPosition(true));

        // Set Position Percent/Get Position Percent.
        Vector2 posPercentToSet = new Vector2(0.25f, 0.5f);
        te.SetPositionPercent(posPercentToSet, false);
        Assert.AreEqual(posPercentToSet, te.GetPositionPercent());

        // Set Rotation/Get Rotation.
        te.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        te.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");
        te.SetRotation(rotToSet, true, false);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        te.SetParent(parentBE);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        te.SetEulerRotation(eRotToSet, false, false);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");
        te.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        LogAssert.Expect(LogType.Error, "[UIElementEntity->SetScale] Cannot set scale of a UI element entity.");
        te.SetScale(sclToSet, false);
        Assert.AreNotEqual(sclToSet, te.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 0);
        Assert.Throws<NotImplementedException>(() => te.SetSize(sizeToSet, false));
        Assert.Throws<NotImplementedException>(() => te.GetSize());

        // Set Size Percent/Get Size Percent.
        te.SetPosition(Vector3.zero, false, false);
        Vector2 sizePercentToSet = new Vector2(0.25f, 0.5f);
        te.SetSizePercent(sizePercentToSet, false);
        Assert.AreEqual(sizePercentToSet, te.GetSizePercent());

        // Compare.
        Assert.IsTrue(te.Compare(te));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        te.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = te.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing; // Not valid for button entity.
        te.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, te.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        te.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = te.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Set Text/Get Text.
        string textToSet = "test";
        te.SetText(textToSet);
        Assert.AreEqual(textToSet, te.GetText());

        // Set Font Size/Get Font Size.
        int fontSizeToSet = 10;
        te.SetFontSize(fontSizeToSet);
        Assert.AreEqual(fontSizeToSet, te.GetFontSize());

        // Set Color/Get Color.
        Color colorToSet = Color.green;
        te.SetColor(colorToSet);
        Assert.AreEqual(colorToSet, te.GetColor());

        // Set Margins/Get Margins.
        Vector4 marginsToSet = new Vector4(1, 2, 3, 4);
        te.SetMargins(marginsToSet);
        Assert.AreEqual(marginsToSet, te.GetMargins());

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        te.StartSynchronizing(synch);
        te.StopSynchronizing();

        // Delete Entity.
        te.Delete();
        yield return null;
        Assert.True(te == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_CharacterEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        CharacterEntity ce = go.AddComponent<CharacterEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        ce.radius = 0.1f;
        ce.Initialize(entityID);
        Assert.AreEqual(entityID, ce.id);
        Assert.Throws<InvalidOperationException>(() => ce.id = Guid.NewGuid());
        Assert.AreEqual(entityID, ce.id);

        // Set Visibility.
        ce.SetVisibility(true);
        Assert.IsTrue(ce.gameObject.activeSelf);
        Assert.IsTrue(ce.GetVisibility());
        ce.SetVisibility(false);
        Assert.IsFalse(ce.gameObject.activeSelf);
        Assert.IsFalse(ce.GetVisibility());

        // Set Highlight.
        ce.SetHighlight(true);
        Assert.IsFalse(ce.GetHighlight());
        ce.SetHighlight(false);
        Assert.IsFalse(ce.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        ce.SetParent(parentBE);
        Assert.AreEqual(ce.GetParent(), parentBE);
        ce.SetParent(null);
        Assert.IsNull(ce.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(ce);
        BaseEntity[] children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(ce);
        children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = ce.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        ce.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        ce.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, ce.GetPosition(false));
        Assert.AreNotEqual(posToSet, ce.GetPosition(true));
        ce.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, ce.GetPosition(true));
        Assert.AreNotEqual(posToSet, ce.GetPosition(false));

        // Set Rotation/Get Rotation.
        ce.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        ce.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = ce.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ce.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        ce.SetRotation(rotToSet, true, false);
        measured = ce.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ce.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        ce.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        ce.SetEulerRotation(eRotToSet, false, false);
        ce.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        ce.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, ce.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        ce.SetSize(sizeToSet, false);
        Assert.AreEqual(sizeToSet, ce.GetSize());

        // Compare.
        Assert.IsTrue(ce.Compare(ce));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        ce.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = ce.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(phyProps.angularDrag, setProps.Value.angularDrag);
        Assert.AreEqual(phyProps.centerOfMass, setProps.Value.centerOfMass);
        Assert.AreEqual(phyProps.drag, setProps.Value.drag);
        Assert.AreEqual(phyProps.gravitational, setProps.Value.gravitational);
        Assert.AreEqual(phyProps.mass, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        ce.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ce.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        ce.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = ce.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        //Assert.AreEqual(entityMotion.angularVelocity, setMotion.Value.angularVelocity);
        //Assert.AreEqual(entityMotion.stationary, setMotion.Value.stationary);
        //Assert.AreEqual(entityMotion.velocity, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        ce.StartSynchronizing(synch);
        ce.StopSynchronizing();

        // Delete Entity.
        ce.Delete();
        yield return null;
        Assert.True(ce == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_LightEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        LightEntity le = go.AddComponent<LightEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        le.Initialize(entityID);
        Assert.AreEqual(entityID, le.id);
        Assert.Throws<InvalidOperationException>(() => le.id = Guid.NewGuid());
        Assert.AreEqual(entityID, le.id);

        // Set Visibility.
        le.SetVisibility(true);
        Assert.IsTrue(le.gameObject.activeSelf);
        Assert.IsTrue(le.GetVisibility());
        le.SetVisibility(false);
        Assert.IsFalse(le.gameObject.activeSelf);
        Assert.IsFalse(le.GetVisibility());

        // Set Highlight.
        le.SetHighlight(true);
        Assert.IsFalse(le.GetHighlight());
        le.SetHighlight(false);
        Assert.IsFalse(le.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        le.SetParent(parentBE);
        Assert.AreEqual(le.GetParent(), parentBE);
        le.SetParent(null);
        Assert.IsNull(le.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(le);
        BaseEntity[] children = le.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(le);
        children = le.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = le.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        le.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        le.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, le.GetPosition(false));
        Assert.AreNotEqual(posToSet, le.GetPosition(true));
        le.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, le.GetPosition(true));
        Assert.AreNotEqual(posToSet, le.GetPosition(false));

        // Set Rotation/Get Rotation.
        le.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        le.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = le.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = le.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        le.SetRotation(rotToSet, true, false);
        measured = le.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = le.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        le.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        le.SetEulerRotation(eRotToSet, false, false);
        le.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        le.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, le.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        LogAssert.Expect(LogType.Warning, "[LightEntity->SetSize] Size not settable for light.");
        le.SetSize(sizeToSet, false);

        // Compare.
        Assert.IsTrue(le.Compare(le));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        LogAssert.Expect(LogType.Warning, "[LightEntity->SetPhysicalProperties] Physical properties not settable for light.");
        le.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = le.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(0, setProps.Value.angularDrag);
        Assert.AreEqual(Vector3.zero, setProps.Value.centerOfMass);
        Assert.AreEqual(0, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(0, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        le.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, le.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        le.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, le.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        le.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, le.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        le.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, le.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        LogAssert.Expect(LogType.Warning, "[LightEntity->SetMotion] Motion not settable for light.");
        le.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = le.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Set Light Type/Get Light Type.
        le.SetLightType(LightEntity.LightType.Point);
        Assert.AreEqual(LightEntity.LightType.Point, le.GetLightType());
        le.SetLightType(LightEntity.LightType.Directional);
        Assert.AreEqual(LightEntity.LightType.Directional, le.GetLightType());
        le.SetLightType(LightEntity.LightType.Spot);
        Assert.AreEqual(LightEntity.LightType.Spot, le.GetLightType());

        // Set Light Properties/Get Light Properties.
        le.SetLightProperties(1, 2);
        LightEntity.LightProperties lProps = le.GetLightProperties();
        Assert.AreEqual(1, lProps.range);
        Assert.AreEqual(2, lProps.intensity);
        Color32 col = Color.blue;
        le.SetLightProperties(col, 1, 2);
        lProps = le.GetLightProperties();
        Assert.AreEqual(col, lProps.color);
        Assert.AreEqual(1, lProps.temperature);
        Assert.AreEqual(2, lProps.intensity);
        col = Color.green;
        le.SetLightProperties(1, 2, 3, col, 4, 5);
        lProps = le.GetLightProperties();
        Assert.AreEqual(1, lProps.range);
        Assert.AreEqual(2, lProps.innerSpotAngle);
        Assert.AreEqual(3, lProps.outerSpotAngle);
        Assert.AreEqual(col, lProps.color);
        Assert.AreEqual(4, lProps.temperature);
        Assert.AreEqual(5, lProps.intensity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        le.StartSynchronizing(synch);
        le.StopSynchronizing();

        // Delete Entity.
        le.Delete();
        yield return null;
        Assert.True(le == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_MeshEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshEntity me = go.AddComponent<MeshEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        me.Initialize(entityID);
        Assert.AreEqual(entityID, me.id);
        Assert.Throws<InvalidOperationException>(() => me.id = Guid.NewGuid());
        Assert.AreEqual(entityID, me.id);

        // Set Visibility.
        me.SetVisibility(true);
        Assert.IsTrue(me.gameObject.activeSelf);
        Assert.IsTrue(me.GetVisibility());
        me.SetVisibility(false);
        Assert.IsFalse(me.gameObject.activeSelf);
        Assert.IsFalse(me.GetVisibility());

        // Set Highlight.
        me.SetHighlight(true);
        Assert.IsFalse(me.GetHighlight());
        me.SetHighlight(false);
        Assert.IsFalse(me.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        me.SetParent(parentBE);
        Assert.AreEqual(me.GetParent(), parentBE);
        me.SetParent(null);
        Assert.IsNull(me.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(me);
        BaseEntity[] children = me.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(me);
        children = me.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = me.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        me.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        me.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, me.GetPosition(false));
        Assert.AreNotEqual(posToSet, me.GetPosition(true));
        me.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, me.GetPosition(true));
        Assert.AreNotEqual(posToSet, me.GetPosition(false));

        // Set Rotation/Get Rotation.
        me.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        me.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = me.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = me.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        me.SetRotation(rotToSet, true, false);
        measured = me.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = me.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        me.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        me.SetEulerRotation(eRotToSet, false, false);
        me.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        me.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, me.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        me.SetSize(sizeToSet, false);
        Assert.AreEqual(sizeToSet, me.GetSize());

        // Compare.
        Assert.IsTrue(me.Compare(me));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        me.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = me.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(phyProps.angularDrag, setProps.Value.angularDrag);
        Assert.AreEqual(phyProps.centerOfMass, setProps.Value.centerOfMass);
        Assert.AreEqual(phyProps.drag, setProps.Value.drag);
        Assert.AreEqual(phyProps.gravitational, setProps.Value.gravitational);
        Assert.AreEqual(phyProps.mass, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        me.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, me.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        me.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, me.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        me.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, me.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        me.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, me.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        me.SetMotion(entityMotion);
        yield return null;
        BaseEntity.EntityMotion? setMotion = me.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        //Assert.AreEqual(entityMotion.angularVelocity, setMotion.Value.angularVelocity);
        Assert.AreEqual(entityMotion.stationary, setMotion.Value.stationary);
        //Assert.AreEqual(entityMotion.velocity, setMotion.Value.velocity);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        me.StartSynchronizing(synch);
        me.StopSynchronizing();

        // Delete Entity.
        me.Delete();
        yield return null;
        Assert.True(me == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_TerrainEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        Guid tID = Guid.NewGuid();
        float[,] heights = { { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 }};
        TerrainEntityLayer[] layers = new TerrainEntityLayer[3]
        {
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/1.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/2.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/3.png")
            }
        };
        float[,] layerMask = { { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }};
        Dictionary<int, float[,]> layerMasks = new Dictionary<int, float[,]>();
        layerMasks.Add(0, layerMask);
        layerMasks.Add(1, layerMask);
        layerMasks.Add(2, layerMask);
        TerrainEntity te = TerrainEntity.Create(8, 8, 8, heights, layers, layerMasks, tID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Assert.AreEqual(tID, te.id);
        Assert.Throws<InvalidOperationException>(() => te.id = Guid.NewGuid());
        Assert.AreEqual(tID, te.id);

        // Set Visibility.
        te.SetVisibility(true);
        Assert.IsTrue(te.terrain.drawHeightmap);
        Assert.IsTrue(te.GetVisibility());
        te.SetVisibility(false);
        Assert.IsFalse(te.terrain.drawHeightmap);
        Assert.IsFalse(te.GetVisibility());

        // Set Highlight.
        te.SetHighlight(true);
        Assert.IsTrue(te.GetHighlight());
        te.SetHighlight(false);
        Assert.IsFalse(te.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        te.SetParent(parentBE);
        Assert.AreEqual(te.GetParent(), parentBE);
        te.SetParent(null);
        Assert.IsNull(te.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(te);
        BaseEntity[] children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(te);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        te.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        te.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, te.GetPosition(false));
        Assert.AreNotEqual(posToSet, te.GetPosition(true));
        te.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, te.GetPosition(true));
        Assert.AreNotEqual(posToSet, te.GetPosition(false));

        // Set Rotation/Get Rotation.
        te.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        te.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        te.SetRotation(rotToSet, true, false);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        te.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        te.SetEulerRotation(eRotToSet, false, false);
        te.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        te.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, te.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        te.SetSize(sizeToSet, false);
        Assert.AreEqual(sizeToSet, te.GetSize());

        // Compare.
        Assert.IsTrue(te.Compare(te));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        te.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = te.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.angularDrag);
        Assert.AreEqual(new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity), setProps.Value.centerOfMass);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        te.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = te.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Set Heights/Get Heights.
        float[,] newHeights = { { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
                                { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 } };
        te.SetHeights(16, 16, 16, newHeights);
        float[,] gottenHeights = te.GetHeights();
        Assert.AreEqual(16, gottenHeights.GetLength(0));
        Assert.AreEqual(16, gottenHeights.GetLength(1));
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                Assert.AreEqual(newHeights[i, j], gottenHeights[i, j]);
            }
        }

        // Set Height/Get Height.
        te.SetHeight(2, 5, 7);
        Assert.AreEqual(7, te.GetHeight(2, 5));

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        te.StartSynchronizing(synch);
        te.StopSynchronizing();

        // Delete Entity.
        te.Delete();
        yield return null;
        Assert.True(te == null);
    }

#if TEST_HYBRID_TERRAIN
    [UnityTest]
    public IEnumerator EntityTests_HybridTerrainEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        Guid tID = Guid.NewGuid();
        float[,] heights = { { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 },
                             { 0, 1, 2, 3, 4, 5, 6, 7 }};
        TerrainEntityLayer[] layers = new TerrainEntityLayer[3]
        {
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/1.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/2.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/3.png")
            }
        };
        float[,] layerMask = { { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }};
        Dictionary<int, float[,]> layerMasks = new Dictionary<int, float[,]>();
        layerMasks.Add(0, layerMask);
        layerMasks.Add(1, layerMask);
        layerMasks.Add(2, layerMask);
        HybridTerrainEntity te = HybridTerrainEntity.Create(8, 8, 8, heights, layers, layerMasks, tID);

        // Initialize Entity with ID and ensure it cannot be changed.
        Assert.AreEqual(tID, te.id);
        Assert.Throws<InvalidOperationException>(() => te.id = Guid.NewGuid());
        Assert.AreEqual(tID, te.id);

        // Set Visibility.
        te.SetVisibility(true);
        Assert.IsTrue(te.terrain.drawHeightmap);
        Assert.IsTrue(te.GetVisibility());
        te.SetVisibility(false);
        Assert.IsFalse(te.terrain.drawHeightmap);
        Assert.IsFalse(te.GetVisibility());

        // Set Highlight.
        te.SetHighlight(true);
        Assert.IsTrue(te.GetHighlight());
        te.SetHighlight(false);
        Assert.IsFalse(te.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        te.SetParent(parentBE);
        Assert.AreEqual(te.GetParent(), parentBE);
        te.SetParent(null);
        Assert.IsNull(te.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(te);
        BaseEntity[] children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(te);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = te.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        te.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        te.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, te.GetPosition(false));
        Assert.AreNotEqual(posToSet, te.GetPosition(true));
        te.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, te.GetPosition(true));
        Assert.AreNotEqual(posToSet, te.GetPosition(false));

        // Set Rotation/Get Rotation.
        te.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        te.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        te.SetRotation(rotToSet, true, false);
        measured = te.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = te.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        te.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        te.SetEulerRotation(eRotToSet, false, false);
        te.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        te.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, te.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        te.SetSize(sizeToSet, false);
        Assert.AreEqual(sizeToSet, te.GetSize());

        // Compare.
        Assert.IsTrue(te.Compare(te));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        te.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = te.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.angularDrag);
        Assert.AreEqual(new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity), setProps.Value.centerOfMass);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, te.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        te.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, te.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        te.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = te.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Build.
        te.Build(new Vector3(1, 2, 3), TerrainEntityBrushType.roundedCube, 1);
        yield return new WaitForSeconds(1);
        te.Build(new Vector3(3, 4, 5), TerrainEntityBrushType.sphere, 2);
        yield return new WaitForSeconds(1);

        // Dig.
        te.Dig(new Vector3(3, 2, 1), TerrainEntityBrushType.roundedCube, 0);
        yield return new WaitForSeconds(1);
        te.Dig(new Vector3(0, 5, 4), TerrainEntityBrushType.sphere, 1);
        yield return new WaitForSeconds(1);

        // Get Block at Position.
        Assert.AreEqual(1, te.GetBlockAtPosition(new Vector3(1, 2, 3)).Item2);
        Assert.AreEqual(HybridTerrainEntity.TerrainOperation.Build, te.GetBlockAtPosition(new Vector3(1, 2, 3)).Item1);
        Assert.AreEqual(0, te.GetBlockAtPosition(new Vector3(3, 2, 1)).Item2);
        Assert.AreEqual(HybridTerrainEntity.TerrainOperation.Dig, te.GetBlockAtPosition(new Vector3(3, 2, 1)).Item1);

        // Get Layer.
        te.GetLayer(0);
        te.GetLayer(1);
        te.GetLayer(2);

        // Get Layer Mask.
        te.GetLayerMask(0);
        te.GetLayerMask(1);
        te.GetLayerMask(2);

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        te.StartSynchronizing(synch);
        te.StopSynchronizing();

        // Delete Entity.
        te.Delete();
        yield return null;
        Assert.True(te == null);
    }
#endif

    [UnityTest]
    public IEnumerator EntityTests_VoxelEntity()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        VoxelEntity ve = go.AddComponent<VoxelEntity>();

        // Initialize Entity with ID and ensure it cannot be changed.
        Guid entityID = Guid.NewGuid();
        ve.Initialize(entityID);
        Assert.AreEqual(entityID, ve.id);
        Assert.Throws<InvalidOperationException>(() => ve.id = Guid.NewGuid());
        Assert.AreEqual(entityID, ve.id);

        // Set Visibility.
        ve.SetVisibility(true);
        Assert.IsTrue(ve.gameObject.activeSelf);
        Assert.IsTrue(ve.GetVisibility());
        ve.SetVisibility(false);
        Assert.IsFalse(ve.gameObject.activeSelf);
        Assert.IsFalse(ve.GetVisibility());

        // Set Highlight.
        ve.SetHighlight(true);
        Assert.IsFalse(ve.GetHighlight());
        ve.SetHighlight(false);
        Assert.IsFalse(ve.GetHighlight());

        // Set Parent/Get Parent.
        GameObject parentGO = new GameObject();
        BaseEntity parentBE = parentGO.AddComponent<BaseEntity>();
        parentBE.Initialize(Guid.NewGuid());
        ve.SetParent(parentBE);
        Assert.AreEqual(ve.GetParent(), parentBE);
        ve.SetParent(null);
        Assert.IsNull(ve.GetParent());

        // Get Children.
        GameObject childGO1 = new GameObject();
        GameObject childGO2 = new GameObject();
        BaseEntity childBE1 = childGO1.AddComponent<BaseEntity>();
        BaseEntity childBE2 = childGO2.AddComponent<BaseEntity>();
        childBE1.Initialize(Guid.NewGuid());
        childBE2.Initialize(Guid.NewGuid());
        childBE1.SetParent(ve);
        BaseEntity[] children = ve.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 1);
        Assert.AreEqual(children[0], childBE1);
        childBE2.SetParent(ve);
        children = ve.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 2);
        Assert.IsTrue(children[0] == childBE1 || children[1] == childBE1);
        Assert.IsTrue(children[0] == childBE2 || children[1] == childBE2);
        childBE1.SetParent(null);
        childBE2.SetParent(null);
        children = ve.GetChildren();
        Assert.IsFalse(children == null);
        Assert.IsTrue(children.Length == 0);

        // Set Position/Get Position.
        ve.SetParent(parentBE);
        parentBE.SetPosition(new Vector3(1, 2, 3), false, false);
        Vector3 posToSet = new Vector3(1, 2, 3);
        ve.SetPosition(posToSet, false, false);
        Assert.AreEqual(posToSet, ve.GetPosition(false));
        Assert.AreNotEqual(posToSet, ve.GetPosition(true));
        ve.SetPosition(posToSet, true, false);
        Assert.AreEqual(posToSet, ve.GetPosition(true));
        Assert.AreNotEqual(posToSet, ve.GetPosition(false));

        // Set Rotation/Get Rotation.
        ve.SetParent(parentBE);
        parentBE.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 1), false, false);
        Quaternion rotToSet = new Quaternion(0.1f, 0.2f, 0.3f, 1);
        ve.SetRotation(rotToSet, false, false);
        yield return null;
        Quaternion measured = ve.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ve.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);
        ve.SetRotation(rotToSet, true, false);
        measured = ve.GetRotation(true);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreEqual(rotToSet, measured);
        measured = ve.GetRotation(false);
        measured = new Quaternion((float) Math.Round(measured.x, 1), (float) Math.Round(measured.y, 1),
            (float) Math.Round(measured.z, 1), (float) Math.Round(measured.w, 0));
        Assert.AreNotEqual(rotToSet, measured);

        // Set Euler Rotation/Get Rotation.
        ve.SetParent(parentBE);
        parentBE.SetEulerRotation(new Vector3(45, 90, 180), false, false);
        Vector3 eRotToSet = new Vector3(45, 90, 180);
        ve.SetEulerRotation(eRotToSet, false, false);
        ve.SetEulerRotation(eRotToSet, true, false);

        // Set Scale/Get Scale.
        Vector3 sclToSet = new Vector3(1, 2, 3);
        ve.SetScale(sclToSet, false);
        Assert.AreEqual(sclToSet, ve.GetScale());

        // Set Size/Get Size.
        Vector3 sizeToSet = new Vector3(1, 2, 3);
        Assert.Throws<NotImplementedException>(() => ve.SetSize(sizeToSet, false));

        // Compare.
        Assert.IsTrue(ve.Compare(ve));

        // Set Physical Properties/Get Physical Properties.
        BaseEntity.EntityPhysicalProperties phyProps = new BaseEntity.EntityPhysicalProperties()
        {
            angularDrag = 1,
            centerOfMass = new Vector3(1, 2, 3),
            drag = 2,
            gravitational = true,
            mass = 42
        };
        ve.SetPhysicalProperties(phyProps);
        BaseEntity.EntityPhysicalProperties? setProps = ve.GetPhysicalProperties();
        Assert.IsTrue(setProps.HasValue);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.angularDrag);
        Assert.AreEqual(new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity), setProps.Value.centerOfMass);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.drag);
        Assert.AreEqual(false, setProps.Value.gravitational);
        Assert.AreEqual(float.PositiveInfinity, setProps.Value.mass);

        // Set Interaction State/Get Interaction State.
        BaseEntity.InteractionState interactionState = BaseEntity.InteractionState.Hidden;
        ve.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ve.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Static;
        ve.SetInteractionState(interactionState);
        Assert.AreEqual(interactionState, ve.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Placing;
        ve.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ve.GetInteractionState());
        interactionState = BaseEntity.InteractionState.Physical;
        ve.SetInteractionState(interactionState);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ve.GetInteractionState());

        // Set Motion/Get Motion.
        BaseEntity.EntityMotion entityMotion = new BaseEntity.EntityMotion()
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = true,
            velocity = new Vector3(3, 4, 5)
        };
        ve.SetMotion(entityMotion);
        BaseEntity.EntityMotion? setMotion = ve.GetMotion();
        Assert.IsTrue(setMotion.HasValue);
        Assert.AreEqual(Vector3.zero, setMotion.Value.angularVelocity);
        Assert.AreEqual(true, setMotion.Value.stationary);
        Assert.AreEqual(Vector3.zero, setMotion.Value.velocity);

        // Set Block Info.
        FiveSQD.StraightFour.Entity.Voxels.BlockInfo blockInfo = new FiveSQD.StraightFour.Entity.Voxels.BlockInfo(0);
        blockInfo.AddSubType(0, true, null, null, null, null, null, null);
        blockInfo.AddSubType(1, false, null, null, null, null, null, null);
        ve.SetBlockInfo(0, blockInfo);

        // Set Block/Get Block.
        ve.SetBlock(0, 0, 0, 0, 0);
        int[] bInfo = ve.GetBlock(0, 0, 0);
        Assert.AreEqual(2, bInfo.Length);
        Assert.AreEqual(0, bInfo[0]);
        Assert.AreEqual(0, bInfo[1]);

        // Contains Chunk.
        Assert.AreEqual(true, ve.ContainsChunk(0, 0, 0));
        Assert.AreEqual(false, ve.ContainsChunk(10, 10, 10));

        // Start Synchronizing/Stop Synchronizing.
        GameObject synchGO = new GameObject();
        BaseSynchronizer synch = synchGO.AddComponent<BaseSynchronizer>();
        ve.StartSynchronizing(synch);
        ve.StopSynchronizing();

        // Delete Entity.
        ve.Delete();
        yield return null;
        Assert.True(ve == null);
    }

    [UnityTest]
    public IEnumerator EntityTests_CharacterLabelBillboard()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -10);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.characterControllerLabelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/CharacterPrefabLabel.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create a character entity with a label
        Guid characterID = StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(
            null, we.characterControllerPrefab, Vector3.zero, Quaternion.identity, 
            Vector3.zero, Vector3.zero, Quaternion.identity, Vector3.one);
        
        yield return null;

        // Get the character entity
        CharacterEntity character = StraightFour.ActiveWorld.entityManager.FindEntity(characterID) as CharacterEntity;
        Assert.IsNotNull(character);

        // Find the character label with Billboard component
        Billboard[] billboards = character.GetComponentsInChildren<Billboard>();
        Assert.AreEqual(1, billboards.Length, "Character should have exactly one Billboard component for its label");

        Billboard labelBillboard = billboards[0];
        Assert.IsNotNull(labelBillboard);
        Assert.IsTrue(labelBillboard.lockXAxis, "Character label billboard should have X axis locked");

        // Test billboard behavior
        Vector3 initialCameraPosition = camera.transform.position;
        Vector3 labelPosition = labelBillboard.transform.position;

        // Move camera to different positions and verify label rotation
        Vector3[] testPositions = {
            new Vector3(10, 0, 0),  // Right
            new Vector3(-10, 0, 0), // Left
            new Vector3(0, 10, 0),  // Above
            new Vector3(0, 0, 10)   // Behind
        };

        foreach (Vector3 testPos in testPositions)
        {
            camera.transform.position = testPos;
            yield return null; // Let Update() run
            
            // Calculate expected direction (billboard should face camera)
            Vector3 expectedDirection = (testPos - labelPosition);
            expectedDirection.y = 0; // X axis is locked, so no Y rotation
            expectedDirection.Normalize();

            // Get actual forward direction of the billboard
            Vector3 actualDirection = labelBillboard.transform.forward;
            actualDirection.y = 0;
            actualDirection.Normalize();

            // They should be pointing in the same general direction (dot product close to 1)
            float dot = Vector3.Dot(expectedDirection, actualDirection);
            Assert.IsTrue(dot > 0.8f, $"Billboard should face camera at position {testPos}. Dot product: {dot}");
        }

        // Clean up
        character.Delete();
        yield return null;
    }
}