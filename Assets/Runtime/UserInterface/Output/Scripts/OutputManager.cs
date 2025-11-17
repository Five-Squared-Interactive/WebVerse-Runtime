// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Output
{
    /// <summary>
    /// Class for the Output Manager.
    /// </summary>
    public class OutputManager : BaseManager
    {
        public float screenCheckPeriod = 0.1f;

        private List<Action<int, int>> screenSizeChangeActions;

        private int screenHeight;

        private int screenWidth;

        private float screenUpdateTimer;

        /// <summary>
        /// Initialize the Output Manager.
        /// </summary>
        public override void Initialize()
        {
            screenSizeChangeActions = new List<Action<int, int>>();
            screenUpdateTimer = 0;

            base.Initialize();
        }

        /// <summary>
        /// Terminate the Output Manager.
        /// </summary>
        public override void Terminate()
        {
            screenSizeChangeActions = null;
            screenUpdateTimer = 0;

            base.Terminate();
        }

        /// <summary>
        /// Register a screen size change action.
        /// </summary>
        /// <param name="screenSizeChangeAction">Action taking int parameters for width and height, which will
        /// be invoked upon screen size change.</param>
        public void RegisterScreenSizeChangeAction(Action<int, int> screenSizeChangeAction)
        {
            if (screenSizeChangeActions == null)
            {
                Logging.LogError("[OutputManager->RegisterScreenSizeChangeActions] Not initialized.");
                return;
            }

            screenSizeChangeActions.Add(screenSizeChangeAction);
        }

        /// <summary>
        /// UnRegister a screen size change action.
        /// </summary>
        /// <param name="screenSizeChangeAction">Action to unregister.</param>
        public void UnRegisterScreenSizeChangeAction(Action<int, int> screenSizeChangeAction)
        {
            if (screenSizeChangeActions == null)
            {
                Logging.LogError("[OutputManager->UnRegisterScreenSizeChangeActions] Not initialized.");
                return;
            }

            screenSizeChangeActions.Remove(screenSizeChangeAction);
        }

        private void Update()
        {
            screenUpdateTimer += Time.deltaTime;
            if (screenUpdateTimer > screenCheckPeriod)
            {
                screenUpdateTimer = 0;
                int currScreenWidth = Screen.width;
                int currScreenHeight = Screen.height;
                if (currScreenWidth != screenWidth || currScreenHeight != screenHeight)
                {
                    screenWidth = currScreenWidth;
                    screenHeight = currScreenHeight;

                    if (screenSizeChangeActions != null)
                    {
                        foreach (Action<int, int> screenSizeChangeAction in screenSizeChangeActions)
                        {
                            screenSizeChangeAction.Invoke(currScreenWidth, currScreenHeight);
                        }
                    }
                }
            }
        }
    }
}