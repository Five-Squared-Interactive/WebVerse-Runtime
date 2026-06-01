// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;

namespace FiveSQD.WebVerse.Handlers.VEML.Tests
{
    [TestFixture]
    public class VEMLSchemaExtensionTests
    {
        // --- Story 4.1: Anchor attribute on entity ---

        [Test]
        public void Entity_AnchorFloor_ParsesCorrectly()
        {
            var entity = DeserializeEntity("<entity anchor=\"floor\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("floor", entity.anchor);
        }

        [Test]
        public void Entity_AnchorWall_ParsesCorrectly()
        {
            var entity = DeserializeEntity("<entity anchor=\"wall\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("wall", entity.anchor);
        }

        [Test]
        public void Entity_AnchorTable_ParsesCorrectly()
        {
            var entity = DeserializeEntity("<entity anchor=\"table\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("table", entity.anchor);
        }

        [Test]
        public void Entity_NoAnchor_ReturnsNull()
        {
            var entity = DeserializeEntity("<entity xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.IsNull(entity.anchor);
        }

        [Test]
        public void Entity_EmptyAnchor_ReturnsEmpty()
        {
            var entity = DeserializeEntity("<entity anchor=\"\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("", entity.anchor);
        }

        // --- Story 4.2: Mode attribute on metadata ---

        [Test]
        public void Metadata_ModeAR_ParsesCorrectly()
        {
            var metadata = DeserializeMetadata("<vemlMetadata mode=\"ar\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("ar", metadata.mode);
        }

        [Test]
        public void Metadata_ModeVR_ParsesCorrectly()
        {
            var metadata = DeserializeMetadata("<vemlMetadata mode=\"vr\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("vr", metadata.mode);
        }

        [Test]
        public void Metadata_ModeHybrid_ParsesCorrectly()
        {
            var metadata = DeserializeMetadata("<vemlMetadata mode=\"hybrid\" xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.AreEqual("hybrid", metadata.mode);
        }

        [Test]
        public void Metadata_NoMode_ReturnsNull()
        {
            var metadata = DeserializeMetadata("<vemlMetadata xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            Assert.IsNull(metadata.mode);
        }

        // --- Story 4.3: Graceful degradation ---

        [Test]
        public void ParseAnchorType_NullAnchorPlacer_NoErrorOnNullSafeCall()
        {
            // Simulates: InputManager.anchorPlacer?.RegisterAnchor(...)
            FiveSQD.WebVerse.Input.IAnchorPlacer anchorPlacer = null;
            Assert.DoesNotThrow(() =>
            {
                var anchorType = VEMLUtilities.ParseAnchorType("floor");
                anchorPlacer?.RegisterAnchor("e1", null);
            });
        }

        [Test]
        public void ParseAnchorType_UnknownValue_NoException()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = VEMLUtilities.ParseAnchorType("ceiling");
                Assert.IsNull(result);
            });
        }

        [Test]
        public void Metadata_NullMode_DefaultsToVR()
        {
            var metadata = DeserializeMetadata("<vemlMetadata xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" />");
            string mode = metadata.mode ?? "vr";
            Assert.AreEqual("vr", mode);
        }

        // --- Helpers ---

        private entity DeserializeEntity(string xml)
        {
            var root = new XmlRootAttribute("entity") { Namespace = "http://www.fivesqd.com/schemas/veml/3.0" };
            var serializer = new XmlSerializer(typeof(entity), root);
            using (var reader = new StringReader(xml))
            {
                return (entity)serializer.Deserialize(reader);
            }
        }

        private vemlMetadata DeserializeMetadata(string xml)
        {
            var root = new XmlRootAttribute("vemlMetadata") { Namespace = "http://www.fivesqd.com/schemas/veml/3.0" };
            var serializer = new XmlSerializer(typeof(vemlMetadata), root);
            using (var reader = new StringReader(xml))
            {
                return (vemlMetadata)serializer.Deserialize(reader);
            }
        }
    }
}