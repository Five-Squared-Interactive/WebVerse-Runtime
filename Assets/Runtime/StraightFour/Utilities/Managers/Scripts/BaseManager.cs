// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Utilities
{
    /// <summary>
    /// Base class for a manager.
    /// </summary>
    public class BaseManager : MonoBehaviour
    {
        /// <summary>
        /// Initialize the manager.
        /// </summary>
        public virtual void Initialize()
        {
            LogSystem.Log("[" + GetType().Name + "] Initialized.");
        }

        /// <summary>
        /// Terminate the manager.
        /// </summary>
        public virtual void Terminate()
        {
            LogSystem.Log("[" + GetType().Name + "] Terminated.");
        }
    }
}