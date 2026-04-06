// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;

using Newtonsoft.Json;
using FiveSQD.WebVerse.VOSSynchronization;

namespace FiveSQD.WebVerse.VOSSynchronization.Tests
{
    [TestFixture]
    public class VOSSynchronizationMessageTests
    {
        // ─── SerializableVector2 ───

        [Test]
        public void SerializableVector2_Construction_SetsFields()
        {

            var sv = new VOSSynchronizationMessages.SerializableVector2(new Vector2(3.5f, 7.2f));
            Assert.AreEqual(3.5f, sv.x, 0.001f);
            Assert.AreEqual(7.2f, sv.y, 0.001f);
        }

        [Test]
        public void SerializableVector2_ToVector2_Converts()
        {

            var sv = new VOSSynchronizationMessages.SerializableVector2(new Vector2(1f, 2f));
            Vector2 result = sv.ToVector2();
            Assert.AreEqual(1f, result.x, 0.001f);
            Assert.AreEqual(2f, result.y, 0.001f);
        }

        [Test]
        public void SerializableVector2_JsonRoundTrip_PreservesValues()
        {

            var original = new VOSSynchronizationMessages.SerializableVector2(new Vector2(5.5f, -3.1f));
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SerializableVector2>(json);
            Assert.AreEqual(original.x, deserialized.x, 0.001f);
            Assert.AreEqual(original.y, deserialized.y, 0.001f);
        }

        // ─── SerializableVector3 ───

        [Test]
        public void SerializableVector3_Construction_SetsFields()
        {

            var sv = new VOSSynchronizationMessages.SerializableVector3(new Vector3(1f, 2f, 3f));
            Assert.AreEqual(1f, sv.x, 0.001f);
            Assert.AreEqual(2f, sv.y, 0.001f);
            Assert.AreEqual(3f, sv.z, 0.001f);
        }

        [Test]
        public void SerializableVector3_ToVector3_Converts()
        {

            var sv = new VOSSynchronizationMessages.SerializableVector3(new Vector3(4f, 5f, 6f));
            Vector3 result = sv.ToVector3();
            Assert.AreEqual(4f, result.x, 0.001f);
            Assert.AreEqual(5f, result.y, 0.001f);
            Assert.AreEqual(6f, result.z, 0.001f);
        }

        [Test]
        public void SerializableVector3_JsonRoundTrip_PreservesValues()
        {

            var original = new VOSSynchronizationMessages.SerializableVector3(new Vector3(10f, -20f, 30.5f));
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SerializableVector3>(json);
            Assert.AreEqual(original.x, deserialized.x, 0.001f);
            Assert.AreEqual(original.y, deserialized.y, 0.001f);
            Assert.AreEqual(original.z, deserialized.z, 0.001f);
        }

        [Test]
        public void SerializableVector3_ZeroVector_RoundTrips()
        {

            var original = new VOSSynchronizationMessages.SerializableVector3(Vector3.zero);
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SerializableVector3>(json);
            Assert.AreEqual(0f, deserialized.x, 0.001f);
            Assert.AreEqual(0f, deserialized.y, 0.001f);
            Assert.AreEqual(0f, deserialized.z, 0.001f);
        }

        // ─── SerializableQuaternion ───

        [Test]
        public void SerializableQuaternion_Construction_SetsFields()
        {

            var sq = new VOSSynchronizationMessages.SerializableQuaternion(new Quaternion(0.1f, 0.2f, 0.3f, 0.9f));
            Assert.AreEqual(0.1f, sq.x, 0.001f);
            Assert.AreEqual(0.2f, sq.y, 0.001f);
            Assert.AreEqual(0.3f, sq.z, 0.001f);
            Assert.AreEqual(0.9f, sq.w, 0.001f);
        }

        [Test]
        public void SerializableQuaternion_ToQuaternion_Converts()
        {

            var sq = new VOSSynchronizationMessages.SerializableQuaternion(Quaternion.identity);
            Quaternion result = sq.ToQuaternion();
            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(0f, result.y, 0.001f);
            Assert.AreEqual(0f, result.z, 0.001f);
            Assert.AreEqual(1f, result.w, 0.001f);
        }

