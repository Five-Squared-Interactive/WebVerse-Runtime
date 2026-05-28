// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    /// <summary>
    /// Test double for IPassthroughLayer.
    /// </summary>
    public class MockPassthroughLayer : IPassthroughLayer
    {
        public bool IsEnabled { get; private set; }
        public int EnableCallCount { get; private set; }
        public int DisableCallCount { get; private set; }
        public bool ThrowOnEnable { get; set; }

        public void Enable()
        {
            if (ThrowOnEnable)
                throw new System.Exception("Passthrough init failed");
            IsEnabled = true;
            EnableCallCount++;
        }

        public void Disable()
        {
            IsEnabled = false;
            DisableCallCount++;
        }
    }

    [TestFixture]
    public class Quest3ARProviderTests
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
        public void InitialDisplayMode_IsVR()
        {
            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void EnablePassthrough_SetsDisplayModeToAR()
        {
            _provider.EnablePassthrough();

            Assert.AreEqual(XRDisplayMode.AR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void EnablePassthrough_EnablesPassthroughLayer()
        {
            _provider.EnablePassthrough();

            Assert.IsTrue(_mockLayer.IsEnabled);
            Assert.AreEqual(1, _mockLayer.EnableCallCount);
        }

        [Test]
        public void DisablePassthrough_SetsDisplayModeToVR()
        {
            _provider.EnablePassthrough();
            _provider.DisablePassthrough();

            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void DisablePassthrough_DisablesPassthroughLayer()
        {
            _provider.EnablePassthrough();
            _provider.DisablePassthrough();

            Assert.IsFalse(_mockLayer.IsEnabled);
            Assert.AreEqual(1, _mockLayer.DisableCallCount);
        }

        [Test]
        public void IsPassthroughSupported_WithLayer_ReturnsTrue()
        {
            Assert.IsTrue(_provider.IsPassthroughSupported);
        }

        [Test]
        public void IsPassthroughSupported_WithNullLayer_ReturnsFalse()
        {
            var provider = new Quest3ARProvider(null);

            Assert.IsFalse(provider.IsPassthroughSupported);
        }

        [Test]
        public void EnablePassthrough_WithNullLayer_DoesNotThrow()
        {
            var provider = new Quest3ARProvider(null);

            Assert.DoesNotThrow(() => provider.EnablePassthrough());
            Assert.AreEqual(XRDisplayMode.VR, provider.CurrentDisplayMode);
        }

        [Test]
        public void DisablePassthrough_WithNullLayer_DoesNotThrow()
        {
            var provider = new Quest3ARProvider(null);

            Assert.DoesNotThrow(() => provider.DisablePassthrough());
        }

        [Test]
        public void EnableThenDisable_RestoresVRMode()
        {
            _provider.EnablePassthrough();
            Assert.AreEqual(XRDisplayMode.AR, _provider.CurrentDisplayMode);

            _provider.DisablePassthrough();
            Assert.AreEqual(XRDisplayMode.VR, _provider.CurrentDisplayMode);
        }

        [Test]
        public void MultipleEnableCalls_AreIdempotent()
        {
            _provider.EnablePassthrough();
            _provider.EnablePassthrough();

            Assert.AreEqual(XRDisplayMode.AR, _provider.CurrentDisplayMode);
            Assert.AreEqual(2, _mockLayer.EnableCallCount);
        }

        [Test]
        public void ImplementsIARProvider()
        {
            Assert.IsInstanceOf<IARProvider>(_provider);
        }
    }
}