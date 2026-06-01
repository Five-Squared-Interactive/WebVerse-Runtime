// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.VEML;
using FiveSQD.WebVerse.Input;

namespace FiveSQD.WebVerse.Handlers.VEML.Tests
{
    [TestFixture]
    public class VEMLAnchorParsingTests
    {
        // --- ParseAnchorType ---

        [Test]
        public void ParseAnchorType_Floor_ReturnsFloor()
        {
            var result = VEMLUtilities.ParseAnchorType("floor");
            Assert.AreEqual(AnchorType.Floor, result);
        }

        [Test]
        public void ParseAnchorType_Table_ReturnsTable()
        {
            var result = VEMLUtilities.ParseAnchorType("table");
            Assert.AreEqual(AnchorType.Table, result);
        }

        [Test]
        public void ParseAnchorType_Wall_ReturnsWall()
        {
            var result = VEMLUtilities.ParseAnchorType("wall");
            Assert.AreEqual(AnchorType.Wall, result);
        }

        [Test]
        public void ParseAnchorType_CaseInsensitive_Floor_ReturnsFloor()
        {
            Assert.AreEqual(AnchorType.Floor, VEMLUtilities.ParseAnchorType("Floor"));
            Assert.AreEqual(AnchorType.Floor, VEMLUtilities.ParseAnchorType("FLOOR"));
        }

        [Test]
        public void ParseAnchorType_CaseInsensitive_TABLE_ReturnsTable()
        {
            Assert.AreEqual(AnchorType.Table, VEMLUtilities.ParseAnchorType("TABLE"));
        }

        [Test]
        public void ParseAnchorType_Null_ReturnsNull()
        {
            Assert.IsNull(VEMLUtilities.ParseAnchorType(null));
        }

        [Test]
        public void ParseAnchorType_Empty_ReturnsNull()
        {
            Assert.IsNull(VEMLUtilities.ParseAnchorType(""));
        }

        [Test]
        public void ParseAnchorType_InvalidValue_ReturnsNull()
        {
            Assert.IsNull(VEMLUtilities.ParseAnchorType("ceiling"));
        }

        [Test]
        public void ParseAnchorType_Unknown_ReturnsNull()
        {
            Assert.IsNull(VEMLUtilities.ParseAnchorType("unknown"));
        }
    }
}