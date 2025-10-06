// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    public class HTMLEntity : BaseEntity
    {
        /// <summary>
        /// Create an HTML entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="isSize">Whether or not the scale parameter is a size.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// HTML entity object.</param>
        /// <returns>The ID of the HTML entity object.</returns>
        public static HTMLEntity Create(BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize = false,
            string id = null, string tag = null, string onMessage = null, string onLoaded = null)
        {
            Guid guid;
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = Guid.Parse(id);
            }

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
            UnityEngine.Vector3 scl = new UnityEngine.Vector3(scale.x, scale.y, scale.z);

            HTMLEntity he = new HTMLEntity(false);

            Action<string> onMessageAction = null;
            if (onMessage != null)
            {
                onMessageAction = (msg) =>
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onMessage, new object[] { msg });
                };
            }

            Action onLoadAction = null;
            onLoadAction = () =>
            {
                he.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(he.internalEntity, he);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { he });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLEntity(pBE, pos, rot, scl, guid, isSize, tag,
                onMessageAction, onLoadAction);

            return he;
        }

        /// <summary>
        /// Create an HTML entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="positionPercent">Position of the entity as a percentage of its parent canvas.</param>
        /// <param name="sizePercent">Size of the entity as a percentage of its parent canvas.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// HTML entity object.</param>
        /// <returns>The ID of the HTML entity object.</returns>
        public static HTMLEntity Create(BaseEntity parent, Vector2 positionPercent, Vector2 sizePercent,
            string id = null, string tag = null, string onMessage = null, string onLoaded = null)
        {
            Guid guid;
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = Guid.Parse(id);
            }

            StraightFour.Entity.CanvasEntity pCE = (StraightFour.Entity.CanvasEntity) EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[HTMLEntity->Create] Invalid parent entity.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 scl = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            HTMLEntity he = new HTMLEntity(true);

            Action<string> onMessageAction = null;
            if (onMessage != null)
            {
                onMessageAction = (msg) =>
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onMessage, new object[] { msg });
                };
            }

            Action onLoadAction = null;
            onLoadAction = () =>
            {
                he.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(he.internalEntity, he);

                WebVerseRuntime.Instance.outputManager.RegisterScreenSizeChangeAction(new Action<int, int>((width, height) =>
                {
                    ((StraightFour.Entity.HTMLUIElementEntity) he.internalEntity).CorrectSizeAndPosition(width, height);
                }));

                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { he });
                }
            };
            
            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLUIElementEntity(pCE, pos, scl, guid, tag,
                onMessageAction, onLoadAction);

            return he;
        }

        /// <summary>
        /// Create an HTML entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the HTML entity configuration.</param>
        /// <param name="parent">Parent entity for the HTML entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created HTML entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, htmlEntity) =>
            {
                if (!success || htmlEntity == null ||
                    !(htmlEntity is StraightFour.Entity.HTMLEntity ||
                    htmlEntity is StraightFour.Entity.HTMLUIElementEntity))
                {
                    Logging.LogError("[HTMLEntity:Create] Error loading HTML entity from JSON.");
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { null });
                    }
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        if (htmlEntity is StraightFour.Entity.HTMLEntity)
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                onLoaded, new object[] { EntityAPIHelper.GetPublicEntity(
                                    (StraightFour.Entity.HTMLEntity) htmlEntity) });
                        }
                        else if (htmlEntity is StraightFour.Entity.HTMLUIElementEntity)
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                onLoaded, new object[] { EntityAPIHelper.GetPublicEntity(
                                    (StraightFour.Entity.HTMLUIElementEntity) htmlEntity) });
                        }
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadHTMLEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal HTMLEntity(bool canvasEntity)
        {
            if (canvasEntity)
            {
                internalEntityType = typeof(StraightFour.Entity.HTMLUIElementEntity);
            }
            else
            {
                internalEntityType = typeof(StraightFour.Entity.HTMLEntity);
            }
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to load content from.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool LoadFromURL(string url)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[HTMLEntity:LoadFromURL] Unknown entity.");
                return false;
            }

            if (internalEntity is StraightFour.Entity.HTMLEntity)
            {
                return ((StraightFour.Entity.HTMLEntity) internalEntity).LoadFromURL(
                    VEML.VEMLUtilities.FullyQualifyURI(url, WebVerseRuntime.Instance.currentBasePath));
            }
            else if (internalEntity is StraightFour.Entity.HTMLUIElementEntity)
            {
                return ((StraightFour.Entity.HTMLUIElementEntity) internalEntity).LoadFromURL(
                    VEML.VEMLUtilities.FullyQualifyURI(url, WebVerseRuntime.Instance.currentBasePath));
            }
            else
            {
                Logging.LogError("[HTMLEntity:LoadFromURL] Invalid internal entity.");
                return false;
            }
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to load content from.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool LoadHTML(string html)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[HTMLEntity:LoadHTML] Unknown entity.");
                return false;
            }

            if (internalEntity is StraightFour.Entity.HTMLEntity)
            {
                return ((StraightFour.Entity.HTMLEntity) internalEntity).LoadHTML(html);
            }
            else if (internalEntity is StraightFour.Entity.HTMLUIElementEntity)
            {
                return ((StraightFour.Entity.HTMLUIElementEntity) internalEntity).LoadHTML(html);
            }
            else
            {
                Logging.LogError("[HTMLEntity:LoadHTML] Invalid internal entity.");
                return false;
            }
        }

        /// <summary>
        /// Get the current URL.
        /// </summary>
        /// <returns>The current URL, or null.</returns>
        public string GetURL()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[HTMLEntity:GetURL] Unknown entity.");
                return null;
            }

            if (internalEntity is StraightFour.Entity.HTMLEntity)
            {
                return ((StraightFour.Entity.HTMLEntity) internalEntity).GetURL();
            }
            else if (internalEntity is StraightFour.Entity.HTMLUIElementEntity)
            {
                return ((StraightFour.Entity.HTMLUIElementEntity) internalEntity).GetURL();
            }
            else
            {
                Logging.LogError("[HTMLEntity:GetURL] Invalid internal entity.");
                return null;
            }
        }

        /// <summary>
        /// Execute JavaScript logic.
        /// </summary>
        /// <param name="logic">Logic to execute.</param>
        /// <param name="onComplete">Action to invoke upon completion. Provides return
        /// from JavaScript as string.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ExecuteJavaScript(string logic, string onComplete)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[HTMLEntity:ExecuteJavaScript] Unknown entity.");
                return false;
            }

            Action<string> onCompleteAction = null;
            if (!string.IsNullOrEmpty(onComplete))
            {
                onCompleteAction = (result) =>
                {
                    if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onComplete, new object[] { result });
                    }
                };
            }
            
            if (internalEntity is StraightFour.Entity.HTMLEntity)
            {
                return ((StraightFour.Entity.HTMLEntity) internalEntity).ExecuteJavaScript(logic, onCompleteAction);
            }
            else if (internalEntity is StraightFour.Entity.HTMLUIElementEntity)
            {
                return ((StraightFour.Entity.HTMLUIElementEntity) internalEntity).ExecuteJavaScript(logic, onCompleteAction);
            }
            else
            {
                Logging.LogError("[HTMLEntity:ExecuteJavaScript] Invalid internal entity.");
                return false;
            }
        }
    }
}