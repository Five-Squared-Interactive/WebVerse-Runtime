# Dual-WebView Architecture for WebVerse Tab UI

## Overview

This document describes the architecture for WebVerse's browser-like Tab UI system, which uses two separate Vuplex WebViews to provide a native browsing experience within Unity.

## Problem Statement

WebVerse needs to display web content (worlds, HTML pages) with a browser-like interface including:
- Tab management
- URL bar with navigation
- Back/forward history
- Loading indicators

### Why Not Use iframes?

Initially considered embedding web content in an iframe within the Tab UI WebView, but this approach has significant limitations:

| Issue | Description |
|-------|-------------|
| **Cross-origin policy** | Cannot intercept clicks or inspect content in cross-origin iframes |
| **Click detection** | Browser security prevents capturing link clicks inside iframes |
| **Performance** | WebView-in-WebView adds unnecessary overhead |
| **Navigation control** | Cannot easily monitor or control iframe navigation |

## Solution: Dual-WebView Architecture

Use two separate Vuplex WebViews coordinated by a Unity manager:

1. **Chrome WebView** - Renders the browser UI (tabs, URL bar, menus)
2. **Content WebView** - Renders actual web content (worlds, pages)

This leverages Vuplex's native navigation events and provides full control over both UI and content.

---

## System Architecture

### High-Level Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                          Unity Scene                                  │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                    TabUIManager (MonoBehaviour)                 │  │
│  │                                                                 │  │
│  │  Responsibilities:                                              │  │
│  │  - Owns and initializes both WebViews                          │  │
│  │  - Routes messages between Chrome and Content                  │  │
│  │  - Manages tab state (active tab, tab list)                    │  │
│  │  - Maintains navigation history per tab                        │  │
│  │  - Handles tab switching (snapshot/restore)                    │  │
│  └───────────────────────┬────────────────────────────────────────┘  │
│                          │                                            │
│            ┌─────────────┴─────────────┐                             │
│            ▼                           ▼                              │
│  ┌──────────────────┐       ┌──────────────────────┐                 │
│  │  Chrome WebView  │       │   Content WebView    │                 │
│  │                  │       │                      │                 │
│  │  Renders:        │       │  Renders:            │                 │
│  │  - Tab bar       │       │  - Web pages         │                 │
│  │  - URL bar       │       │  - World content     │                 │
│  │  - Nav buttons   │       │  - HTML/JS apps      │                 │
│  │  - Menus/modals  │       │                      │                 │
│  │  - Stats HUD     │       │                      │                 │
│  │                  │       │                      │                 │
│  │  Size: Full      │       │  Size: Content       │                 │
│  │  screen overlay  │       │  frame area          │                 │
│  └──────────────────┘       └──────────────────────┘                 │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘
```

### Visual Layout

#### Desktop Mode (Chrome at Bottom)

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                                                             │
│                    Content WebView                          │
│                    (Web Page Area)                          │
│                                                             │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  [Tab] [◄] [►] [↻] [ URL Bar ..................] [≡]       │
│                    Chrome WebView                           │
└─────────────────────────────────────────────────────────────┘
```

#### VR Mode (Chrome at Top)

```
┌─────────────────────────────────────────────────────────────┐
│  [Tab] [◄] [►] [↻] [ URL Bar ..................] [≡]       │
│                    Chrome WebView                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                                                             │
│                    Content WebView                          │
│                    (Web Page Area)                          │
│                                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Details

### TabUIManager

The central coordinator that owns both WebViews and manages all state.

```csharp
public class TabUIManager : MonoBehaviour
{
    // WebView references
    [SerializeField] private CanvasWebViewPrefab chromeWebView;
    [SerializeField] private CanvasWebViewPrefab contentWebView;

    // State
    private TabState tabState;
    private bool isVRMode;

    // Lifecycle
    void Awake();
    void OnDestroy();

    // Navigation
    public void NavigateTo(string url);
    public void GoBack();
    public void GoForward();
    public void Reload();
    public void StopLoading();

    // Tab Management
    public void NewTab(string url = null);
    public void CloseTab(string tabId);
    public void SwitchTab(string tabId);

