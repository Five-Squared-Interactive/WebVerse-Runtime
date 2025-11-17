// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.WebVerse.Interface.ExitMenu
{
    /// <summary>
    /// Class for exit menu.
    /// </summary>
    public class ExitMenu : MonoBehaviour
    {
        /// <summary>
        /// Action to perform upon return.
        /// </summary>
        private Action onReturnAction;

        /// <summary>
        /// Initialize exit menu.
        /// </summary>
        /// <param name="onReturn">Action to perform upon return.</param>
        public void Initialize(Action onReturn)
        {
            onReturnAction = onReturn;
        }

        /// <summary>
        /// Return from exit menu.
        /// </summary>
        public void Return()
        {
            if (onReturnAction != null)
            {
                onReturnAction.Invoke();
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Perform an exit.
        /// </summary>
        public void Exit()
        {
            Application.Quit();
        }
    }
}