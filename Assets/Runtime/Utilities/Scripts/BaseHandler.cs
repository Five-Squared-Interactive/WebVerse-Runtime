// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Utilities
{
    /// <summary>
    /// Base class for a handler.
    /// </summary>
    public class BaseHandler : MonoBehaviour
    {
        /// <summary>
        /// Initialize the handler.
        /// </summary>
        public virtual void Initialize()
        {
            Logging.Log("[" + GetType().Name + "] Initialized.");
        }

        /// <summary>
        /// Terminate the handler.
        /// </summary>
        public virtual void Terminate()
        {
            Logging.Log("[" + GetType().Name + "] Terminated.");
        }
    }
}