// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    [TestFixture]
    public class Quest3ARProviderFailureTests
    {
        private MockPassthroughLayer _mockLayer;
        private Quest3ARProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _mockLayer = new MockPassthroughLayer();
            _provider = new Quest3ARProvider(_mockLayer);
        }

        [Test]
        public void EnablePassthrough_WhenLayerThrows_KeepsDisplayModeAsVR()
        {
            _mockLayer.ThrowOnEnable = true;

            _provider.EnablePassthrough();

            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void EnablePassthrough_WhenLayerThrows_DoesNotThrow()
        {
            _mockLayer.ThrowOnEnable = true;

            Assert.DoesNotThrow(() => _provider.EnablePassthrough());
        }

        [Test]
        public void EnablePassthrough_WhenLayerThrows_CallsOnARError()
        {
            _mockLayer.ThrowOnEnable = true;
            ARErrorType? receivedError = null;
            _provider.OnARError = (error) => receivedError = error;

            _provider.EnablePassthrough();

            Assert.AreEqual(ARErrorType.PassthroughFailed, receivedError);
        }

        [Test]
        public void EnablePassthrough_WhenLayerThrows_DisablesLayerDefensively()
        {
            _mockLayer.ThrowOnEnable = true;

            _provider.EnablePassthrough();

            Assert.IsFalse(_mockLayer.IsEnabled);
            Assert.AreEqual(1, _mockLayer.DisableCallCount);
        }

        [Test]
        public void HandleMidSessionFailure_InARMode_SetsDisplayModeToVR()
        {
            _provider.EnablePassthrough();
            Assert.AreEqual(XRDisplayMode.AR, _provider.CurrentDisplayMode);

            _provider.HandleMidSessionFailure();

            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void HandleMidSessionFailure_InARMode_CallsOnARError()
        {
            _provider.EnablePassthrough();
            ARErrorType? receivedError = null;
            _provider.OnARError = (error) => receivedError = error;

            _provider.HandleMidSessionFailure();

            Assert.AreEqual(ARErrorType.PassthroughFailed, receivedError);
        }

        [Test]
        public void HandleMidSessionFailure_InVRMode_DoesNothing()
        {
            ARErrorType? receivedError = null;
            _provider.OnARError = (error) => receivedError = error;

            _provider.HandleMidSessionFailure();

            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
            Assert.IsNull(receivedError);
        }

        [Test]
        public void HandleMidSessionFailure_DisablesPassthroughLayer()
        {
            _provider.EnablePassthrough();
            var countBefore = _mockLayer.DisableCallCount;

            _provider.HandleMidSessionFailure();

            Assert.IsFalse(_mockLayer.IsEnabled);
            Assert.AreEqual(countBefore + 1, _mockLayer.DisableCallCount);
        }

        [Test]
        public void EnablePassthrough_WhenLayerThrows_OnARErrorNotSet_DoesNotThrow()
        {
            _mockLayer.ThrowOnEnable = true;
            _provider.OnARError = null;

            Assert.DoesNotThrow(() => _provider.EnablePassthrough());
        }
    }
}