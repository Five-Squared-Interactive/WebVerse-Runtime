// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Quest 3 implementation of world loading error handler.
    /// Extends BaseWorldLoadingErrorHandler with Quest 3-specific runtime integration.
    /// Provides VR-friendly error dialogs when world loading fails.
    /// </summary>
    public class WorldLoadingErrorHandler : BaseWorldLoadingErrorHandler
    {
        #region BaseWorldLoadingErrorHandler Implementation

        protected override void CheckRuntimeState()
        {
            if (WebVerseRuntime.Instance == null)
            {
                return;
            }

            // Check for error state
            if (WebVerseRuntime.Instance.state == WebVerseRuntime.RuntimeState.Error && !isShowingError)
            {
                OnWorldLoadingFailed("Failed to load world content");
            }
            else if (WebVerseRuntime.Instance.state == WebVerseRuntime.RuntimeState.LoadedWorld && isLoading)
            {
                OnWorldLoadingSucceeded();
            }
        }

        #endregion
    }
}