    // Mode
    public void SetVRMode(bool vrMode);
}
```

### TabState

Manages the collection of tabs and their states.

```csharp
public class TabState
{
    public List<Tab> Tabs { get; }
    public string ActiveTabId { get; }

    public Tab GetActiveTab();
    public Tab GetTab(string id);
    public Tab CreateTab(string url = null);
    public void RemoveTab(string id);
    public void SetActiveTab(string id);
}

public class Tab
{
    public string Id { get; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string FaviconUrl { get; set; }
    public bool IsLoading { get; set; }
    public float LoadProgress { get; set; }
    public TabHistory History { get; }
    public byte[] Snapshot { get; set; } // For tab switching
}
```

### TabHistory

Per-tab navigation history management.

```csharp
public class TabHistory
{
    public List<HistoryEntry> BackStack { get; }
    public List<HistoryEntry> ForwardStack { get; }
    public HistoryEntry Current { get; }

    public bool CanGoBack { get; }
    public bool CanGoForward { get; }

    public void Push(string url, string title);
    public HistoryEntry GoBack();
    public HistoryEntry GoForward();
    public List<HistoryEntry> GetBackHistory(int count = 10);
    public List<HistoryEntry> GetForwardHistory(int count = 10);
}

public class HistoryEntry
{
    public string Url { get; set; }
    public string Title { get; set; }
    public DateTime Timestamp { get; set; }
    public Vector2 ScrollPosition { get; set; }
}
```

---

## Communication Protocol

### Message Flow Directions

```
Chrome WebView ──PostMessage──► TabUIManager ──Vuplex API──► Content WebView
                                     │
Content WebView ──Vuplex Events──► TabUIManager ──PostMessage──► Chrome WebView
```

### Chrome → Unity Messages

Messages sent from the Chrome WebView (Tab UI) to Unity via Vuplex's `PostMessage`.

| Message Type | Payload | Description |
|--------------|---------|-------------|
| `navigate` | `{ url: string }` | User entered URL or clicked go |
| `goBack` | `{}` | Back button clicked |
| `goForward` | `{}` | Forward button clicked |
| `reload` | `{}` | Reload button clicked |
| `stopLoading` | `{}` | Stop button clicked |
| `newTab` | `{ url?: string }` | New tab requested |
| `closeTab` | `{ tabId: string }` | Close tab clicked |
| `switchTab` | `{ tabId: string }` | Tab selected |
| `requestBackHistory` | `{}` | Back button long-pressed |
| `requestForwardHistory` | `{}` | Forward button long-pressed |
| `goBackToIndex` | `{ index: number }` | History item selected |
| `goForwardToIndex` | `{ index: number }` | History item selected |

### Unity → Chrome Messages

Messages sent from Unity to the Chrome WebView to update UI state.

| Message Type | Payload | Description |
|--------------|---------|-------------|
| `urlChanged` | `{ url: string }` | Content URL changed |
| `titleChanged` | `{ title: string }` | Page title changed |
| `loadingChanged` | `{ loading: boolean, progress?: number }` | Loading state changed |
| `navStateChanged` | `{ canGoBack: boolean, canGoForward: boolean }` | Nav button states |
| `tabsUpdated` | `{ tabs: Tab[], activeTabId: string }` | Tab list changed |
| `backHistoryData` | `{ items: HistoryEntry[] }` | Back history for dropdown |
| `forwardHistoryData` | `{ items: HistoryEntry[] }` | Forward history for dropdown |

### Content WebView Events (Vuplex)

Events from the Content WebView that Unity listens to.

| Vuplex Event | Unity Handler | Action |
|--------------|---------------|--------|
| `UrlChanged` | `OnContentUrlChanged` | Update Chrome URL bar, push to history |
| `TitleChanged` | `OnContentTitleChanged` | Update tab title in Chrome |
| `LoadProgressChanged` | `OnContentLoadProgress` | Update loading indicator |
| `PageLoadFailed` | `OnContentLoadFailed` | Show error state |
| `CloseRequested` | `OnContentCloseRequested` | Handle window.close() |

---

## Data Flow Examples

### Example 1: User Clicks Link in Content

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Content   │         │  TabUIManager │         │   Chrome    │
│   WebView   │         │              │         │   WebView   │
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘
       │                       │                        │
       │ UrlChanged Event      │                        │
       │ (user clicked link)   │                        │
       │──────────────────────►│                        │
       │                       │                        │
       │                       │ Update TabState        │
       │                       │ Push to history        │
       │                       │                        │
       │                       │ PostMessage            │
       │                       │ {type:"urlChanged"}    │
       │                       │───────────────────────►│
       │                       │                        │
       │                       │ PostMessage            │
       │                       │ {type:"navStateChanged"}
       │                       │───────────────────────►│
       │                       │                        │
       │                       │                        │ Update URL bar
       │                       │                        │ Update nav buttons
       │                       │                        │
```

### Example 2: User Types URL in Chrome

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Chrome    │         │  TabUIManager │         │   Content   │
│   WebView   │         │              │         │   WebView   │
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"navigate",     │                        │
       │  url:"https://..."}   │                        │
       │──────────────────────►│                        │
       │                       │                        │
       │                       │ contentWebView         │
       │                       │   .LoadUrl(url)        │
       │                       │───────────────────────►│
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"loadingChanged",                        │
       │  loading:true}        │                        │
       │◄──────────────────────│                        │
       │                       │                        │
       │                       │ LoadProgressChanged    │
       │                       │◄───────────────────────│
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"loadingChanged",                        │
       │  loading:false}       │                        │
       │◄──────────────────────│                        │
       │                       │                        │
