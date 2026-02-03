// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

// Tests disabled - NUnit not available in this project
// To enable, add ENABLE_OMI_TESTS to Scripting Define Symbols
#if ENABLE_OMI_TESTS
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using NWH.VehiclePhysics2;

/// <summary>
/// Unit tests for AutomobileEntity OMI configuration APIs.
/// </summary>
public class AutomobileEntityOMIConfigTests
{
    private GameObject testGameObject;
    private AutomobileEntity testEntity;

    /// <summary>
    /// Setup runs before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        testGameObject = new GameObject("TestVehicle");
        testEntity = testGameObject.AddComponent<AutomobileEntity>();

        // Initialize required components
        Rigidbody rb = testGameObject.AddComponent<Rigidbody>();
        testEntity.rbody = rb;

        VehicleController vc = testGameObject.AddComponent<VehicleController>();
        testEntity.vehicleController = vc;

        testEntity.wheels = new System.Collections.Generic.List<GameObject>();
        testEntity.wheelControllers = new System.Collections.Generic.List<NWH.WheelController3D.WheelController>();
    }

    /// <summary>
    /// Cleanup runs after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            Object.DestroyImmediate(testGameObject);
        }
    }

    /// <summary>
    /// Test: ConfigureFromOMI with valid configuration applies settings.
    /// </summary>
    [Test]
    public void ConfigureFromOMI_ValidConfig_AppliesSettings()
    {
        // Arrange
        var config = new AutomobileEntity.OMIVehicleConfiguration
        {
            linearActivation = new Vector3(0, 0, 1),
            angularActivation = new Vector3(0, 1, 0),
            gyroTorque = new Vector3(0, 5, 0),
            maxSpeed = 25.0f, // 25 m/s = 90 km/h
            angularDampeners = true,
            linearDampeners = true,
            useThrottle = true
        };

        // Act
        testEntity.ConfigureFromOMI(config);

        // Assert
        Assert.IsNotNull(testEntity.vehicleController, "VehicleController should be present");
        Assert.IsNotNull(testEntity.rbody, "Rigidbody should be present");

        // Check dampening was applied
        if (config.linearDampeners)
        {
            Assert.Greater(testEntity.rbody.linearDamping, 0, "Linear dampening should be applied");
        }

        if (config.angularDampeners)
        {
            Assert.Greater(testEntity.rbody.angularDamping, 0, "Angular dampening should be applied");
        }
    }

    /// <summary>
    /// Test: ConfigureFromOMI with zero max speed.
    /// </summary>
    [Test]
    public void ConfigureFromOMI_ZeroMaxSpeed_NoError()
    {
        // Arrange
        var config = new AutomobileEntity.OMIVehicleConfiguration
        {
            linearActivation = Vector3.zero,
            angularActivation = Vector3.zero,
            gyroTorque = Vector3.zero,
            maxSpeed = 0,
            angularDampeners = false,
            linearDampeners = false,
            useThrottle = false
        };

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => testEntity.ConfigureFromOMI(config));
    }

    /// <summary>
    /// Test: ConfigureFromOMI without dampeners does not apply dampening.
    /// </summary>
    [Test]
    public void ConfigureFromOMI_NoDampeners_DoesNotApplyDampening()
    {
        // Arrange
        testEntity.rbody.linearDamping = 0;
        testEntity.rbody.angularDamping = 0;

        var config = new AutomobileEntity.OMIVehicleConfiguration
        {
            linearActivation = new Vector3(0, 0, 1),
            angularActivation = new Vector3(0, 1, 0),
            gyroTorque = Vector3.zero,
            maxSpeed = 10.0f,
            angularDampeners = false,
            linearDampeners = false,
            useThrottle = true
        };

        // Act
        testEntity.ConfigureFromOMI(config);

        // Assert
        Assert.AreEqual(0, testEntity.rbody.linearDamping,
            "Linear damping should remain 0 when linearDampeners is false");
        Assert.AreEqual(0, testEntity.rbody.angularDamping,
            "Angular damping should remain 0 when angularDampeners is false");
    }

    /// <summary>
    /// Test: AttachOMIWheel creates WheelController with correct parameters.
    /// </summary>
    [Test]
    public void AttachOMIWheel_ValidParameters_CreatesWheelController()
    {
        // Arrange
        GameObject wheelObject = new GameObject("Wheel");
        wheelObject.transform.SetParent(testGameObject.transform);
        float wheelRadius = 0.35f;
        float suspensionTravel = 0.2f;

        // Act
        testEntity.AttachOMIWheel(wheelObject.transform, wheelRadius, suspensionTravel);

        // Assert
        Assert.IsTrue(testEntity.wheels.Contains(wheelObject),
            "Wheel should be added to wheels list");
        Assert.AreEqual(1, testEntity.wheelControllers.Count,
            "WheelController should be added to list");

        var wheelController = wheelObject.GetComponent<NWH.WheelController3D.WheelController>();
        Assert.IsNotNull(wheelController, "WheelController component should be created");
        Assert.AreEqual(wheelRadius, wheelController.Radius,
            "Wheel radius should match OMI data");
        Assert.AreEqual(suspensionTravel, wheelController.SpringMaxLength,
            "Suspension travel should match OMI data");

        // Cleanup
        Object.DestroyImmediate(wheelObject);
    }

    /// <summary>
    /// Test: AttachOMIWheel with multiple wheels.
    /// </summary>
    [Test]
    public void AttachOMIWheel_MultipleWheels_AddsAllWheels()
    {
        // Arrange
        GameObject[] wheels = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            wheels[i] = new GameObject($"Wheel{i}");
            wheels[i].transform.SetParent(testGameObject.transform);
        }

        // Act
        testEntity.AttachOMIWheel(wheels[0].transform, 0.35f, 0.2f);
        testEntity.AttachOMIWheel(wheels[1].transform, 0.35f, 0.2f);
        testEntity.AttachOMIWheel(wheels[2].transform, 0.35f, 0.2f);
        testEntity.AttachOMIWheel(wheels[3].transform, 0.35f, 0.2f);

        // Assert
        Assert.AreEqual(4, testEntity.wheels.Count,
            "All 4 wheels should be added");
        Assert.AreEqual(4, testEntity.wheelControllers.Count,
            "All 4 WheelControllers should be created");

        // Cleanup
        foreach (var wheel in wheels)
        {
            Object.DestroyImmediate(wheel);
        }
    }

    /// <summary>
    /// Test: AttachOMIWheel with null transform logs error.
    /// </summary>
    [Test]
    public void AttachOMIWheel_NullTransform_LogsError()
    {
        // Arrange
        Transform nullTransform = null;

        // Act & Assert - should not throw, but logs error
        LogAssert.Expect(LogType.Error, "[AutomobileEntity->AttachOMIWheel] Wheel transform is null.");
        testEntity.AttachOMIWheel(nullTransform, 0.35f, 0.2f);

        Assert.AreEqual(0, testEntity.wheels.Count,
            "No wheels should be added when transform is null");
    }

    /// <summary>
    /// Test: AttachOMIWheel initializes lists if null.
    /// </summary>
    [Test]
    public void AttachOMIWheel_NullLists_InitializesLists()
    {
        // Arrange
        testEntity.wheels = null;
        testEntity.wheelControllers = null;
        GameObject wheelObject = new GameObject("Wheel");
        wheelObject.transform.SetParent(testGameObject.transform);

        // Act
        testEntity.AttachOMIWheel(wheelObject.transform, 0.35f, 0.2f);

        // Assert
        Assert.IsNotNull(testEntity.wheels, "Wheels list should be initialized");
        Assert.IsNotNull(testEntity.wheelControllers, "WheelControllers list should be initialized");
        Assert.AreEqual(1, testEntity.wheels.Count, "Wheel should be added after initialization");

        // Cleanup
        Object.DestroyImmediate(wheelObject);
    }

    /// <summary>
    /// Test: ConfigureFromOMI with binary throttle mode.
    /// </summary>
    [Test]
    public void ConfigureFromOMI_BinaryThrottleMode_ConfiguresCorrectly()
    {
        // Arrange
        var config = new AutomobileEntity.OMIVehicleConfiguration
        {
            linearActivation = new Vector3(0, 0, 1),
            angularActivation = Vector3.zero,
            gyroTorque = Vector3.zero,
            maxSpeed = 15.0f,
            angularDampeners = false,
            linearDampeners = false,
            useThrottle = false // Binary mode
        };

        // Act
        LogAssert.Expect(LogType.Log,
            new System.Text.RegularExpressions.Regex(".*Using binary throttle mode.*"));
        testEntity.ConfigureFromOMI(config);

        // Assert - no exception thrown
        Assert.Pass("Binary throttle mode configuration completed without error");
    }

    /// <summary>
    /// Test: OMIVehicleConfiguration struct can be created with all fields.
    /// </summary>
    [Test]
    public void OMIVehicleConfiguration_AllFields_CanBeSet()
    {
        // Act
        var config = new AutomobileEntity.OMIVehicleConfiguration
        {
            linearActivation = new Vector3(1, 2, 3),
            angularActivation = new Vector3(4, 5, 6),
            gyroTorque = new Vector3(7, 8, 9),
            maxSpeed = 50.0f,
            angularDampeners = true,
            linearDampeners = false,
            useThrottle = true
        };

        // Assert
        Assert.AreEqual(new Vector3(1, 2, 3), config.linearActivation);
        Assert.AreEqual(new Vector3(4, 5, 6), config.angularActivation);
        Assert.AreEqual(new Vector3(7, 8, 9), config.gyroTorque);
        Assert.AreEqual(50.0f, config.maxSpeed);
        Assert.IsTrue(config.angularDampeners);
        Assert.IsFalse(config.linearDampeners);
        Assert.IsTrue(config.useThrottle);
    }
}
#endif
