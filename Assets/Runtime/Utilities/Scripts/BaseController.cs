// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Utilities
{
    /// <summary>
    /// Base class for a controller.
    /// </summary>
    public class BaseController : MonoBehaviour
    {
        /// <summary>
        /// Initialize the controller.
        /// </summary>
        public virtual void Initialize()
        {
            Logging.Log("[" + GetType().Name + "] Initialized.");
        }

        /// <summary>
        /// Terminate the controller.
        /// </summary>
        public virtual void Terminate()
        {
            Logging.Log("[" + GetType().Name + "] Terminated.");
        }
    }
}