```

### Example 3: User Switches Tabs

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Chrome    │         │  TabUIManager │         │   Content   │
│   WebView   │         │              │         │   WebView   │
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"switchTab",    │                        │
       │  tabId:"tab-2"}       │                        │
       │──────────────────────►│                        │
       │                       │                        │
       │                       │ 1. Save current tab    │
       │                       │    snapshot/scroll     │
       │                       │◄──────────────────────►│
       │                       │                        │
       │                       │ 2. Update activeTabId  │
       │                       │                        │
       │                       │ 3. Load new tab URL    │
       │                       │───────────────────────►│
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"tabsUpdated",  │                        │
       │  activeTabId:"tab-2"} │                        │
       │◄──────────────────────│                        │
       │                       │                        │
       │ PostMessage           │                        │
       │ {type:"urlChanged",   │                        │
       │  url:"..."}           │                        │
       │◄──────────────────────│                        │
       │                       │                        │
```

---

## Layout and Positioning

### RectTransform Configuration

The Content WebView is positioned to fill the area within the content frame, excluding the Chrome bar.

```csharp
public void PositionContentWebView()
{
    RectTransform contentRect = contentWebView.GetComponent<RectTransform>();

    // Anchor to fill parent
    contentRect.anchorMin = Vector2.zero;
    contentRect.anchorMax = Vector2.one;

    // Content frame padding (matches CSS --content-frame-radius)
    float padding = 16f;

    // Chrome bar height (matches CSS --bar-height)
    float chromeHeight = isVRMode ? 64f : 48f;

    if (isVRMode)
    {
        // Chrome at top
        contentRect.offsetMin = new Vector2(padding, padding);
        contentRect.offsetMax = new Vector2(-padding, -(chromeHeight + padding));
    }
    else
    {
        // Chrome at bottom
        contentRect.offsetMin = new Vector2(padding, chromeHeight + padding);
        contentRect.offsetMax = new Vector2(-padding, -padding);
    }
}
```

### Z-Order / Render Order

```
Layer Order (front to back):
1. Chrome WebView (always on top for UI interaction)
2. Content WebView (behind chrome)
3. Unity 3D content (if any, behind both)
```

---

## Tab Switching Strategy

When switching tabs, we need to preserve the state of the previous tab:

### Option A: URL-Only (Simple)

Just store the URL; reload page when switching back.

