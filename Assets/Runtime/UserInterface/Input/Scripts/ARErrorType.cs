// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Types of AR-related errors that can occur during an AR session.
    /// </summary>
    public enum ARErrorType
    {
        PassthroughFailed,
        SurfaceDetectionFailed,
        NoSurfacesFound,
        AnchorPlacementFailed
    }
}