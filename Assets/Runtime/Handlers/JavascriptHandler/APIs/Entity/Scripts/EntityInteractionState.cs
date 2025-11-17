// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Interaction state for an entity.
    /// Hidden: Visibly hidden and not interactable.
    /// Static: Visible but not interactable.
    /// Physical: Visible and interactable.
    /// Placing: Visible and in a placing interaction mode.
    /// </summary>
    public enum InteractionState { Hidden, Static, Physical, Placing }
}