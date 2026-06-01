// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Interface for screen fade transitions during mode switching.
    /// InputManager holds a nullable reference; null on non-AR platforms.
    /// </summary>
    public interface IFadeTransition
    {
        /// <summary>
        /// Fade the screen to black, then invoke the callback.
        /// </summary>
        /// <param name="callback">Action to invoke once the screen is fully black.</param>
        void FadeOut(Action callback);

        /// <summary>
        /// Fade the screen back from black to clear.
        /// </summary>
        void FadeIn();
    }
}