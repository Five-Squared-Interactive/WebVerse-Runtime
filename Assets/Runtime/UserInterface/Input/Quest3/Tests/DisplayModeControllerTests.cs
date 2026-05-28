// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    /// <summary>
    /// Mock IARProvider for DisplayModeController tests.
    /// </summary>
    public class MockARProvider : IARProvider
    {
        public XRDisplayMode CurrentDisplayMode { get; private set; } = XRDisplayMode.VR;
        public bool IsPassthroughSupported => true;
        public int EnableCallCount { get; private set; }
        public int DisableCallCount { get; private set; }

        public void EnablePassthrough()
        {
            CurrentDisplayMode = XRDisplayMode.AR;
            EnableCallCount++;
        }

        public void DisablePassthrough()
        {
            CurrentDisplayMode = XRDisplayMode.VR;
            DisableCallCount++;
        }
    }

    /// <summary>
    /// Mock IFadeTransition that invokes callback immediately (synchronous for testing).
    /// </summary>
    public class MockFadeTransition : IFadeTransition
    {
        public int FadeOutCallCount { get; private set; }
        public int FadeInCallCount { get; private set; }
        public List<string> CallOrder { get; } = new List<string>();

        public void FadeOut(Action callback)
        {
            FadeOutCallCount++;
            CallOrder.Add("FadeOut");
            callback?.Invoke();
        }

        public void FadeIn()
        {
            FadeInCallCount++;
            CallOrder.Add("FadeIn");
        }
    }

    [TestFixture]
    public class DisplayModeControllerTests
    {
        private InputManager _inputManager;
        private MockARProvider _mockAR;
        private MockFadeTransition _mockFade;
        private MockSurfaceDetector _mockSurface;
        private MockAnchorPlacer _mockAnchors;
        private DisplayModeController _controller;

        [SetUp]
        public void SetUp()
        {
            // InputManager is a MonoBehaviour, create a lightweight test double
            var go = new UnityEngine.GameObject("TestInputManager");
            _inputManager = go.AddComponent<InputManager>();

            _mockAR = new MockARProvider();
            _mockFade = new MockFadeTransition();
            _mockSurface = new MockSurfaceDetector();
            _mockAnchors = new MockAnchorPlacer();

            _inputManager.arProvider = _mockAR;
            _inputManager.fadeTransition = _mockFade;
            _inputManager.surfaceDetector = _mockSurface;
            _inputManager.anchorPlacer = _mockAnchors;

            _controller = new DisplayModeController(_inputManager);
        }

        [TearDown]
        public void TearDown()
        {
            if (_inputManager != null)
                UnityEngine.Object.DestroyImmediate(_inputManager.gameObject);
        }

        // --- Story 5.1: XRDisplayMode and initial state ---

        [Test]
        public void InitialDisplayMode_IsVR()
        {
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void XRDisplayMode_HasVRAndAR()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(XRDisplayMode), XRDisplayMode.VR));
            Assert.IsTrue(System.Enum.IsDefined(typeof(XRDisplayMode), XRDisplayMode.AR));
        }

        // --- Story 5.2: Mode toggle ---

        [Test]
        public void SwitchToAR_CallsSequence_FadeOut_EnablePassthrough_StartScanning_FadeIn()
        {
            _controller.SwitchToAR();

            Assert.AreEqual(1, _mockFade.FadeOutCallCount);
            Assert.AreEqual(1, _mockAR.EnableCallCount);
            Assert.AreEqual(1, _mockSurface.StartScanningCallCount);
            Assert.AreEqual(1, _mockFade.FadeInCallCount);
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void SwitchToVR_CallsSequence_FadeOut_DisablePassthrough_FadeIn()
        {
            _controller.SwitchToAR(); // Go to AR first
            _controller.SwitchToVR();

            Assert.AreEqual(2, _mockFade.FadeOutCallCount);
            Assert.AreEqual(1, _mockAR.DisableCallCount);
            Assert.AreEqual(2, _mockFade.FadeInCallCount);
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ToggleDisplayMode_FromVR_SwitchesToAR()
        {
            _controller.ToggleDisplayMode();
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ToggleDisplayMode_FromAR_SwitchesToVR()
        {
            _controller.ToggleDisplayMode(); // VR -> AR
            _controller.ToggleDisplayMode(); // AR -> VR
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ToggleDisplayMode_NullARProvider_Ignored()
        {
            _inputManager.arProvider = null;
            Assert.DoesNotThrow(() => _controller.ToggleDisplayMode());
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void SwitchToAR_NotifiesAnchorPlacer_ModeChanged()
        {
            _controller.SwitchToAR();
            Assert.AreEqual(XRDisplayMode.AR, _mockAnchors.LastModeChange);
        }

        [Test]
        public void SwitchToVR_NotifiesAnchorPlacer_ModeChanged()
        {
            _controller.SwitchToAR();
            _controller.SwitchToVR();
            Assert.AreEqual(XRDisplayMode.VR, _mockAnchors.LastModeChange);
        }

        // --- Story 5.3: Apply world mode ---

        [Test]
        public void ApplyWorldMode_AR_ActivatesARMode()
        {
            _controller.ApplyWorldMode("ar");
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ApplyWorldMode_VR_StaysInVR()
        {
            _controller.ApplyWorldMode("vr");
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ApplyWorldMode_VR_FromAR_SwitchesToVR()
        {
            _controller.SwitchToAR();
            _controller.ApplyWorldMode("vr");
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ApplyWorldMode_Hybrid_StaysInCurrentMode()
        {
            _controller.ApplyWorldMode("hybrid");
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ApplyWorldMode_Null_StaysInCurrentMode()
        {
            _controller.ApplyWorldMode(null);
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ApplyWorldMode_NullARProvider_Ignored()
        {
            _inputManager.arProvider = null;
            Assert.DoesNotThrow(() => _controller.ApplyWorldMode("ar"));
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        // --- Story 5.4: State preservation ---

        [Test]
        public void VRToARToVR_RoundTrip_DisplayModeCorrect()
        {
            _controller.SwitchToAR();
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);

            _controller.SwitchToVR();
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
        }

        [Test]
        public void ThreeConsecutiveSwitches_AllSucceed()
        {
            _controller.ToggleDisplayMode(); // VR -> AR
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);

            _controller.ToggleDisplayMode(); // AR -> VR
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);

            _controller.ToggleDisplayMode(); // VR -> AR
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);
        }

        // --- Integration: full VR->AR->VR round-trip ---

        [Test]
        public void FullRoundTrip_WithMockedInterfaces_VerifyFinalState()
        {
            // Start in VR
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);

            // Switch to AR
            _controller.ToggleDisplayMode();
            Assert.AreEqual(XRDisplayMode.AR, _controller.CurrentDisplayMode);
            Assert.AreEqual(1, _mockAR.EnableCallCount);
            Assert.IsTrue(_mockSurface.IsScanning);

            // Switch back to VR
            _controller.ToggleDisplayMode();
            Assert.AreEqual(XRDisplayMode.VR, _controller.CurrentDisplayMode);
            Assert.AreEqual(1, _mockAR.DisableCallCount);

            // Verify fade counts
            Assert.AreEqual(2, _mockFade.FadeOutCallCount);
            Assert.AreEqual(2, _mockFade.FadeInCallCount);
        }
    }
}