```csharp
public void SwitchTab(string newTabId)
{
    // Store current URL
    currentTab.Url = contentWebView.Url;

    // Load new tab URL
    var newTab = tabState.GetTab(newTabId);
    contentWebView.LoadUrl(newTab.Url);
}
```

**Pros:** Simple, low memory
**Cons:** Loses scroll position, form data, JS state

### Option B: Snapshot (Advanced)

Capture visual snapshot for instant preview, reload on focus.

```csharp
public async void SwitchTab(string newTabId)
{
    // Capture snapshot of current tab
    currentTab.Snapshot = await contentWebView.CaptureScreenshot();
    currentTab.ScrollPosition = await GetScrollPosition();

    // Show snapshot immediately while loading
    ShowTabSnapshot(newTab.Snapshot);

    // Load new tab URL
    contentWebView.LoadUrl(newTab.Url);

    // Restore scroll position after load
    await WaitForPageLoad();
    await SetScrollPosition(newTab.ScrollPosition);
}
```

**Pros:** Better UX, preserves scroll position
**Cons:** More complex, memory usage for snapshots

### Option C: Multiple WebViews (Heavyweight)

One Content WebView per tab, show/hide as needed.

**Pros:** True tab persistence, instant switching
**Cons:** High memory usage, complex management

**Recommendation:** Start with Option A, upgrade to Option B if needed.

---

## Error Handling

### Navigation Errors

```csharp
contentWebView.PageLoadFailed += (sender, args) =>
{
    // Update tab state
    currentTab.IsLoading = false;
    currentTab.HasError = true;
    currentTab.ErrorMessage = args.ErrorMessage;

    // Notify Chrome to show error state
    SendToChromeWebView(new {
        type = "loadError",
        error = args.ErrorMessage,
        url = args.Url
    });
};
```

### WebView Crash Recovery

```csharp
contentWebView.Terminated += (sender, args) =>
{
    // Log crash
    Debug.LogError($"Content WebView crashed: {args.Type}");

    // Recreate WebView
    RecreateContentWebView();

    // Reload current tab
    contentWebView.LoadUrl(currentTab.Url);
};
```

---

## Security Considerations

### URL Validation

```csharp
public bool IsUrlAllowed(string url)
{
    // Parse URL
    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        return false;

    // Allow HTTP/HTTPS only
    if (uri.Scheme != "http" && uri.Scheme != "https")
        return false;

    // Optional: Check against blocklist
    if (IsBlockedDomain(uri.Host))
        return false;

    return true;
}
```

### Content Security

- Content WebView should have JavaScript enabled for web compatibility
- Chrome WebView can have stricter settings (no external navigation)
- Consider sandboxing for untrusted content

---

## Performance Considerations

### Memory Management

- Limit number of open tabs (e.g., max 10)
- Clear snapshots for inactive tabs after timeout
- Release WebView resources on tab close

### Rendering

- Use `SetRenderingEnabled(false)` for hidden Content WebView during UI-only interactions
- Throttle message frequency between WebViews
- Batch tab state updates to Chrome

---

## Future Enhancements

### Phase 2 Features

- [ ] Tab persistence across sessions (save/restore tabs)
- [ ] Pinned tabs
- [ ] Tab groups
- [ ] Picture-in-picture mode for video content
- [ ] Download manager integration
- [ ] Print support

### Phase 3 Features

- [ ] Extensions/plugin support
- [ ] Developer tools integration
- [ ] Multi-window support
- [ ] Sync across devices

---

## Related Files

- `TabUI/index.html` - Chrome WebView HTML
- `TabUI/scripts/ui.js` - Chrome WebView JavaScript
- `TabUI/scripts/bridge.js` - Chrome ↔ Unity communication
- `TabUI/styles/` - Chrome WebView styling
- `TabUIController.cs` - Current Unity controller (to be replaced/extended by TabUIManager)

---

## References

- [Vuplex WebView Documentation](https://developer.vuplex.com/webview/overview)
- [Vuplex IWebView API](https://developer.vuplex.com/webview/IWebView)
- [Vuplex CanvasWebViewPrefab](https://developer.vuplex.com/webview/CanvasWebViewPrefab)
