// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using Jint;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FiveSQD.WebVerse.Handlers.Javascript
{
    /// <summary>
    /// Class for the JavaScript Handler.
    /// </summary>
    public class JavascriptHandler : BaseHandler
    {
        /// <summary>
        /// A struct for an execution task.
        /// </summary>
        private class ExecutionTask
        {
            /// <summary>
            /// Logic to execute.
            /// </summary>
            public string logic;

            /// <summary>
            /// Milliseconds remaining before executing.
            /// </summary>
            public int millisecondsRemaining;

            /// <summary>
            /// Action to invoke upon completion.
            /// </summary>
            public Action<object> onComplete;

            /// <summary>
            /// Execution task.
            /// </summary>
            /// <param name="logic">Logic to execute.</param>
            /// <param name="millisecondsRemaining">Milliseconds remaining before executing.</param>
            /// <param name="onComplete">Action to invoke upon completion.</param>
            public ExecutionTask(string logic, int millisecondsRemaining, Action<object> onComplete = null)
            {
                this.logic = logic;
                this.millisecondsRemaining = millisecondsRemaining;
                this.onComplete = onComplete;
            }
        }

        /// <summary>
        /// Tuple of class names and internal types.
        /// </summary>
        private static readonly System.Tuple<string, System.Type>[] apis = new System.Tuple<string, System.Type>[]
        {
            // World Types.
            new System.Tuple<string, System.Type>("Color", typeof(Color)),
            new System.Tuple<string, System.Type>("RaycastHitInfo", typeof(RaycastHitInfo)),
            new System.Tuple<string, System.Type>("Vector2", typeof(Vector2)),
            new System.Tuple<string, System.Type>("Vector2D", typeof(Vector2D)),
            new System.Tuple<string, System.Type>("Vector2Int", typeof(Vector2Int)),
            new System.Tuple<string, System.Type>("Vector3", typeof(Vector3)),
            new System.Tuple<string, System.Type>("Vector3D", typeof(Vector3D)),
            new System.Tuple<string, System.Type>("Vector3Int", typeof(Vector3Int)),
            new System.Tuple<string, System.Type>("Vector4", typeof(Vector4)),
            new System.Tuple<string, System.Type>("Vector4D", typeof(Vector4D)),
            new System.Tuple<string, System.Type>("Vector4Int", typeof(Vector4Int)),
            new System.Tuple<string, System.Type>("Quaternion", typeof(Quaternion)),
            new System.Tuple<string, System.Type>("QuaternionD", typeof(QuaternionD)),
            new System.Tuple<string, System.Type>("UUID", typeof(UUID)),

            // Entity.
            new System.Tuple<string, System.Type>("Entity", typeof(Entity)),
            new System.Tuple<string, System.Type>("AirplaneEntity", typeof(AirplaneEntity)),
            new System.Tuple<string, System.Type>("AudioEntity", typeof(AudioEntity)),
            new System.Tuple<string, System.Type>("AutomobileEntity", typeof(AutomobileEntity)),
            new System.Tuple<string, System.Type>("AutomobileEntityWheel", typeof(AutomobileEntityWheel)),
            new System.Tuple<string, System.Type>("AutomobileType", typeof(AutomobileEntity.AutomobileType)),
            new System.Tuple<string, System.Type>("ButtonEntity", typeof(ButtonEntity)),
            new System.Tuple<string, System.Type>("CanvasEntity", typeof(CanvasEntity)),
            new System.Tuple<string, System.Type>("CharacterEntity", typeof(CharacterEntity)),
            new System.Tuple<string, System.Type>("ContainerEntity", typeof(ContainerEntity)),
            new System.Tuple<string, System.Type>("HTMLEntity", typeof(HTMLEntity)),
            new System.Tuple<string, System.Type>("ImageEntity", typeof(ImageEntity)),
            new System.Tuple<string, System.Type>("InputEntity", typeof(InputEntity)),
            new System.Tuple<string, System.Type>("LightEntity", typeof(LightEntity)),
            new System.Tuple<string, System.Type>("MeshEntity", typeof(MeshEntity)),
            new System.Tuple<string, System.Type>("TerrainEntity", typeof(TerrainEntity)),
            new System.Tuple<string, System.Type>("TerrainEntityBrushType", typeof(TerrainEntityBrushType)),
            new System.Tuple<string, System.Type>("TerrainEntityLayer", typeof(TerrainEntityLayer)),
            new System.Tuple<string, System.Type>("TerrainEntityLayerMask", typeof(TerrainEntityLayerMask)),
            new System.Tuple<string, System.Type>("TerrainEntityLayerMaskCollection", typeof(TerrainEntityLayerMaskCollection)),
            new System.Tuple<string, System.Type>("TerrainEntityModification", typeof(TerrainEntityModification)),
            new System.Tuple<string, System.Type>("TerrainEntityOperation", typeof(TerrainEntityModification.TerrainEntityOperation)),
            new System.Tuple<string, System.Type>("TextAlignment", typeof(TextAlignment)),
            new System.Tuple<string, System.Type>("TextEntity", typeof(TextEntity)),
            new System.Tuple<string, System.Type>("TextWrapping", typeof(TextWrapping)),
            new System.Tuple<string, System.Type>("UIElementAlignment", typeof(UIElementAlignment)),
            new System.Tuple<string, System.Type>("VoxelEntity", typeof(VoxelEntity)),
            new System.Tuple<string, System.Type>("WaterBlockerEntity", typeof(WaterBlockerEntity)),
            new System.Tuple<string, System.Type>("WaterEntity", typeof(WaterEntity)),
            new System.Tuple<string, System.Type>("InteractionState", typeof(InteractionState)),
            new System.Tuple<string, System.Type>("EntityMotion", typeof(EntityMotion)),
            new System.Tuple<string, System.Type>("EntityPhysicalProperties", typeof(EntityPhysicalProperties)),
            new System.Tuple<string, System.Type>("LightProperties", typeof(LightProperties)),
            new System.Tuple<string, System.Type>("LightType", typeof(LightType)),
            new System.Tuple<string, System.Type>("VoxelBlockInfo", typeof(VoxelBlockInfo)),
            new System.Tuple<string, System.Type>("VoxelBlockSubType", typeof(VoxelBlockSubType)),

            // Networking.
            new System.Tuple<string, System.Type>("HTTPNetworking", typeof(HTTPNetworking)),
#if USE_WEBINTERFACE
            new System.Tuple<string, System.Type>("MQTTClient", typeof(MQTTClient)),
            new System.Tuple<string, System.Type>("WebSocket", typeof(WebSocket)),
#endif

            // Input.
            new System.Tuple<string, System.Type>("Input", typeof(APIs.Input.Input)),

#if USE_WEBINTERFACE
            // VOS Synchronization.
            new System.Tuple<string, System.Type>("VOSSynchronization", typeof(APIs.VOSSynchronization.VOSSynchronization)),
            new System.Tuple<string, System.Type>("VSSTransport",
                typeof(APIs.VOSSynchronization.VOSSynchronization.Transport)),
#endif

            // Environment.
            new System.Tuple<string, System.Type>("Environment", typeof(APIs.Environment.Environment)),

            // Data.
            new Tuple<string, Type>("AsyncJSON", typeof(APIs.Data.AsyncJSON)),

            // World Browser Utilities.
            new System.Tuple<string, System.Type>("Camera", typeof(APIs.Utilities.Camera)),
            new System.Tuple<string, System.Type>("Context", typeof(APIs.Utilities.Context)),
            new System.Tuple<string, System.Type>("Date", typeof(APIs.Utilities.Date)),
            new System.Tuple<string, System.Type>("LocalStorage", typeof(APIs.Utilities.LocalStorage)),
            new System.Tuple<string, System.Type>("Logging", typeof(APIs.Utilities.Logging)),
            new System.Tuple<string, System.Type>("Scripting", typeof(APIs.Utilities.Scripting)),
            new System.Tuple<string, System.Type>("Time", typeof(APIs.Utilities.Time)),
            new System.Tuple<string, System.Type>("World", typeof(APIs.Utilities.World)),
            new System.Tuple<string, System.Type>("WorldStorage", typeof(APIs.Utilities.WorldStorage))
        };

        /// <summary>
        /// Reference to the JS engine instance.
        /// </summary>
        private Engine engine;

        /// <summary>
        /// Pending scripts to be run and the remaining milliseconds left before running.
        /// </summary>
        private List<ExecutionTask> pendingScriptsToRun;

        /// <summary>
        /// Pending logic to be run and the remaining milliseconds left before running.
        /// </summary>
        private List<ExecutionTask> pendingLogicToRun;

        /// <summary>
        /// Initialize the JavascriptHandler.
        /// </summary>
        public override void Initialize()
        {
            pendingScriptsToRun = new List<ExecutionTask>();
            pendingLogicToRun = new List<ExecutionTask>();

            engine = new Engine();
            RegisterAllAPIs();

            base.Initialize();
            EntityAPIHelper.InitializeEntityMapping();
            EntityAPIHelper.cubeMeshPrefab = Runtime.WebVerseRuntime.Instance.cubeMeshPrefab;
            EntityAPIHelper.capsuleMeshPrefab = Runtime.WebVerseRuntime.Instance.capsuleMeshPrefab;
            EntityAPIHelper.coneMeshPrefab = Runtime.WebVerseRuntime.Instance.coneMeshPrefab;
            EntityAPIHelper.cylinderMeshPrefab = Runtime.WebVerseRuntime.Instance.cylinderMeshPrefab;
            EntityAPIHelper.planeMeshPrefab = Runtime.WebVerseRuntime.Instance.planeMeshPrefab;
            EntityAPIHelper.rectangularPyramidMeshPrefab = Runtime.WebVerseRuntime.Instance.rectangularPyramidMeshPrefab;
            EntityAPIHelper.sphereMeshPrefab = Runtime.WebVerseRuntime.Instance.sphereMeshPrefab;
            EntityAPIHelper.tetrahedronMeshPrefab = Runtime.WebVerseRuntime.Instance.tetrahedronMeshPrefab;
            EntityAPIHelper.torusMeshPrefab = Runtime.WebVerseRuntime.Instance.torusMeshPrefab;
            EntityAPIHelper.prismMeshPrefab = Runtime.WebVerseRuntime.Instance.prismMeshPrefab;
            EntityAPIHelper.archMeshPrefab = Runtime.WebVerseRuntime.Instance.archMeshPrefab;
        }

        public void Reset()
        {
            engine.Advanced.ResetCallStack();
            engine = new Engine();
            RegisterAllAPIs();
            EntityAPIHelper.InitializeEntityMapping();
        }

        /// <summary>
        /// Terminate the JavascriptHandler.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }
        
        /// <summary>
        /// Run a script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        public void RunScript(string script)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptHandler->RunScript] No engine reference.");
                return;
            }

            try
            {
                engine = engine.Execute(script);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Run a script after a specified timeout.
        /// </summary>
        /// <param name="script">The script to run.</param>
        /// <param name="timeout">The timeout after which to run the script.</param>
        public void RunScriptAfterTimeout(string script, int timeout)
        {
            pendingScriptsToRun.Add(new ExecutionTask(script, timeout));
        }

        /// <summary>
        /// Run logic.
        /// </summary>
        /// <param name="logic">The logic to run.</param>
        /// <returns>The engine that is running the logic.</returns>
        public object Run(string logic)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->Run] No engine reference.");
                return null;
            }

            object result = null;
            try
            {
                result = engine.Evaluate(logic);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }

            return result;
        }

        /// <summary>
        /// Run logic after a specified timeout.
        /// </summary>
        /// <param name="logic">The logic to run.</param>
        /// <param name="timeout">The timeout after which to run the logic.</param>
        /// <param name="onComplete">Action to invoke upon completion.</param>
        public void RunAfterTimeout(string logic, int timeout, Action<object> onComplete)
        {
            pendingLogicToRun.Add(new ExecutionTask(logic, timeout, onComplete));
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, bool value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, byte value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, byte[] value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, float value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, double value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, int value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <param name="value">The value to set the variable to.</param>
        public void SetValue(string variableName, string value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        public void SetValue(string variableName, object value)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->SetValue] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(variableName, value);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        public void CallWithParams(string functionName, object[] parameters)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->CallWithParams] No engine reference.");
                return;
            }

            try
            {
                System.Collections.Generic.List<Jint.Native.JsValue> values
                    = new System.Collections.Generic.List<Jint.Native.JsValue>();

                foreach (object parameter in parameters)
                {
                    values.Add(Jint.Native.JsValue.FromObject(engine, parameter));
                }
                engine.Invoke(functionName, values.ToArray());
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        /// <returns>The value of the variable, or null.</returns>
        public object GetValue(string variableName)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->GetValue] No engine reference.");
                return null;
            }

            try
            {
                return engine.GetValue(variableName);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
                return null;
            }
        }

        /// <summary>
        /// Set a context.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="context">Context.</param>
        public void DefineContext(string contextName, object context)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->DefineContext] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue("INTERNAL_CONTEXTS_" + contextName, context);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        /// <summary>
        /// Get a context.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <returns>Context.</returns>
        public object GetContext(string contextName)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptManager->DefineContext] No engine reference.");
                return null;
            }

            try
            {
                return engine.GetValue("INTERNAL_CONTEXTS_" + contextName);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
                return null;
            }
        }

        /// <summary>
        /// Register the APIs.
        /// </summary>
        private void RegisterAllAPIs()
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptHandler->RegisterAllAPIs] No engine reference.");
                return;
            }

            foreach (System.Tuple<string, System.Type> api in apis)
            {
                RegisterAPI(api.Item1, api.Item2);
            }
        }

        /// <summary>
        /// Register an API.
        /// </summary>
        private void RegisterAPI(string name, System.Type type)
        {
            if (engine == null)
            {
                LogSystem.LogError("[JavascriptHandler->RegisterAPI] No engine reference.");
                return;
            }

            try
            {
                engine.SetValue(name, type);
            }
            catch (System.Exception e)
            {
                LogSystem.LogError("[Exception Caught] " + e);
            }
        }

        private void Update()
        {
            int elapsedTime = (int) (UnityEngine.Time.deltaTime * 1000);

            if (pendingScriptsToRun != null)
            {
                foreach (ExecutionTask pendingScriptToRun in pendingScriptsToRun.ToList())
                {
                    pendingScriptToRun.millisecondsRemaining -= elapsedTime;
                    if (pendingScriptToRun.millisecondsRemaining <= 0)
                    {
                        RunScript(pendingScriptToRun.logic);
                        pendingScriptsToRun.Remove(pendingScriptToRun);
                    }
                }
            }

            if (pendingLogicToRun != null)
            {
                foreach (ExecutionTask pendingLogic in pendingLogicToRun.ToList())
                {
                    pendingLogic.millisecondsRemaining -= elapsedTime;
                    if (pendingLogic.millisecondsRemaining <= 0)
                    {
                        object result = Run(pendingLogic.logic);
                        if (pendingLogic.onComplete != null)
                        {
                            pendingLogic.onComplete.Invoke(result);
                        }
                        pendingLogicToRun.Remove(pendingLogic);
                    }
                }
            }
        }
    }
}