        [Test]
        public void SerializableQuaternion_JsonRoundTrip_PreservesValues()
        {

            var original = new VOSSynchronizationMessages.SerializableQuaternion(new Quaternion(0.5f, 0.5f, 0.5f, 0.5f));
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SerializableQuaternion>(json);
            Assert.AreEqual(original.x, deserialized.x, 0.001f);
            Assert.AreEqual(original.y, deserialized.y, 0.001f);
            Assert.AreEqual(original.z, deserialized.z, 0.001f);
            Assert.AreEqual(original.w, deserialized.w, 0.001f);
        }

        // ─── ClientInfo ───

        [Test]
        public void ClientInfo_Construction_SetsFields()
        {

            Guid id = Guid.NewGuid();
            var ci = new VOSSynchronizationMessages.ClientInfo(id, "player1");
            Assert.AreEqual(id.ToString(), ci.id);
            Assert.AreEqual("player1", ci.tag);
        }

        [Test]
        public void ClientInfo_JsonRoundTrip_PreservesValues()
        {

            Guid id = Guid.NewGuid();
            var original = new VOSSynchronizationMessages.ClientInfo(id, "testClient");
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.ClientInfo>(json);
            Assert.AreEqual(original.id, deserialized.id);
            Assert.AreEqual(original.tag, deserialized.tag);
        }

        [Test]
        public void ClientInfo_JsonPropertyNames_AreKebabCase()
        {

            var ci = new VOSSynchronizationMessages.ClientInfo(Guid.NewGuid(), "test");
            string json = JsonConvert.SerializeObject(ci);
            StringAssert.Contains("\"id\"", json);
            StringAssert.Contains("\"tag\"", json);
        }

        // ─── EntityInfo ───

        [Test]
        public void EntityInfo_JsonPropertyNames_AreKebabCase()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"tag\":\"entity-tag\",\"type\":\"container\",\"scale\":null,\"size\":null}";
            var ei = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            ei.position = new VOSSynchronizationMessages.SerializableVector3(Vector3.one);
            ei.rotation = new VOSSynchronizationMessages.SerializableQuaternion(Quaternion.identity);
            ei.scale = new VOSSynchronizationMessages.SerializableVector3(Vector3.one);
            string json = JsonConvert.SerializeObject(ei);
            StringAssert.Contains("\"id\"", json);
            StringAssert.Contains("\"tag\"", json);
            StringAssert.Contains("\"type\"", json);
            StringAssert.Contains("\"position\"", json);
            StringAssert.Contains("\"rotation\"", json);
        }

        [Test]
        public void EntityInfo_JsonRoundTrip_PreservesValues()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"tag\":\"my-entity\",\"type\":\"mesh\",\"sub-type\":\"cube\",\"path\":\"/models/cube.glb\",\"text\":\"hello\",\"font-size\":14,\"mass\":5.0,\"scale\":null,\"size\":null}";
            var original = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            original.position = new VOSSynchronizationMessages.SerializableVector3(new Vector3(1, 2, 3));
            original.rotation = new VOSSynchronizationMessages.SerializableQuaternion(Quaternion.identity);
            original.scale = new VOSSynchronizationMessages.SerializableVector3(Vector3.one);

            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(json);

