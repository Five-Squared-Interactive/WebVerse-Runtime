// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests for the Events constants and validation registry.
    /// </summary>
    [TestFixture]
    public class EventsTests
    {
        // --- Constant Value Tests (AC #1, #2) ---

        [Test]
        public void WorldLoadEqualsExpectedString()
        {
            Assert.AreEqual("load", Events.World.Load);
        }

        [Test]
        public void WorldReadyEqualsExpectedString()
        {
            Assert.AreEqual("ready", Events.World.Ready);
        }

        [Test]
        public void WorldErrorEqualsExpectedString()
        {
            Assert.AreEqual("error", Events.World.Error);
        }

        [Test]
        public void EntitySpawnEqualsExpectedString()
        {
            Assert.AreEqual("spawn", Events.Entity.Spawn);
        }

        [Test]
        public void EntityDestroyEqualsExpectedString()
        {
            Assert.AreEqual("destroy", Events.Entity.Destroy);
        }

        [Test]
        public void CollisionEnterEqualsExpectedString()
        {
            Assert.AreEqual("collision:enter", Events.Collision.Enter);
        }

        [Test]
        public void CollisionExitEqualsExpectedString()
        {
            Assert.AreEqual("collision:exit", Events.Collision.Exit);
        }

        // --- IsValid True Tests (AC #3) ---

        [Test]
        public void IsValidReturnsTrueForWorldLoad()
        {
            Assert.IsTrue(Events.IsValid(Events.World.Load));
        }

        [Test]
        public void IsValidReturnsTrueForWorldReady()
        {
            Assert.IsTrue(Events.IsValid(Events.World.Ready));
        }

        [Test]
        public void IsValidReturnsTrueForWorldError()
        {
            Assert.IsTrue(Events.IsValid(Events.World.Error));
        }

        [Test]
        public void IsValidReturnsTrueForEntitySpawn()
        {
            Assert.IsTrue(Events.IsValid(Events.Entity.Spawn));
        }

        [Test]
        public void IsValidReturnsTrueForEntityDestroy()
        {
            Assert.IsTrue(Events.IsValid(Events.Entity.Destroy));
        }

        [Test]
        public void IsValidReturnsTrueForCollisionEnter()
        {
            Assert.IsTrue(Events.IsValid(Events.Collision.Enter));
        }

        [Test]
        public void IsValidReturnsTrueForCollisionExit()
        {
            Assert.IsTrue(Events.IsValid(Events.Collision.Exit));
        }

        [Test]
        public void IsValidReturnsTrueForRawStringMatchingConstant()
        {
            Assert.IsTrue(Events.IsValid("spawn"));
            Assert.IsTrue(Events.IsValid("ready"));
            Assert.IsTrue(Events.IsValid("collision:enter"));
        }

        // --- IsValid False Tests (AC #4) ---

        [Test]
        public void IsValidReturnsFalseForUnknownEventName()
        {
            Assert.IsFalse(Events.IsValid("nonexistent"));
        }

        [Test]
        public void IsValidReturnsFalseForMisspelledEventName()
        {
            Assert.IsFalse(Events.IsValid("collison:enter"));
        }

        [Test]
        public void IsValidReturnsFalseForEmptyString()
        {
            Assert.IsFalse(Events.IsValid(""));
        }

        [Test]
        public void IsValidReturnsFalseForNull()
        {
            Assert.IsFalse(Events.IsValid(null));
        }

        [Test]
        public void IsValidReturnsFalseForPartialEventName()
        {
            Assert.IsFalse(Events.IsValid("collision"));
            Assert.IsFalse(Events.IsValid("collision:"));
        }

        [Test]
        public void IsValidIsCaseSensitive()
        {
            Assert.IsFalse(Events.IsValid("Spawn"));
            Assert.IsFalse(Events.IsValid("READY"));
            Assert.IsFalse(Events.IsValid("Collision:Enter"));
        }

        // --- IsValid Non-String Type Safety (Jint marshalling) ---

        [Test]
        public void IsValidReturnsFalseForIntegerArgument()
        {
            Assert.IsFalse(Events.IsValid((object)42));
        }

        [Test]
        public void IsValidReturnsFalseForBooleanArgument()
        {
            Assert.IsFalse(Events.IsValid((object)true));
        }

        [Test]
        public void IsValidReturnsFalseForObjectArgument()
        {
            Assert.IsFalse(Events.IsValid(new object()));
        }

        [Test]
        public void IsValidReturnsTrueForStringObjectArgument()
        {
            Assert.IsTrue(Events.IsValid((object)"spawn"));
        }
    }
}
