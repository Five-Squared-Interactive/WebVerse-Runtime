/**
 * Bridge - Unity <-> WebView Communication
 * Handles bidirectional messaging with Unity via Vuplex WebView.
 */

(function() {
    'use strict';

    // Wait for Vuplex to be ready
    function initBridge() {
        if (!window.vuplex) {
            console.warn('[Bridge] Vuplex not available, running in standalone mode');
            window.vuplex = createMockVuplex();
        }

        // Set up message listener from Unity
        window.vuplex.addEventListener('message', handleUnityMessage);

        console.log('[Bridge] Initialized');

        // Notify Unity that we're ready
        sendToUnity({ type: 'ready' });
    }

    /**
     * Handle messages from Unity
     */
    function handleUnityMessage(event) {
        try {
            const data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
            console.log('[Bridge] Received from Unity:', data.type);

            switch (data.type) {
                case 'updateTabs':
                    window.tabUI?.updateTabs(data.tabs);
                    break;

                case 'setActiveTab':
                    window.tabUI?.setActiveTab(data.tabId);
                    break;

                case 'updateNavState':
                    window.tabUI?.updateNavState(data.canGoBack, data.canGoForward, data.canReload);
                    break;

                case 'setMode':
                    window.tabUI?.setMode(data.mode);
                    break;

                case 'showChrome':
                    window.tabUI?.showChrome();
                    break;

                case 'hideChrome':
                    window.tabUI?.hideChrome();
                    break;

                case 'toggleChrome':
                    window.tabUI?.toggleChrome();
                    break;

                case 'setUrl':
                    window.tabUI?.setUrl(data.url);
                    break;

                case 'setLoading':
                    window.tabUI?.setLoading(data.isLoading);
                    break;

                case 'showContentFrame':
                    window.tabUI?.showContentFrame();
                    break;

                case 'hideContentFrame':
                    window.tabUI?.hideContentFrame();
                    break;

                case 'setContentFrameVisible':
                    window.tabUI?.setContentFrameVisible(data.visible);
                    break;

                case 'showToast':
                    window.tabUI?.showToast(data.message, data.toastType, data.duration);
                    break;

                case 'tabLoadStateChanged':
                    window.tabUI?.updateTabLoadState(data.tabId, data.loadState);
                    break;

                // Modal data responses
                case 'historyData':
                    window.tabUI?.updateHistory(data.items);
                    break;

                case 'consoleData':
                    window.tabUI?.updateConsole(data.lines);
                    break;

                case 'consoleLine':
                    window.tabUI?.addConsoleLine(data.level, data.message);
                    break;

                case 'settingsData':
                    window.tabUI?.updateSettings(data.settings);
                    break;

                case 'setTheme':
                    window.tabUI?.setTheme(data.theme);
                    break;

                case 'aboutData':
                    window.tabUI?.updateAboutInfo(data.info);
                    break;

                // Stats/Performance
                case 'statsUpdate':
                    window.tabUI?.updateStats(data.stats);
                    break;

                // Navigation history dropdowns
                case 'backHistoryData':
                    window.tabUI?.updateBackHistory(data.items);
                    break;

                case 'forwardHistoryData':
                    window.tabUI?.updateForwardHistory(data.items);
                    break;

                // Tab thumbnails
                case 'updateTabThumbnail':
                    window.tabUI?.updateTabThumbnail(data.tabId, data.thumbnail);
                    break;

                default:
                    console.warn('[Bridge] Unknown message type:', data.type);
            }
        } catch (error) {
            console.error('[Bridge] Error handling Unity message:', error);
        }
    }

    /**
     * Send message to Unity
     */
    function sendToUnity(message) {
        try {
            const json = JSON.stringify(message);
            if (message.type !== 'requestStats' && message.type !== 'hudBounds') console.log('[Bridge] Sending to Unity:', message.type);
            window.vuplex.postMessage(json);
        } catch (error) {
            console.error('[Bridge] Error sending to Unity:', error);
        }
    }

    /**
     * Create mock Vuplex for standalone testing
     */
    function createMockVuplex() {
        const listeners = {};
        return {
            addEventListener: function(event, callback) {
                if (!listeners[event]) listeners[event] = [];
                listeners[event].push(callback);
            },
            postMessage: function(message) {
                console.log('[Mock Vuplex] Would send:', message);
            },
            // For testing: simulate Unity messages
            simulateMessage: function(data) {
                const event = { data: data };
                (listeners['message'] || []).forEach(cb => cb(event));
            }
        };
    }

    // Expose bridge API
    window.bridge = {
        // Navigation
        navigate: function(url) {
            sendToUnity({ type: 'navigate', url: url });
        },
        goBack: function() {
            sendToUnity({ type: 'goBack' });
        },
        goForward: function() {
            sendToUnity({ type: 'goForward' });
        },
        reload: function() {
            sendToUnity({ type: 'reload' });
        },

        // Navigation history (for back/forward dropdowns)
        requestBackHistory: function() {
            sendToUnity({ type: 'requestBackHistory' });
        },
        requestForwardHistory: function() {
            sendToUnity({ type: 'requestForwardHistory' });
        },
        goBackToIndex: function(index) {
            sendToUnity({ type: 'goBackToIndex', index: index });
        },
        goForwardToIndex: function(index) {
            sendToUnity({ type: 'goForwardToIndex', index: index });
        },

        // Tab operations
        switchTab: function(tabId) {
            sendToUnity({ type: 'switchTab', tabId: tabId });
        },
        closeTab: function(tabId) {
            sendToUnity({ type: 'closeTab', tabId: tabId });
        },
        newTab: function() {
            sendToUnity({ type: 'newTab' });
        },

        // Menu actions
        toggleFullscreen: function() {
            sendToUnity({ type: 'menuAction', action: 'fullscreen' });
        },
        toggleVRMode: function() {
            sendToUnity({ type: 'menuAction', action: 'vr-mode' });
        },

        // Modal data requests
        requestHistory: function() {
            sendToUnity({ type: 'requestHistory' });
        },
        requestConsoleLog: function() {
            sendToUnity({ type: 'requestConsoleLog' });
        },
        requestSettings: function() {
            sendToUnity({ type: 'requestSettings' });
        },
        requestAboutInfo: function() {
            sendToUnity({ type: 'requestAboutInfo' });
        },
        requestStats: function() {
            sendToUnity({ type: 'requestStats' });
        },

        // Modal actions
        clearHistory: function() {
            sendToUnity({ type: 'clearHistory' });
        },
        saveSettings: function(settings) {
            sendToUnity({ type: 'saveSettings', settings: settings });
        },
        clearCache: function(timeRange) {
            sendToUnity({ type: 'clearCache', timeRange: timeRange });
        },
        exit: function() {
            sendToUnity({ type: 'exit' });
        },
        openExternalUrl: function(url) {
            sendToUnity({ type: 'openExternalUrl', url: url });
        },

        // Chrome visibility
        requestHideChrome: function() {
            sendToUnity({ type: 'requestHideChrome' });
        },

        // Theme
        notifyThemeChange: function(theme) {
            sendToUnity({ type: 'themeChanged', theme: theme });
        },

        // Thumbnails
        requestThumbnail: function(tabId) {
            sendToUnity({ type: 'requestThumbnail', tabId: tabId });
        },
        requestAllThumbnails: function() {
            sendToUnity({ type: 'requestAllThumbnails' });
        },

        // Overlay state (modals/dropdowns that extend into content area)
        notifyOverlayOpened: function() {
            sendToUnity({ type: 'overlayOpened' });
        },
        notifyOverlayClosed: function() {
            sendToUnity({ type: 'overlayClosed' });
        },

        // HUD bounds (for precise raycast hit testing)
        notifyHudBounds: function(rect) {
            sendToUnity({ type: 'hudBounds', visible: true, x: rect.x, y: rect.y, width: rect.width, height: rect.height });
        },
        notifyHudHidden: function() {
            sendToUnity({ type: 'hudBounds', visible: false });
        },

        // Ready notification
        notifyReady: function() {
            sendToUnity({ type: 'ready' });
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initBridge);
    } else {
        initBridge();
    }
})();