            Assert.AreEqual(original.id, deserialized.id);
            Assert.AreEqual(original.tag, deserialized.tag);
            Assert.AreEqual(original.type, deserialized.type);
            Assert.AreEqual(original.subType, deserialized.subType);
            Assert.AreEqual(original.path, deserialized.path);
            Assert.AreEqual(original.position.x, deserialized.position.x, 0.001f);
            Assert.AreEqual(original.mass, deserialized.mass, 0.001f);
            Assert.AreEqual(original.text, deserialized.text);
            Assert.AreEqual(original.fontSize, deserialized.fontSize);
        }

        [Test]
        public void EntityInfo_WithNullFields_SerializesGracefully()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"scale\":null,\"size\":null}";
            var ei = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            string json = JsonConvert.SerializeObject(ei);
            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);
        }

        [Test]
        public void EntityInfo_WithArrayFields_RoundTrips()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"scale\":null,\"size\":null}";
            var original = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            original.resources = new string[] { "res1.png", "res2.png" };
            original.diffuseTextures = new string[] { "tex1.png" };
            original.metallicValues = new float[] { 0.5f, 0.8f };

            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(json);

            Assert.AreEqual(2, deserialized.resources.Length);
            Assert.AreEqual("res1.png", deserialized.resources[0]);
            Assert.AreEqual(1, deserialized.diffuseTextures.Length);
            Assert.AreEqual(2, deserialized.metallicValues.Length);
            Assert.AreEqual(0.5f, deserialized.metallicValues[0], 0.001f);
        }

        // ─── TerrainModification ───

        [Test]
        public void TerrainModification_Construction_SetsFields()
        {

            var tm = new VOSSynchronizationMessages.TerrainModification(
                "raise", Vector3.one, "circle", 0, 5.0f);
            Assert.AreEqual("raise", tm.modification);
            Assert.AreEqual(1f, tm.position.x, 0.001f);
            Assert.AreEqual("circle", tm.brushType);
            Assert.AreEqual(0, tm.layer);
            Assert.AreEqual(5.0f, tm.size, 0.001f);
        }

        [Test]
        public void TerrainModification_JsonRoundTrip_PreservesValues()
        {

            var original = new VOSSynchronizationMessages.TerrainModification(
                "lower", new Vector3(10, 0, 10), "square", 1, 3.0f);
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.TerrainModification>(json);
            Assert.AreEqual(original.modification, deserialized.modification);
            Assert.AreEqual(original.position.x, deserialized.position.x, 0.001f);
            Assert.AreEqual(original.brushType, deserialized.brushType);
            Assert.AreEqual(original.layer, deserialized.layer);
            Assert.AreEqual(original.size, deserialized.size, 0.001f);
        }

        [Test]
        public void TerrainModification_JsonPropertyNames_AreKebabCase()
        {

            var tm = new VOSSynchronizationMessages.TerrainModification(
                "paint", Vector3.zero, "soft", 0, 1.0f);
            string json = JsonConvert.SerializeObject(tm);
            StringAssert.Contains("\"modification\"", json);
            StringAssert.Contains("\"position\"", json);
            StringAssert.Contains("\"brush-type\"", json);
            StringAssert.Contains("\"layer\"", json);
            StringAssert.Contains("\"size\"", json);
        }

        // ─── Session Messages ───

        [Test]
        public void CreateSessionMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.CreateSessionMessage(
                msgId, "client-123", "token-abc", sessId, "test-session");
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual("client-123", msg.clientID);
            Assert.AreEqual("token-abc", msg.clientToken);
            Assert.AreEqual(sessId.ToString(), msg.sessionID);
            Assert.AreEqual("test-session", msg.sessionTag);
        }

        [Test]
        public void CreateSessionMessage_JsonRoundTrip_PreservesValues()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var original = new VOSSynchronizationMessages.SessionMessages.CreateSessionMessage(
                msgId, "c1", "t1", sessId, "sess-tag");
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SessionMessages.CreateSessionMessage>(json);
            Assert.AreEqual(original.messageID, deserialized.messageID);
            Assert.AreEqual(original.clientID, deserialized.clientID);
            Assert.AreEqual(original.sessionID, deserialized.sessionID);
            Assert.AreEqual(original.sessionTag, deserialized.sessionTag);
        }

        [Test]
        public void CreateSessionMessage_JsonPropertyNames_AreKebabCase()
        {

            var msg = new VOSSynchronizationMessages.SessionMessages.CreateSessionMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid(), "s");
            string json = JsonConvert.SerializeObject(msg);
            StringAssert.Contains("\"message-id\"", json);
            StringAssert.Contains("\"client-id\"", json);
            StringAssert.Contains("\"client-token\"", json);
            StringAssert.Contains("\"session-id\"", json);
            StringAssert.Contains("\"session-tag\"", json);
        }

        [Test]
        public void DestroySessionMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.DestroySessionMessage(
                msgId, "client-1", "token-1", sessId);
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(sessId.ToString(), msg.sessionID);
        }

        [Test]
        public void DestroySessionMessage_JsonRoundTrip()
        {

            var original = new VOSSynchronizationMessages.SessionMessages.DestroySessionMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid());
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SessionMessages.DestroySessionMessage>(json);
            Assert.AreEqual(original.messageID, deserialized.messageID);
            Assert.AreEqual(original.sessionID, deserialized.sessionID);
        }

        [Test]
        public void NewSessionMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.NewSessionMessage(
                msgId, sessId, "new-sess");
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(sessId.ToString(), msg.sessionID);
            Assert.AreEqual("new-sess", msg.sessionTag);
        }

        [Test]
        public void JoinSessionMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.JoinSessionMessage(
                msgId, sessId, "client-2", "token-2", "player-tag");
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(sessId.ToString(), msg.sessionID);
            Assert.AreEqual("client-2", msg.clientID);
            Assert.AreEqual("player-tag", msg.clientTag);
        }

        [Test]
        public void JoinSessionMessage_Serialization_ContainsExpectedFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.JoinSessionMessage(
                msgId, sessId, "c", "t", "tag");
            string json = JsonConvert.SerializeObject(msg);
            StringAssert.Contains(msgId.ToString(), json);
            StringAssert.Contains(sessId.ToString(), json);
            StringAssert.Contains("\"client-tag\"", json);
            StringAssert.Contains("tag", json);
        }

        [Test]
        public void ClientHeartbeatMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.SessionMessages.ClientHeartbeatMessage(
                msgId, sessId, "client-hb", "token-hb");
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(sessId.ToString(), msg.sessionID);
            Assert.AreEqual("client-hb", msg.clientID);
            Assert.AreEqual("token-hb", msg.clientToken);
        }

        [Test]
        public void GetSessionStateMessage_JsonRoundTrip()
        {

            var original = new VOSSynchronizationMessages.SessionMessages.GetSessionStateMessage(
                Guid.NewGuid(), Guid.NewGuid(), "c1", "t1");
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.SessionMessages.GetSessionStateMessage>(json);
            Assert.AreEqual(original.messageID, deserialized.messageID);
            Assert.AreEqual(original.sessionID, deserialized.sessionID);
        }

        // ─── Request Messages ───

        [Test]
        public void RemoveEntityMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.RemoveEntityMessage(
                msgId, "c1", "t1", sessId, entityId);
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(entityId.ToString(), msg.id);
        }

        [Test]
        public void RemoveEntityMessage_JsonRoundTrip()
        {

            Guid sessId = Guid.NewGuid();
            var original = new VOSSynchronizationMessages.RequestMessages.RemoveEntityMessage(
                Guid.NewGuid(), "c", "t", sessId, Guid.NewGuid());
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.RequestMessages.RemoveEntityMessage>(json);
            Assert.AreEqual(original.messageID, deserialized.messageID);
            Assert.AreEqual(original.id, deserialized.id);
        }

        [Test]
        public void DeleteEntityMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.DeleteEntityMessage(
                msgId, "c1", "t1", sessId, entityId);
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(entityId.ToString(), msg.id);
        }

        [Test]
        public void UpdateEntityPositionMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.UpdateEntityPositionMessage(
                msgId, "c1", "t1", sessId, entityId, new Vector3(1, 2, 3));
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(entityId.ToString(), msg.id);
            Assert.AreEqual(1f, msg.position.x, 0.001f);
            Assert.AreEqual(2f, msg.position.y, 0.001f);
            Assert.AreEqual(3f, msg.position.z, 0.001f);
        }

        [Test]
        public void UpdateEntityPositionMessage_JsonRoundTrip()
        {

            var original = new VOSSynchronizationMessages.RequestMessages.UpdateEntityPositionMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid(), Guid.NewGuid(), new Vector3(10, 20, 30));
            string json = JsonConvert.SerializeObject(original);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.RequestMessages.UpdateEntityPositionMessage>(json);
            Assert.AreEqual(original.position.x, deserialized.position.x, 0.001f);
            Assert.AreEqual(original.position.y, deserialized.position.y, 0.001f);
            Assert.AreEqual(original.position.z, deserialized.position.z, 0.001f);
        }

        [Test]
        public void UpdateEntityRotationMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            Quaternion rot = Quaternion.Euler(45, 90, 0);
            var msg = new VOSSynchronizationMessages.RequestMessages.UpdateEntityRotationMessage(
                msgId, "c1", "t1", sessId, entityId, rot);
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(rot.x, msg.rotation.x, 0.001f);
            Assert.AreEqual(rot.y, msg.rotation.y, 0.001f);
        }

        [Test]
        public void UpdateEntityScaleMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.UpdateEntityScaleMessage(
                msgId, "c1", "t1", sessId, entityId, new Vector3(2, 3, 4));
            Assert.AreEqual(2f, msg.scale.x, 0.001f);
            Assert.AreEqual(3f, msg.scale.y, 0.001f);
            Assert.AreEqual(4f, msg.scale.z, 0.001f);
        }

        [Test]
        public void SetVisibilityMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.SetVisibilityMessage(
                msgId, "c1", "t1", sessId, entityId, false);
            Assert.AreEqual(msgId.ToString(), msg.messageID);
            Assert.AreEqual(entityId.ToString(), msg.id);
            Assert.IsFalse(msg.visible);
        }

        [Test]
        public void SetInteractionStateMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.SetInteractionStateMessage(
                msgId, "c1", "t1", sessId, entityId, "physical");
            Assert.AreEqual("physical", msg.interactionState);
        }

        [Test]
        public void SetHighlightStateMessage_Construction_SetsFields()
        {

            Guid msgId = Guid.NewGuid();
            Guid sessId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var msg = new VOSSynchronizationMessages.RequestMessages.SetHighlightStateMessage(
                msgId, "c1", "t1", sessId, entityId, true);
            Assert.IsTrue(msg.highlighted);
        }

        // ─── Cross-type Serialization ───

        [Test]
        public void MultipleMessageTypes_SerializeToDistinctJson()
        {

            var create = new VOSSynchronizationMessages.SessionMessages.CreateSessionMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid(), "s");
            var destroy = new VOSSynchronizationMessages.SessionMessages.DestroySessionMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid());
            var remove = new VOSSynchronizationMessages.RequestMessages.RemoveEntityMessage(
                Guid.NewGuid(), "c", "t", Guid.NewGuid(), Guid.NewGuid());

            string createJson = JsonConvert.SerializeObject(create);
            string destroyJson = JsonConvert.SerializeObject(destroy);
            string removeJson = JsonConvert.SerializeObject(remove);

            Assert.AreNotEqual(createJson, destroyJson);
            Assert.AreNotEqual(createJson, removeJson);
            // CreateSession has session-tag, destroy does not.
            StringAssert.Contains("session-tag", createJson);
            Assert.IsFalse(destroyJson.Contains("session-tag"));
        }

        [Test]
        public void EntityInfo_WithTerrainModifications_RoundTrips()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"type\":\"terrain\",\"length\":100.0,\"scale\":null,\"size\":null}";
            var ei = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            ei.width = 100f;
            ei.height = 50f;
            ei.modifications = new VOSSynchronizationMessages.TerrainModification[]
            {
                new VOSSynchronizationMessages.TerrainModification("raise", Vector3.one, "circle", 0, 5f),
                new VOSSynchronizationMessages.TerrainModification("lower", Vector3.zero, "square", 1, 3f)
            };

            string json = JsonConvert.SerializeObject(ei);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(json);

            Assert.AreEqual(2, deserialized.modifications.Length);
            Assert.AreEqual("raise", deserialized.modifications[0].modification);
            Assert.AreEqual("lower", deserialized.modifications[1].modification);
            Assert.AreEqual(100f, deserialized.length, 0.001f);
        }

        [Test]
        public void EntityInfo_WithNestedVectors_RoundTrips()
        {

            string seedJson = "{\"id\":\"" + Guid.NewGuid().ToString() + "\",\"scale\":null,\"size\":null}";
            var ei = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(seedJson);
            ei.position = new VOSSynchronizationMessages.SerializableVector3(new Vector3(1, 2, 3));
            ei.rotation = new VOSSynchronizationMessages.SerializableQuaternion(Quaternion.Euler(45, 90, 0));
            ei.scale = new VOSSynchronizationMessages.SerializableVector3(new Vector3(2, 2, 2));
            ei.modelOffset = new VOSSynchronizationMessages.SerializableVector3(new Vector3(0, 1, 0));
            ei.labelOffset = new VOSSynchronizationMessages.SerializableVector3(new Vector3(0, 2, 0));

            string json = JsonConvert.SerializeObject(ei);
            var deserialized = JsonConvert.DeserializeObject<VOSSynchronizationMessages.EntityInfo>(json);

            Assert.AreEqual(1f, deserialized.position.x, 0.001f);
            Assert.AreEqual(2f, deserialized.scale.x, 0.001f);
            Assert.AreEqual(1f, deserialized.modelOffset.y, 0.001f);
            Assert.AreEqual(2f, deserialized.labelOffset.y, 0.001f);
        }
    }
}
