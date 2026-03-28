/**
 * UI - Tab UI State Management and Rendering
 * Manages the chrome bar, tabs, dropdowns, and user interactions.
 */

(function() {
    'use strict';

    // State
    const state = {
        tabs: [],
        activeTabId: null,
        mode: 'desktop', // 'desktop' or 'vr'
        chromeVisible: true,
        tabDropdownOpen: false,
        menuDropdownOpen: false,
        backHistoryDropdownOpen: false,
        forwardHistoryDropdownOpen: false,
        canGoBack: false,
        canGoForward: false,
        canReload: false,
        isLoading: false,
        isFullscreen: false,
        isVRMode: false,
        currentUrl: '',
        theme: 'system', // 'system', 'dark', 'light'
        backHistory: [],
        forwardHistory: [],
        // Thumbnails: tabId -> dataUrl
        tabThumbnails: {},
        // Stats
        statsHudVisible: false,
        statsModalOpen: false,
        stats: {
            rendering: { fps: 0, frameTimeMs: 0, fpsHistory: [], drawCalls: 0, triangles: 0, setPassCalls: 0, gpuMemoryMB: 0 },
            system: { totalMemoryMB: 0, usedMemoryMB: 0, monoHeapMB: 0, monoUsedMB: 0, gcTotalMB: 0 },
            network: { pingMs: 0, isConnected: false, connectionState: 'Disconnected', downloadKBps: 0, uploadKBps: 0 },
            world: { entityCount: 0, activeScripts: 0, physicsBodies: 0, audioSources: 0, texturesLoaded: 0, textureMemoryMB: 0 }
        }
    };

    // Stats update interval
    let statsUpdateInterval = null;
    const STATS_UPDATE_RATE = 1000; // ms

    // Long press detection
    const LONG_PRESS_DELAY = 500; // ms
    let backButtonPressTimer = null;
    let forwardButtonPressTimer = null;
    let backButtonPressed = false;
    let forwardButtonPressed = false;

    // DOM Elements
    let elements = {};

    // Content frame element (cached separately for performance)
    let contentFrame = null;

    // Tooltip element and state
    let tooltipElement = null;
    let tooltipTimeout = null;
    const TOOLTIP_DELAY = 500; // ms before showing tooltip

    // Thumbnail preview element and state
    let thumbnailPreviewElement = null;
    let thumbnailHoverTimeout = null;
    let currentThumbnailTabId = null;
    const THUMBNAIL_HOVER_DELAY = 300; // ms before showing thumbnail preview

    /**
     * Initialize the UI
     */
    function init() {
        cacheElements();
        bindEvents();
        initTooltips();
        bindKeyboardShortcuts();

        // Initialize theme from localStorage or default to system
        initTheme();

        // Preview mode: show content frame when running standalone (not in Unity)
        if (!window.vuplex) {
            showContentFrame();
        }

        console.log('[UI] Initialized');
    }

    // ===================
    // Tooltips
    // ===================

    /**
     * Initialize tooltip system
     */
    function initTooltips() {
        // Create tooltip element
        tooltipElement = document.createElement('div');
        tooltipElement.className = 'tooltip';
        tooltipElement.setAttribute('role', 'tooltip');
        tooltipElement.style.display = 'none';
        document.body.appendChild(tooltipElement);

        // Add event listeners to all elements with data-tooltip
        document.addEventListener('mouseover', handleTooltipMouseOver);
        document.addEventListener('mouseout', handleTooltipMouseOut);
        document.addEventListener('focusin', handleTooltipFocusIn);
        document.addEventListener('focusout', handleTooltipFocusOut);
    }

    /**
     * Handle mouseover for tooltips
     */
    function handleTooltipMouseOver(e) {
        const target = e.target.closest('[data-tooltip]');
        if (target) {
            scheduleTooltip(target);
        }
    }

    /**
     * Handle mouseout for tooltips
     */
    function handleTooltipMouseOut(e) {
        const target = e.target.closest('[data-tooltip]');
        if (target) {
            hideTooltip();
        }
    }

    /**
     * Handle focus for tooltips (accessibility)
     */
    function handleTooltipFocusIn(e) {
        const target = e.target.closest('[data-tooltip]');
        if (target) {
            scheduleTooltip(target);
        }
    }

    /**
     * Handle blur for tooltips
     */
    function handleTooltipFocusOut(e) {
        hideTooltip();
    }

    /**
     * Schedule tooltip to appear after delay
     */
    function scheduleTooltip(target) {
        clearTimeout(tooltipTimeout);
        tooltipTimeout = setTimeout(() => {
            showTooltip(target);
        }, TOOLTIP_DELAY);
    }

    /**
     * Show tooltip for element
     */
    function showTooltip(target) {
        const text = target.getAttribute('data-tooltip');
        if (!text || !tooltipElement) return;

        tooltipElement.textContent = text;
        tooltipElement.style.display = 'block';

        // Position tooltip
        const rect = target.getBoundingClientRect();
        const tooltipRect = tooltipElement.getBoundingClientRect();

        // Default: below the element
        let top = rect.bottom + 8;
        let left = rect.left + (rect.width / 2) - (tooltipRect.width / 2);

        // Check if tooltip goes off screen bottom - show above instead
        if (top + tooltipRect.height > window.innerHeight) {
            top = rect.top - tooltipRect.height - 8;
        }

        // Keep tooltip within horizontal bounds
        if (left < 8) left = 8;
        if (left + tooltipRect.width > window.innerWidth - 8) {
            left = window.innerWidth - tooltipRect.width - 8;
        }

        tooltipElement.style.top = top + 'px';
        tooltipElement.style.left = left + 'px';
    }

    /**
     * Hide tooltip
     */
    function hideTooltip() {
        clearTimeout(tooltipTimeout);
        if (tooltipElement) {
            tooltipElement.style.display = 'none';
        }
    }

    // ===================
    // Thumbnail Preview
    // ===================

    /**
     * Update a tab's thumbnail
     * @param {string} tabId - The tab ID
     * @param {string} dataUrl - The base64 data URL
     */
    function updateTabThumbnail(tabId, dataUrl) {
        if (!tabId) return;
        state.tabThumbnails[tabId] = dataUrl;

        // If we're currently showing this tab's preview, update it
        if (currentThumbnailTabId === tabId && thumbnailPreviewElement) {
            if (elements.thumbnailPreviewImage) {
                elements.thumbnailPreviewImage.src = dataUrl;
                elements.thumbnailPreviewImage.style.display = 'block';
            }
            if (elements.thumbnailPreviewPlaceholder) {
                elements.thumbnailPreviewPlaceholder.style.display = 'none';
            }
        }
    }

    /**
     * Show thumbnail preview for a tab
     * @param {string} tabId - The tab ID
     * @param {HTMLElement} anchor - The element to position near
     */
    function showThumbnailPreview(tabId, anchor) {
        if (!tabId || !thumbnailPreviewElement || !anchor) return;

        currentThumbnailTabId = tabId;
        const thumbnail = state.tabThumbnails[tabId];

        // Show or hide image based on thumbnail availability
        if (elements.thumbnailPreviewImage && elements.thumbnailPreviewPlaceholder) {
            if (thumbnail) {
                elements.thumbnailPreviewImage.src = thumbnail;
                elements.thumbnailPreviewImage.style.display = 'block';
                elements.thumbnailPreviewPlaceholder.style.display = 'none';
            } else {
                elements.thumbnailPreviewImage.style.display = 'none';
                elements.thumbnailPreviewPlaceholder.style.display = 'flex';
                // Request thumbnail from Unity if not cached
                window.bridge?.requestThumbnail(tabId);
            }
        }

        // Position preview near the anchor
        const anchorRect = anchor.getBoundingClientRect();
        const previewWidth = 256;
        const previewHeight = 144;
        const padding = 12;

        // Position to the right of the tab dropdown
        let left = anchorRect.right + padding;
        let top = anchorRect.top;

        // Check if it goes off screen right - show on left instead
        if (left + previewWidth > window.innerWidth - padding) {
            left = anchorRect.left - previewWidth - padding;
        }

        // Check if it goes off screen left
        if (left < padding) {
            left = padding;
        }

        // Check if it goes off screen bottom
        if (top + previewHeight > window.innerHeight - padding) {
            top = window.innerHeight - previewHeight - padding;
        }

        // Check if it goes off screen top
        if (top < padding) {
            top = padding;
        }

        thumbnailPreviewElement.style.left = left + 'px';
        thumbnailPreviewElement.style.top = top + 'px';
        thumbnailPreviewElement.classList.add('thumbnail-preview--visible');
    }

    /**
     * Hide thumbnail preview
     */
    function hideThumbnailPreview() {
        clearTimeout(thumbnailHoverTimeout);
        currentThumbnailTabId = null;

        if (thumbnailPreviewElement) {
            thumbnailPreviewElement.classList.remove('thumbnail-preview--visible');
        }
    }

    /**
     * Schedule showing thumbnail preview
     * @param {string} tabId - The tab ID
     * @param {HTMLElement} anchor - The element to position near
     */
    function scheduleThumbnailPreview(tabId, anchor) {
        clearTimeout(thumbnailHoverTimeout);
        thumbnailHoverTimeout = setTimeout(() => {
            showThumbnailPreview(tabId, anchor);
        }, THUMBNAIL_HOVER_DELAY);
    }

    /**
     * Remove a tab's thumbnail from the cache
     * @param {string} tabId - The tab ID
     */
    function removeTabThumbnail(tabId) {
        if (tabId && state.tabThumbnails[tabId]) {
            delete state.tabThumbnails[tabId];
        }
    }

    // ===================
    // Keyboard Shortcuts
    // ===================

    /**
     * Bind keyboard shortcuts
     */
    function bindKeyboardShortcuts() {
        document.addEventListener('keydown', handleKeyDown);
        console.log('[UI] Keyboard shortcuts bound');
    }

    /**
     * Handle keydown events for keyboard shortcuts
     * @param {KeyboardEvent} e
     */
    function handleKeyDown(e) {
        const key = e.key;

        // Check if URL bar is focused
        const urlBarFocused = document.activeElement === elements.urlBar;

        // Escape - close dropdowns/modals or blur URL bar
        if (key === 'Escape') {
            if (urlBarFocused) {
                elements.urlBar.blur();
                elements.urlBar.value = state.currentUrl; // Restore URL
            } else if (state.tabDropdownOpen || state.menuDropdownOpen ||
                       state.backHistoryDropdownOpen || state.forwardHistoryDropdownOpen) {
                closeAllDropdowns();
            } else {
                closeAllModals();
            }
            e.preventDefault();
            return;
        }

        // URL bar Enter to navigate
        if (urlBarFocused && key === 'Enter') {
            const url = elements.urlBar.value.trim();
            if (url) {
                window.bridge?.navigate(url);
                elements.urlBar.blur();
            }
            e.preventDefault();
            return;
        }

        // All other keyboard shortcuts are handled by C# TabUIInputHandler
        // to avoid double-firing (Unity Input System + Vuplex key forwarding).
    }

    /**
     * Flash a button to indicate shortcut activation
     * @param {HTMLElement} button
     */
    function flashButton(button) {
        if (!button) return;
        button.classList.add('shortcut-flash');
        setTimeout(() => {
            button.classList.remove('shortcut-flash');
        }, 200);
    }

    /**
     * Initialize theme on load
     */
    function initTheme() {
        const savedTheme = localStorage.getItem('webverse-theme') || 'system';
        setTheme(savedTheme);
    }

    /**
     * Set the theme
     * @param {string} theme - 'system', 'dark', or 'light'
     */
    function setTheme(theme) {
        state.theme = theme;

        // Remove existing theme classes
        document.documentElement.classList.remove('dark-mode', 'light-mode');

        // Apply theme class (system = no class, let CSS media query handle it)
        if (theme === 'dark') {
            document.documentElement.classList.add('dark-mode');
        } else if (theme === 'light') {
            document.documentElement.classList.add('light-mode');
        }

        // Save to localStorage
        localStorage.setItem('webverse-theme', theme);

        // Update the settings dropdown if it exists
        const themeSelect = document.getElementById('setting-theme');
        if (themeSelect) {
            themeSelect.value = theme;
        }

        // Notify Unity of theme change
        window.bridge?.notifyThemeChange(theme);

        console.log('[UI] Theme set to:', theme);
    }

    /**
     * Get current theme
     */
    function getTheme() {
        return state.theme;
    }

    /**
     * Cache DOM element references
     */
    function cacheElements() {
        elements = {
            chrome: document.getElementById('chrome'),
            tabsButton: document.getElementById('tabs-button'),
            tabsButtonIcon: document.querySelector('.tabs-button__icon'),
            tabsButtonWorldIcon: document.querySelector('.tabs-button__world-icon'),
            mainBar: document.querySelector('.main-bar'),
            btnBack: document.getElementById('btn-back'),
            btnForward: document.getElementById('btn-forward'),
            btnReload: document.getElementById('btn-reload'),
            backHistoryDropdown: document.getElementById('back-history-dropdown'),
            forwardHistoryDropdown: document.getElementById('forward-history-dropdown'),
            urlBar: document.getElementById('url-bar'),
            loadingIndicator: document.getElementById('loading-indicator'),
            btnFullscreen: document.getElementById('btn-fullscreen'),
            btnVR: document.getElementById('btn-vr'),
            btnMenu: document.getElementById('btn-menu'),
            tabDropdown: document.getElementById('tab-dropdown'),
            tabList: document.getElementById('tab-list'),
            btnNewTab: document.getElementById('btn-new-tab'),
            menuDropdown: document.getElementById('menu-dropdown'),
            toastContainer: document.getElementById('toast-container'),
            // Stats elements
            statsHud: document.getElementById('stats-hud'),
            statsModal: document.getElementById('modal-stats'),
            // Thumbnail preview
            thumbnailPreview: document.getElementById('thumbnail-preview'),
            thumbnailPreviewImage: document.getElementById('thumbnail-preview-image'),
            thumbnailPreviewPlaceholder: document.getElementById('thumbnail-preview-placeholder')
        };

        // Cache content frame separately
        contentFrame = document.getElementById('content-frame');

        // Cache thumbnail preview element
        thumbnailPreviewElement = elements.thumbnailPreview;
    }

    /**
     * Bind event listeners
     */
    function bindEvents() {
        // Tabs button
        elements.tabsButton.addEventListener('click', toggleTabDropdown);

        // Navigation buttons - with long press detection for history
        bindBackButtonEvents();
        bindForwardButtonEvents();
        elements.btnReload.addEventListener('click', () => window.bridge.reload());

        // URL bar
        elements.urlBar.addEventListener('keydown', handleUrlKeydown);
        elements.urlBar.addEventListener('focus', handleUrlFocus);
        elements.urlBar.addEventListener('blur', handleUrlBlur);

        // Fullscreen button
        elements.btnFullscreen.addEventListener('click', handleFullscreenToggle);

        // VR button
        elements.btnVR.addEventListener('click', handleVRToggle);

        // Menu button
        elements.btnMenu.addEventListener('click', toggleMenuDropdown);

        // New tab button
        elements.btnNewTab.addEventListener('click', handleNewTab);

        // Menu items
        elements.menuDropdown.querySelectorAll('.menu-item').forEach(item => {
            item.addEventListener('click', () => handleMenuAction(item.dataset.action));
        });

        // Modal close buttons
        document.querySelectorAll('[data-modal-close]').forEach(btn => {
            btn.addEventListener('click', () => closeModal(btn.dataset.modalClose));
        });

        // Modal action buttons
        document.querySelectorAll('[data-action]').forEach(btn => {
            if (!btn.classList.contains('menu-item')) {
                btn.addEventListener('click', () => handleMenuAction(btn.dataset.action));
            }
        });

        // Console filter checkboxes
        document.querySelectorAll('#console-filters input[data-filter]').forEach(input => {
            input.addEventListener('change', applyConsoleFilters);
        });

        // Console search input
        const consoleSearch = document.getElementById('console-search');
        if (consoleSearch) {
            consoleSearch.addEventListener('input', applyConsoleFilters);
        }

        // Console search clear button
        const consoleSearchClear = document.getElementById('console-search-clear');
        if (consoleSearchClear) {
            consoleSearchClear.addEventListener('click', clearConsoleSearch);
        }

        // Theme select change
        const themeSelect = document.getElementById('setting-theme');
        if (themeSelect) {
            themeSelect.addEventListener('change', (e) => setTheme(e.target.value));
        }

        // Close dropdowns on outside click
        document.addEventListener('click', handleOutsideClick);
    }

    /**
     * Handle URL bar keydown
     */
    function handleUrlKeydown(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const url = elements.urlBar.value.trim();
            if (url) {
                window.bridge.navigate(url);
                elements.urlBar.blur();
            }
        } else if (e.key === 'Escape') {
            elements.urlBar.blur();
        }
    }

    /**
     * Handle URL bar focus
     */
    function handleUrlFocus() {
        elements.urlBar.select();
    }

    /**
     * Handle URL bar blur
     */
    function handleUrlBlur() {
        // Restore current URL if input was cleared
        if (!elements.urlBar.value.trim() && state.currentUrl) {
            elements.urlBar.value = state.currentUrl;
        }
    }


    /**
     * Toggle tab dropdown
     */
    function toggleTabDropdown() {
        if (state.tabDropdownOpen) {
            closeTabDropdown();
        } else {
            closeMenuDropdown();
            openTabDropdown();
        }
    }

    /**
     * Open tab dropdown
     */
    function openTabDropdown() {
        state.tabDropdownOpen = true;
        elements.tabsButton.setAttribute('aria-expanded', 'true');
        elements.tabDropdown.style.display = 'block';
        requestAnimationFrame(() => {
            elements.tabDropdown.classList.add('dropdown--open');
        });
        window.bridge?.notifyOverlayOpened();
    }

    /**
     * Close tab dropdown
     */
    function closeTabDropdown() {
        state.tabDropdownOpen = false;
        elements.tabsButton.setAttribute('aria-expanded', 'false');
        elements.tabDropdown.classList.remove('dropdown--open');
        hideThumbnailPreview();
        setTimeout(() => {
            if (!state.tabDropdownOpen) {
                elements.tabDropdown.style.display = 'none';
            }
        }, 200);
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Toggle menu dropdown
     */
    function toggleMenuDropdown() {
        if (state.menuDropdownOpen) {
            closeMenuDropdown();
        } else {
            closeTabDropdown();
            openMenuDropdown();
        }
    }

    /**
     * Open menu dropdown
     */
    function openMenuDropdown() {
        state.menuDropdownOpen = true;
        elements.btnMenu.setAttribute('aria-expanded', 'true');
        elements.menuDropdown.style.display = 'block';
        requestAnimationFrame(() => {
            elements.menuDropdown.classList.add('dropdown--open');
        });
        window.bridge?.notifyOverlayOpened();
    }

    /**
     * Close menu dropdown
     */
    function closeMenuDropdown() {
        state.menuDropdownOpen = false;
        elements.btnMenu.setAttribute('aria-expanded', 'false');
        elements.menuDropdown.classList.remove('dropdown--open');
        setTimeout(() => {
            if (!state.menuDropdownOpen) {
                elements.menuDropdown.style.display = 'none';
            }
        }, 200);
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Close all dropdowns
     */
    function closeAllDropdowns() {
        closeTabDropdown();
        closeMenuDropdown();
        closeBackHistoryDropdown();
        closeForwardHistoryDropdown();
    }

    // ===================
    // Back/Forward History Dropdowns
    // ===================

    /**
     * Bind back button events for click and long-press
     */
    function bindBackButtonEvents() {
        const btn = elements.btnBack;

        // Mouse events
        btn.addEventListener('mousedown', (e) => {
            if (btn.disabled) return;
            backButtonPressed = true;
            backButtonPressTimer = setTimeout(() => {
                if (backButtonPressed) {
                    showBackHistoryDropdown();
                }
            }, LONG_PRESS_DELAY);
        });

        btn.addEventListener('mouseup', () => {
            if (backButtonPressTimer) {
                clearTimeout(backButtonPressTimer);
                // If released before long press, do normal back navigation
                if (backButtonPressed && !state.backHistoryDropdownOpen) {
                    window.bridge.goBack();
                }
            }
            backButtonPressed = false;
        });

        btn.addEventListener('mouseleave', () => {
            if (backButtonPressTimer) {
                clearTimeout(backButtonPressTimer);
            }
            backButtonPressed = false;
        });

        // Touch events for mobile/VR
        btn.addEventListener('touchstart', (e) => {
            if (btn.disabled) return;
            backButtonPressed = true;
            backButtonPressTimer = setTimeout(() => {
                if (backButtonPressed) {
                    showBackHistoryDropdown();
                }
            }, LONG_PRESS_DELAY);
        });

        btn.addEventListener('touchend', () => {
            if (backButtonPressTimer) {
                clearTimeout(backButtonPressTimer);
                if (backButtonPressed && !state.backHistoryDropdownOpen) {
                    window.bridge.goBack();
                }
            }
            backButtonPressed = false;
        });

        btn.addEventListener('touchcancel', () => {
            if (backButtonPressTimer) {
                clearTimeout(backButtonPressTimer);
            }
            backButtonPressed = false;
        });

        // Prevent default click since we handle it manually
        btn.addEventListener('click', (e) => {
            e.preventDefault();
        });
    }

    /**
     * Bind forward button events for click and long-press
     */
    function bindForwardButtonEvents() {
        const btn = elements.btnForward;

        // Mouse events
        btn.addEventListener('mousedown', (e) => {
            if (btn.disabled) return;
            forwardButtonPressed = true;
            forwardButtonPressTimer = setTimeout(() => {
                if (forwardButtonPressed) {
                    showForwardHistoryDropdown();
                }
            }, LONG_PRESS_DELAY);
        });

        btn.addEventListener('mouseup', () => {
            if (forwardButtonPressTimer) {
                clearTimeout(forwardButtonPressTimer);
                if (forwardButtonPressed && !state.forwardHistoryDropdownOpen) {
                    window.bridge.goForward();
                }
            }
            forwardButtonPressed = false;
        });

        btn.addEventListener('mouseleave', () => {
            if (forwardButtonPressTimer) {
                clearTimeout(forwardButtonPressTimer);
            }
            forwardButtonPressed = false;
        });

        // Touch events for mobile/VR
        btn.addEventListener('touchstart', (e) => {
            if (btn.disabled) return;
            forwardButtonPressed = true;
            forwardButtonPressTimer = setTimeout(() => {
                if (forwardButtonPressed) {
                    showForwardHistoryDropdown();
                }
            }, LONG_PRESS_DELAY);
        });

        btn.addEventListener('touchend', () => {
            if (forwardButtonPressTimer) {
                clearTimeout(forwardButtonPressTimer);
                if (forwardButtonPressed && !state.forwardHistoryDropdownOpen) {
                    window.bridge.goForward();
                }
            }
            forwardButtonPressed = false;
        });

        btn.addEventListener('touchcancel', () => {
            if (forwardButtonPressTimer) {
                clearTimeout(forwardButtonPressTimer);
            }
            forwardButtonPressed = false;
        });

        // Prevent default click since we handle it manually
        btn.addEventListener('click', (e) => {
            e.preventDefault();
        });
    }

    /**
     * Show back history dropdown
     */
    function showBackHistoryDropdown() {
        closeAllDropdowns();
        state.backHistoryDropdownOpen = true;
        elements.backHistoryDropdown.style.display = 'block';
        requestAnimationFrame(() => {
            elements.backHistoryDropdown.classList.add('dropdown--open');
        });
        window.bridge?.notifyOverlayOpened();
        // Request history from Unity
        window.bridge.requestBackHistory();
    }

    /**
     * Close back history dropdown
     */
    function closeBackHistoryDropdown() {
        state.backHistoryDropdownOpen = false;
        if (elements.backHistoryDropdown) {
            elements.backHistoryDropdown.classList.remove('dropdown--open');
            setTimeout(() => {
                if (!state.backHistoryDropdownOpen) {
                    elements.backHistoryDropdown.style.display = 'none';
                }
            }, 200);
        }
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Show forward history dropdown
     */
    function showForwardHistoryDropdown() {
        closeAllDropdowns();
        state.forwardHistoryDropdownOpen = true;
        elements.forwardHistoryDropdown.style.display = 'block';
        requestAnimationFrame(() => {
            elements.forwardHistoryDropdown.classList.add('dropdown--open');
        });
        window.bridge?.notifyOverlayOpened();
        // Request history from Unity
        window.bridge.requestForwardHistory();
    }

    /**
     * Close forward history dropdown
     */
    function closeForwardHistoryDropdown() {
        state.forwardHistoryDropdownOpen = false;
        if (elements.forwardHistoryDropdown) {
            elements.forwardHistoryDropdown.classList.remove('dropdown--open');
            setTimeout(() => {
                if (!state.forwardHistoryDropdownOpen) {
                    elements.forwardHistoryDropdown.style.display = 'none';
                }
            }, 200);
        }
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Update back history dropdown content
     * @param {Array} historyItems - Array of {title, url, isCurrent} objects
     */
    function updateBackHistory(historyItems) {
        state.backHistory = historyItems || [];
        renderHistoryDropdown(elements.backHistoryDropdown, state.backHistory, 'back');
    }

    /**
     * Update forward history dropdown content
     * @param {Array} historyItems - Array of {title, url, isCurrent} objects
     */
    function updateForwardHistory(historyItems) {
        state.forwardHistory = historyItems || [];
        renderHistoryDropdown(elements.forwardHistoryDropdown, state.forwardHistory, 'forward');
    }

    /**
     * Render history dropdown items
     */
    function renderHistoryDropdown(dropdown, items, direction) {
        if (!dropdown) return;

        const container = dropdown.querySelector('.nav-history-dropdown__items');
        if (!container) return;

        container.innerHTML = '';

        if (!items || items.length === 0) {
            container.innerHTML = `<div class="nav-history-dropdown__empty">No ${direction} history</div>`;
            return;
        }

        items.forEach((item, index) => {
            const el = document.createElement('button');
            el.className = 'nav-history-dropdown__item';
            if (item.isCurrent) {
                el.classList.add('nav-history-dropdown__item--current');
            }

            el.innerHTML = `<span class="nav-history-dropdown__title">${escapeHtml(item.title || item.url || 'Unknown')}</span>`;

            el.addEventListener('click', () => {
                if (direction === 'back') {
                    closeBackHistoryDropdown();
                    window.bridge.goBackToIndex(index);
                } else {
                    closeForwardHistoryDropdown();
                    window.bridge.goForwardToIndex(index);
                }
            });

            container.appendChild(el);
        });
    }

    /**
     * Handle click outside dropdowns
     */
    function handleOutsideClick(e) {
        // Tab dropdown
        if (state.tabDropdownOpen &&
            !elements.tabsButton.contains(e.target) &&
            !elements.tabDropdown.contains(e.target)) {
            closeTabDropdown();
        }

        // Menu dropdown
        if (state.menuDropdownOpen &&
            !elements.btnMenu.contains(e.target) &&
            !elements.menuDropdown.contains(e.target)) {
            closeMenuDropdown();
        }

        // Back history dropdown
        if (state.backHistoryDropdownOpen &&
            !elements.btnBack.contains(e.target) &&
            !elements.backHistoryDropdown.contains(e.target)) {
            closeBackHistoryDropdown();
        }

        // Forward history dropdown
        if (state.forwardHistoryDropdownOpen &&
            !elements.btnForward.contains(e.target) &&
            !elements.forwardHistoryDropdown.contains(e.target)) {
            closeForwardHistoryDropdown();
        }
    }

    /**
     * Handle menu action
     */
    function handleMenuAction(action) {
        closeMenuDropdown();

        switch (action) {
            case 'history':
                openModal('history');
                window.bridge.requestHistory();
                break;
            case 'console':
                openModal('console');
                window.bridge.requestConsoleLog();
                break;
            case 'stats':
                openStatsModal();
                break;
            case 'toggle-stats-hud':
                toggleStatsHud();
                break;
            case 'close-stats-hud':
                hideStatsHud();
                break;
            case 'settings':
                openModal('settings');
                window.bridge.requestSettings();
                break;
            case 'fullscreen':
                window.bridge.toggleFullscreen();
                break;
            case 'vr-mode':
                window.bridge.toggleVRMode();
                break;
            case 'about':
                openModal('about');
                window.bridge.requestAboutInfo();
                break;
            case 'exit':
                openModal('exit');
                break;
            case 'clear-history':
                window.bridge.clearHistory();
                closeModal('history');
                break;
            case 'clear-console':
                clearConsoleOutput();
                break;
            case 'save-settings':
                window.bridge.saveSettings(getSettingsValues());
                closeModal('settings');
                showToast('Settings saved', 'success');
                break;
            case 'clear-cache':
                handleClearCache();
                break;
            case 'confirm-exit':
                window.bridge.exit();
                break;
            case 'open-docs':
                window.bridge.openExternalUrl('https://github.com/Five-Squared-Interactive/WebVerse/wiki');
                break;
            case 'open-issues':
                window.bridge.openExternalUrl('https://github.com/Five-Squared-Interactive/WebVerse/issues');
                break;
            case 'open-terms':
                window.bridge.openExternalUrl('https://github.com/Five-Squared-Interactive/WebVerse/blob/main/TermsAndConditions.md');
                break;
            case 'open-license':
                window.bridge.openExternalUrl('https://github.com/Five-Squared-Interactive/WebVerse/blob/main/LICENSE');
                break;
            case 'open-github':
                window.bridge.openExternalUrl('https://github.com/Five-Squared-Interactive/WebVerse');
                break;
        }
    }

    /**
     * Handle new tab click
     */
    function handleNewTab() {
        closeTabDropdown();
        window.bridge.newTab();
    }

    /**
     * Handle fullscreen toggle
     */
    function handleFullscreenToggle() {
        state.isFullscreen = !state.isFullscreen;
        window.bridge.toggleFullscreen();
        updateFullscreenButton();
    }

    /**
     * Update fullscreen button icon
     */
    function updateFullscreenButton() {
        const iconFullscreen = elements.btnFullscreen.querySelector('.icon-fullscreen');
        const iconUnfullscreen = elements.btnFullscreen.querySelector('.icon-unfullscreen');
        if (iconFullscreen && iconUnfullscreen) {
            iconFullscreen.style.display = state.isFullscreen ? 'none' : 'block';
            iconUnfullscreen.style.display = state.isFullscreen ? 'block' : 'none';
        }
    }

    /**
     * Handle VR toggle
     */
    function handleVRToggle() {
        window.bridge.toggleVRMode();
    }

    /**
     * Handle tab click
     */
    function handleTabClick(tabId) {
        closeTabDropdown();
        window.bridge.switchTab(tabId);
    }

    /**
     * Handle tab close click
     */
    function handleTabClose(e, tabId) {
        e.stopPropagation();
        window.bridge.closeTab(tabId);
    }

    // ===================
    // Public API (called from bridge.js)
    // ===================

    /**
     * Update tabs list
     */
    function updateTabs(tabs) {
        state.tabs = tabs || [];
        hideThumbnailPreview();
        renderTabs();
        updateTabsButton();
    }

    /**
     * Set active tab
     */
    function setActiveTab(tabId) {
        state.activeTabId = tabId;
        hideThumbnailPreview();
        renderTabs();
        updateTabsButton();

        // Update URL bar with active tab's URL
        const activeTab = state.tabs.find(t => t.id === tabId);
        if (activeTab && activeTab.url) {
            state.currentUrl = activeTab.url;
            elements.urlBar.value = activeTab.url;
        }
    }

    /**
     * Update navigation state
     */
    function updateNavState(canGoBack, canGoForward, canReload) {
        state.canGoBack = canGoBack;
        state.canGoForward = canGoForward;
        state.canReload = canReload;

        elements.btnBack.disabled = !canGoBack;
        elements.btnForward.disabled = !canGoForward;
        elements.btnReload.disabled = !canReload;
    }

    /**
     * Set mode (desktop or vr)
     */
    function setMode(mode) {
        state.mode = mode;
        if (mode === 'vr') {
            document.body.classList.add('vr-mode');
        } else {
            document.body.classList.remove('vr-mode');
        }
    }

    /**
     * Show chrome
     */
    function showChrome() {
        state.chromeVisible = true;
        elements.chrome.classList.remove('chrome--hidden');
        elements.chrome.classList.add('chrome--visible');
    }

    /**
     * Hide chrome
     */
    function hideChrome() {
        state.chromeVisible = false;
        closeAllDropdowns();
        elements.chrome.classList.remove('chrome--visible');
        elements.chrome.classList.add('chrome--hidden');
    }

    /**
     * Toggle chrome visibility
     */
    function toggleChrome() {
        if (state.chromeVisible) {
            hideChrome();
        } else {
            showChrome();
        }
    }

    /**
     * Show content frame (rounded border for web content area)
     */
    function showContentFrame() {
        if (contentFrame) {
            contentFrame.classList.add('content-frame--visible');
        }
    }

    /**
     * Hide content frame
     */
    function hideContentFrame() {
        if (contentFrame) {
            contentFrame.classList.remove('content-frame--visible');
        }
    }

    /**
     * Set content frame visibility based on whether content is loaded
     */
    function setContentFrameVisible(visible) {
        if (visible) {
            showContentFrame();
        } else {
            hideContentFrame();
        }
    }

    // ===================
    // Stats / Performance
    // ===================

    /**
     * Show the stats HUD overlay
     */
    function showStatsHud() {
        state.statsHudVisible = true;
        if (elements.statsHud) {
            elements.statsHud.style.display = 'block';
            // Send HUD bounds to Unity for precise raycast hit testing
            requestAnimationFrame(() => {
                const rect = elements.statsHud.getBoundingClientRect();
                window.bridge?.notifyHudBounds(rect);
            });
        }
        startStatsUpdates();
        window.bridge?.requestStats();
        console.log('[UI] Stats HUD shown');
    }

    /**
     * Hide the stats HUD overlay
     */
    function hideStatsHud() {
        state.statsHudVisible = false;
        if (elements.statsHud) {
            elements.statsHud.style.display = 'none';
        }
        // Only stop updates if modal is also closed
        if (!state.statsModalOpen) {
            stopStatsUpdates();
        }
        // Clear HUD hit rect in Unity
        window.bridge?.notifyHudHidden();
        console.log('[UI] Stats HUD hidden');
    }

    /**
     * Toggle the stats HUD overlay
     */
    function toggleStatsHud() {
        if (state.statsHudVisible) {
            hideStatsHud();
        } else {
            showStatsHud();
        }
    }

    /**
     * Open the stats modal
     */
    function openStatsModal() {
        state.statsModalOpen = true;
        openModal('stats');
        startStatsUpdates();
        window.bridge?.requestStats();
    }

    /**
     * Close the stats modal
     */
    function closeStatsModal() {
        state.statsModalOpen = false;
        closeModal('stats');
        // Only stop updates if HUD is also hidden
        if (!state.statsHudVisible) {
            stopStatsUpdates();
        }
    }

    /**
     * Start requesting stats updates from Unity
     */
    function startStatsUpdates() {
        if (statsUpdateInterval) return; // Already running
        statsUpdateInterval = setInterval(() => {
            window.bridge?.requestStats();
        }, STATS_UPDATE_RATE);
    }

    /**
     * Stop requesting stats updates
     */
    function stopStatsUpdates() {
        if (statsUpdateInterval) {
            clearInterval(statsUpdateInterval);
            statsUpdateInterval = null;
        }
    }

    /**
     * Update stats from Unity data
     * @param {Object} stats - Stats object from Unity
     */
    function updateStats(stats) {
        if (!stats) return;

        // Merge incoming stats with state
        if (stats.rendering) Object.assign(state.stats.rendering, stats.rendering);
        if (stats.system) Object.assign(state.stats.system, stats.system);
        if (stats.network) Object.assign(state.stats.network, stats.network);
        if (stats.world) Object.assign(state.stats.world, stats.world);

        // Update HUD if visible
        if (state.statsHudVisible) {
            updateStatsHud();
        }

        // Update modal if open
        if (state.statsModalOpen) {
            updateStatsModalDisplay();
        }
    }

    /**
     * Update the stats HUD display
     */
    function updateStatsHud() {
        const { rendering, system, network } = state.stats;

        // FPS
        const hudFps = document.getElementById('hud-fps');
        if (hudFps) {
            hudFps.textContent = Math.round(rendering.fps);
            hudFps.className = 'stats-hud__value ' + getFpsColorClass(rendering.fps);
        }

        // Frame time
        const hudFrameTime = document.getElementById('hud-frame-time');
        if (hudFrameTime) {
            hudFrameTime.textContent = rendering.frameTimeMs.toFixed(1) + ' ms';
        }

        // Memory
        const hudMemory = document.getElementById('hud-memory');
        if (hudMemory) {
            hudMemory.textContent = Math.round(system.usedMemoryMB) + ' MB';
        }

        // Ping
        const hudPing = document.getElementById('hud-ping');
        if (hudPing) {
            hudPing.textContent = network.isConnected ? Math.round(network.pingMs) + ' ms' : '--';
            hudPing.className = 'stats-hud__value ' + getPingColorClass(network.pingMs);
        }

        // Draw FPS graph
        drawFpsGraph('hud-fps-graph', rendering.fpsHistory || []);
    }

    /**
     * Update the stats modal display
     */
    function updateStatsModalDisplay() {
        const { rendering, system, network, world } = state.stats;

        // Rendering
        setText('stats-fps', Math.round(rendering.fps));
        setText('stats-frame-time', rendering.frameTimeMs.toFixed(1));
        setText('stats-gpu-memory', Math.round(rendering.gpuMemoryMB));

        // Draw FPS graph in modal
        drawFpsGraph('stats-fps-graph', rendering.fpsHistory || []);

        // System
        setText('stats-memory-used', Math.round(system.usedMemoryMB));
        setText('stats-memory-total', Math.round(system.totalMemoryMB));
        setText('stats-mono-used', Math.round(system.monoUsedMB));
        setText('stats-mono-heap', Math.round(system.monoHeapMB));
        setText('stats-gc-memory', Math.round(system.gcTotalMB));

        // Memory bars
        updateStatsBar('stats-memory-bar', system.usedMemoryMB, system.totalMemoryMB);
        updateStatsBar('stats-mono-bar', system.monoUsedMB, system.monoHeapMB);

        // Network
        updateConnectionStatus(network.connectionState, network.isConnected);

        // World
        setText('stats-entities', formatNumber(world.entityCount));
        setText('stats-physics', world.physicsBodies);
        setText('stats-audio', world.audioSources);
    }

    /**
     * Update a stats progress bar
     */
    function updateStatsBar(id, used, total) {
        const bar = document.getElementById(id);
        if (!bar) return;

        const percent = total > 0 ? (used / total) * 100 : 0;
        bar.style.width = Math.min(percent, 100) + '%';

        // Color based on usage
        bar.classList.remove('stats-bar__fill--warning', 'stats-bar__fill--danger');
        if (percent > 90) {
            bar.classList.add('stats-bar__fill--danger');
        } else if (percent > 70) {
            bar.classList.add('stats-bar__fill--warning');
        }
    }

    /**
     * Update connection status display
     */
    function updateConnectionStatus(connectionState, isConnected) {
        const el = document.getElementById('stats-connection');
        if (!el) return;

        const dot = el.querySelector('.stats-status-dot');
        const text = el.querySelector('span:last-child');

        if (dot) {
            dot.className = 'stats-status-dot';
            if (isConnected) {
                dot.classList.add('stats-status-dot--connected');
            } else if (connectionState === 'Reconnecting') {
                dot.classList.add('stats-status-dot--reconnecting');
            } else {
                dot.classList.add('stats-status-dot--disconnected');
            }
        }

        if (text) {
            text.textContent = connectionState || (isConnected ? 'Connected' : 'Disconnected');
        }
    }

    /**
     * Draw FPS graph on canvas
     */
    function drawFpsGraph(canvasId, fpsHistory) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const width = canvas.width;
        const height = canvas.height;

        // Clear
        ctx.clearRect(0, 0, width, height);

        if (!fpsHistory || fpsHistory.length < 2) return;

        // Determine scale (target 60 FPS as baseline)
        const maxFps = Math.max(60, ...fpsHistory);
        const minFps = Math.min(...fpsHistory);

        // Draw line
        ctx.beginPath();
        ctx.strokeStyle = getComputedStyle(document.documentElement).getPropertyValue('--color-accent').trim() || '#4a9eff';
        ctx.lineWidth = 1.5;

        const samples = fpsHistory.slice(-60); // Last 60 samples
        const step = width / (samples.length - 1);

        samples.forEach((fps, i) => {
            const x = i * step;
            const y = height - ((fps / maxFps) * (height - 4)) - 2;
            if (i === 0) {
                ctx.moveTo(x, y);
            } else {
                ctx.lineTo(x, y);
            }
        });

        ctx.stroke();

        // Draw target line at 60 FPS
        const targetY = height - ((60 / maxFps) * (height - 4)) - 2;
        ctx.beginPath();
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.2)';
        ctx.lineWidth = 1;
        ctx.setLineDash([2, 2]);
        ctx.moveTo(0, targetY);
        ctx.lineTo(width, targetY);
        ctx.stroke();
        ctx.setLineDash([]);
    }

    /**
     * Get color class based on FPS value
     */
    function getFpsColorClass(fps) {
        if (fps >= 55) return 'stats-hud__value--good';
        if (fps >= 30) return 'stats-hud__value--warning';
        return 'stats-hud__value--bad';
    }

    /**
     * Get color class based on ping value
     */
    function getPingColorClass(pingMs) {
        if (pingMs <= 50) return 'stats-hud__value--good';
        if (pingMs <= 100) return 'stats-hud__value--warning';
        return 'stats-hud__value--bad';
    }

    /**
     * Helper to set text content by ID
     */
    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    /**
     * Format large numbers (e.g., 1234567 -> "1.2M")
     */
    function formatNumber(num) {
        if (num >= 1000000) {
            return (num / 1000000).toFixed(1) + 'M';
        }
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    }

    /**
     * Set URL in URL bar
     */
    function setUrl(url) {
        state.currentUrl = url || '';
        elements.urlBar.value = state.currentUrl;
    }

    /**
     * Set loading state
     */
    function setLoading(isLoading) {
        state.isLoading = isLoading;
        elements.loadingIndicator.style.display = isLoading ? 'flex' : 'none';
    }

    /**
     * Update tab load state
     */
    function updateTabLoadState(tabId, loadState) {
        const tab = state.tabs.find(t => t.id === tabId);
        if (tab) {
            tab.loadState = loadState;
            renderTabs();
        }
    }

    /**
     * Show toast notification
     */
    function showToast(message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `toast toast--${type}`;
        toast.innerHTML = `
            <span class="toast__message">${escapeHtml(message)}</span>
            <button class="toast__close" aria-label="Dismiss">&times;</button>
        `;

        const closeBtn = toast.querySelector('.toast__close');
        closeBtn.addEventListener('click', () => removeToast(toast));

        elements.toastContainer.appendChild(toast);

        // Auto remove after duration
        if (duration > 0) {
            setTimeout(() => removeToast(toast), duration);
        }

        return toast;
    }

    /**
     * Remove toast
     */
    function removeToast(toast) {
        toast.classList.add('toast--removing');
        setTimeout(() => {
            toast.remove();
        }, 150);
    }

    // ===================
    // Modals
    // ===================

    /**
     * Open a modal
     */
    function openModal(modalId) {
        const modal = document.getElementById('modal-' + modalId);
        if (modal) {
            modal.classList.add('modal--open');
            // Add overlay click handler
            modal.addEventListener('click', handleModalOverlayClick);
            // Notify Unity so clicks reach the modal overlay
            window.bridge?.notifyOverlayOpened();
        }
    }

    /**
     * Close a modal
     */
    function closeModal(modalId) {
        const modal = document.getElementById('modal-' + modalId);
        if (modal) {
            modal.classList.remove('modal--open');
            modal.removeEventListener('click', handleModalOverlayClick);
        }
        // If no modals or dropdowns are open, release overlay input
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Close all modals
     */
    function closeAllModals() {
        document.querySelectorAll('.modal-overlay.modal--open').forEach(modal => {
            modal.classList.remove('modal--open');
        });
        if (!hasAnyOverlayOpen()) {
            window.bridge?.notifyOverlayClosed();
        }
    }

    /**
     * Check if any modal or dropdown is currently open
     */
    function hasAnyOverlayOpen() {
        if (document.querySelector('.modal-overlay.modal--open')) return true;
        if (state.tabDropdownOpen) return true;
        if (state.menuDropdownOpen) return true;
        if (state.backHistoryDropdownOpen) return true;
        if (state.forwardHistoryDropdownOpen) return true;
        return false;
    }

    /**
     * Handle modal overlay click (close on outside click)
     */
    function handleModalOverlayClick(e) {
        if (e.target.classList.contains('modal-overlay')) {
            e.target.classList.remove('modal--open');
            if (!hasAnyOverlayOpen()) {
                window.bridge?.notifyOverlayClosed();
            }
        }
    }

    /**
     * Update history list
     */
    function updateHistory(historyItems) {
        const container = document.getElementById('history-list');
        if (!container) return;

        container.innerHTML = '';

        if (!historyItems || historyItems.length === 0) {
            container.innerHTML = '<p style="color: var(--color-text-secondary); text-align: center; padding: var(--spacing-md);">No history yet</p>';
            return;
        }

        historyItems.forEach(item => {
            const el = document.createElement('button');
            el.className = 'history-item';

            // Format timestamp
            const dateTime = formatHistoryDateTime(item.timestamp);

            el.innerHTML = `
                <div class="history-item__datetime">
                    <div class="history-item__date">${escapeHtml(dateTime.date)}</div>
                    <div class="history-item__time">${escapeHtml(dateTime.time)}</div>
                </div>
                <div class="history-item__site">
                    <div class="history-item__name">${escapeHtml(item.name || item.site || 'Unknown')}</div>
                    <div class="history-item__url">${escapeHtml(item.url || item.site || '')}</div>
                </div>
            `;
            el.addEventListener('click', () => {
                closeModal('history');
                window.bridge.navigate(item.url || item.site);
            });
            container.appendChild(el);
        });
    }

    /**
     * Format history timestamp into date and time parts
     */
    function formatHistoryDateTime(timestamp) {
        if (!timestamp) {
            return { date: '', time: '' };
        }

        try {
            const date = new Date(timestamp);
            const now = new Date();
            const isToday = date.toDateString() === now.toDateString();
            const yesterday = new Date(now);
            yesterday.setDate(yesterday.getDate() - 1);
            const isYesterday = date.toDateString() === yesterday.toDateString();

            let dateStr;
            if (isToday) {
                dateStr = 'Today';
            } else if (isYesterday) {
                dateStr = 'Yesterday';
            } else {
                dateStr = date.toLocaleDateString(undefined, {
                    month: 'short',
                    day: 'numeric',
                    year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
                });
            }

            const timeStr = date.toLocaleTimeString(undefined, {
                hour: 'numeric',
                minute: '2-digit'
            });

            return { date: dateStr, time: timeStr };
        } catch {
            return { date: '', time: '' };
        }
    }

    /**
     * Update console output
     */
    function updateConsole(logLines) {
        const container = document.getElementById('console-output');
        if (!container) return;

        container.innerHTML = '';

        if (!logLines || logLines.length === 0) {
            addConsoleLine('msg', 'No log entries', new Date());
            return;
        }

        logLines.forEach(line => {
            const lineType = mapConsoleType(line.type || line.level);
            const timestamp = line.timestamp ? new Date(line.timestamp) : new Date();
            appendConsoleLine(container, lineType, line.message, timestamp);
        });

        // Scroll to bottom
        container.scrollTop = container.scrollHeight;

        // Apply current filters
        applyConsoleFilters();
    }

    /**
     * Add console line
     */
    function addConsoleLine(type, message, timestamp) {
        const container = document.getElementById('console-output');
        if (!container) return;

        const lineType = mapConsoleType(type);
        appendConsoleLine(container, lineType, message, timestamp || new Date());

        // Scroll to bottom
        container.scrollTop = container.scrollHeight;

        // Apply current filters
        applyConsoleFilters();
    }

    /**
     * Append a console line element
     */
    function appendConsoleLine(container, type, message, timestamp) {
        const el = document.createElement('div');
        el.className = `console-line console-line--${type}`;

        const timeStr = timestamp.toLocaleTimeString(undefined, {
            hour: 'numeric',
            minute: '2-digit',
            second: '2-digit'
        });

        const typeLabel = getConsoleTypeLabel(type);

        el.innerHTML = `
            <span class="console-line__time">${escapeHtml(timeStr)}</span>
            <span class="console-line__type">[${typeLabel}]</span>
            <span class="console-line__text">${escapeHtml(message)}</span>
        `;

        container.appendChild(el);
    }

    /**
     * Map console type to internal type
     */
    function mapConsoleType(type) {
        const typeMap = {
            'ScriptError': 'error',
            'ScriptWarning': 'warn',
            'ScriptDefault': 'msg',
            'ScriptDebug': 'debug',
            'Error': 'internal',
            'Warning': 'internal',
            'Default': 'internal',
            'Debug': 'internal',
            'error': 'error',
            'warn': 'warn',
            'warning': 'warn',
            'msg': 'msg',
            'message': 'msg',
            'info': 'msg',
            'debug': 'debug',
            'internal': 'internal'
        };
        return typeMap[type] || 'msg';
    }

    /**
     * Get console type label
     */
    function getConsoleTypeLabel(type) {
        const labels = {
            'error': 'Err',
            'warn': 'Warn',
            'msg': 'Msg',
            'debug': 'Deb',
            'internal': 'IMsg'
        };
        return labels[type] || 'Msg';
    }

    /**
     * Apply console filters based on checkbox state and search
     */
    function applyConsoleFilters() {
        const filtersContainer = document.getElementById('console-filters');
        if (!filtersContainer) return;

        // Get type filters
        const filters = {};
        filtersContainer.querySelectorAll('input[data-filter]').forEach(input => {
            filters[input.dataset.filter] = input.checked;
        });

        // Get search term
        const searchInput = document.getElementById('console-search');
        const searchClear = document.getElementById('console-search-clear');
        const searchTerm = searchInput ? searchInput.value.toLowerCase().trim() : '';

        // Show/hide clear button
        if (searchClear) {
            searchClear.style.display = searchTerm ? 'flex' : 'none';
        }

        document.querySelectorAll('.console-line').forEach(line => {
            // Check type filter
            let showByType = true;
            if (line.classList.contains('console-line--error')) {
                showByType = filters.error !== false;
            } else if (line.classList.contains('console-line--warn')) {
                showByType = filters.warn !== false;
            } else if (line.classList.contains('console-line--msg')) {
                showByType = filters.msg !== false;
            } else if (line.classList.contains('console-line--debug')) {
                showByType = filters.debug !== false;
            } else if (line.classList.contains('console-line--internal')) {
                showByType = filters.internal !== false;
            }

            // Check search filter
            let showBySearch = true;
            if (searchTerm) {
                const text = line.textContent.toLowerCase();
                showBySearch = text.includes(searchTerm);
            }

            line.style.display = (showByType && showBySearch) ? 'flex' : 'none';
        });
    }

    /**
     * Clear console search
     */
    function clearConsoleSearch() {
        const searchInput = document.getElementById('console-search');
        if (searchInput) {
            searchInput.value = '';
            applyConsoleFilters();
            searchInput.focus();
        }
    }

    /**
     * Clear console output
     */
    function clearConsoleOutput() {
        const container = document.getElementById('console-output');
        if (container) {
            container.innerHTML = '';
            addConsoleLine('msg', 'Console cleared', new Date());
        }
    }

    /**
     * Update settings UI with values from Unity
     */
    function updateSettings(settings) {
        if (!settings) return;

        // Update theme if provided
        if (settings.theme) {
            setTheme(settings.theme);
        }

        // Update each setting field
        const fieldMappings = {
            homeURL: 'setting-home-url',
            worldLoadTimeout: 'setting-world-load-timeout',
            storageMode: 'setting-storage-mode',
            maxStorageEntries: 'setting-max-storage-entries',
            maxStorageKeyLength: 'setting-max-key-length',
            maxStorageEntryLength: 'setting-max-entry-length',
            cacheDirectory: 'setting-cache-directory'
        };

        Object.entries(fieldMappings).forEach(([key, elementId]) => {
            const el = document.getElementById(elementId);
            if (el && settings[key] !== undefined) {
                el.value = settings[key];
            }
        });

        // Reset clear cache dropdown
        const clearCacheSelect = document.getElementById('setting-clear-cache');
        if (clearCacheSelect) {
            clearCacheSelect.value = '';
        }
    }

    /**
     * Get current settings values from form
     */
    function getSettingsValues() {
        const values = {};

        // Theme
        values.theme = state.theme;

        // Text and number inputs
        const homeUrl = document.getElementById('setting-home-url');
        if (homeUrl) values.homeURL = homeUrl.value;

        const worldLoadTimeout = document.getElementById('setting-world-load-timeout');
        if (worldLoadTimeout) values.worldLoadTimeout = parseInt(worldLoadTimeout.value) || 60;

        const storageMode = document.getElementById('setting-storage-mode');
        if (storageMode) values.storageMode = storageMode.value;

        const maxStorageEntries = document.getElementById('setting-max-storage-entries');
        if (maxStorageEntries) values.maxStorageEntries = parseInt(maxStorageEntries.value) || 1024;

        const maxKeyLength = document.getElementById('setting-max-key-length');
        if (maxKeyLength) values.maxStorageKeyLength = parseInt(maxKeyLength.value) || 256;

        const maxEntryLength = document.getElementById('setting-max-entry-length');
        if (maxEntryLength) values.maxStorageEntryLength = parseInt(maxEntryLength.value) || 4096;

        const cacheDirectory = document.getElementById('setting-cache-directory');
        if (cacheDirectory) values.cacheDirectory = cacheDirectory.value;

        return values;
    }

    /**
     * Handle clear cache action
     */
    function handleClearCache() {
        const clearCacheSelect = document.getElementById('setting-clear-cache');
        if (!clearCacheSelect || !clearCacheSelect.value) {
            showToast('Please select a time range', 'warning');
            return;
        }

        const timeRange = clearCacheSelect.value;
        window.bridge.clearCache(timeRange);
        clearCacheSelect.value = '';
        showToast('Cache cleared', 'success');
    }

    /**
     * Update about info
     */
    function updateAboutInfo(info) {
        if (info.title) {
            const el = document.getElementById('about-title');
            if (el) el.textContent = info.title;
        }
        if (info.version) {
            const el = document.getElementById('about-version');
            if (el) el.textContent = 'Version ' + info.version;
        }
        if (info.description) {
            const el = document.getElementById('about-description');
            if (el) el.textContent = info.description;
        }
    }

    // ===================
    // Rendering
    // ===================

    /**
     * Render tabs list
     */
    function renderTabs() {
        elements.tabList.innerHTML = '';

        state.tabs.forEach(tab => {
            const tabEl = createTabElement(tab);
            elements.tabList.appendChild(tabEl);
        });
    }

    /**
     * Create tab element
     */
    function createTabElement(tab) {
        const div = document.createElement('button');
        div.className = 'tab-item';

        if (tab.id === state.activeTabId) {
            div.classList.add('tab-item--active');
        }

        if (tab.loadState === 'loading') {
            div.classList.add('tab-item--loading');
        } else if (tab.loadState === 'error') {
            div.classList.add('tab-item--error');
        }

        div.setAttribute('aria-label', tab.displayName || 'Tab');
        if (tab.id === state.activeTabId) {
            div.setAttribute('aria-current', 'true');
        }

        div.innerHTML = `
            <div class="tab-item__icon">
                ${tab.icon ? `<img src="${escapeHtml(tab.icon)}" alt="">` : getDefaultIcon(tab)}
            </div>
            <div class="tab-item__info">
                <span class="tab-item__name">${escapeHtml(tab.displayName || 'New Tab')}</span>
                ${tab.url ? `<span class="tab-item__url">${escapeHtml(truncateUrl(tab.url))}</span>` : ''}
            </div>
            <button class="tab-item__close" aria-label="Close tab">&times;</button>
        `;

        // Tab click (switch)
        div.addEventListener('click', () => handleTabClick(tab.id));

        // Close button click
        const closeBtn = div.querySelector('.tab-item__close');
        closeBtn.addEventListener('click', (e) => handleTabClose(e, tab.id));

        // Thumbnail preview hover events (not for active tab)
        if (tab.id !== state.activeTabId) {
            div.addEventListener('mouseenter', () => {
                scheduleThumbnailPreview(tab.id, div);
            });
            div.addEventListener('mouseleave', () => {
                hideThumbnailPreview();
            });
        }

        return div;
    }

    /**
     * Get default icon for tab
     */
    function getDefaultIcon(tab) {
        if (tab.loadState === 'loading') {
            return '<div class="spinner" style="width:16px;height:16px;border-width:2px;"></div>';
        }
        if (tab.loadState === 'error') {
            return '!';
        }
        // Globe icon
        return '<svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor"><circle cx="8" cy="8" r="6" stroke="currentColor" stroke-width="1.5" fill="none"/><path d="M2 8h12M8 2c-2 2-2 10 0 12M8 2c2 2 2 10 0 12" stroke="currentColor" stroke-width="1" fill="none"/></svg>';
    }

    /**
     * Update tabs button appearance
     */
    function updateTabsButton() {
        const activeTab = state.tabs.find(t => t.id === state.activeTabId);

        if (activeTab && activeTab.icon) {
            elements.tabsButtonIcon.style.display = 'none';
            elements.tabsButtonWorldIcon.src = activeTab.icon;
            elements.tabsButtonWorldIcon.style.display = 'block';
        } else {
            elements.tabsButtonIcon.style.display = 'flex';
            elements.tabsButtonWorldIcon.style.display = 'none';
            // Show overlapping tabs icon for multiple tabs, or plus icon for single/no tabs
            if (state.tabs.length > 1) {
                elements.tabsButtonIcon.innerHTML = '<svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="7" width="13" height="11" rx="2" fill="currentColor" opacity="0.25"/><rect x="8" y="3" width="13" height="11" rx="2"/></svg>';
            } else {
                elements.tabsButtonIcon.innerHTML = '<svg width="36" height="36" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"><path d="M8 2v12M2 8h12"/></svg>';
            }
        }
    }

    // ===================
    // Utilities
    // ===================

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    /**
     * Truncate URL for display
     */
    function truncateUrl(url) {
        if (!url) return '';
        try {
            const parsed = new URL(url);
            let display = parsed.host + parsed.pathname;
            if (display.length > 40) {
                display = display.substring(0, 37) + '...';
            }
            return display;
        } catch {
            return url.length > 40 ? url.substring(0, 37) + '...' : url;
        }
    }

    // ===================
    // Expose API
    // ===================

    window.tabUI = {
        updateTabs,
        setActiveTab,
        updateNavState,
        setMode,
        showChrome,
        hideChrome,
        toggleChrome,
        setUrl,
        setLoading,
        updateTabLoadState,
        showToast,
        // Content frame API
        showContentFrame,
        hideContentFrame,
        setContentFrameVisible,
        // Theme API
        setTheme,
        getTheme,
        // Navigation history API
        updateBackHistory,
        updateForwardHistory,
        closeBackHistoryDropdown,
        closeForwardHistoryDropdown,
        // Stats API
        showStatsHud,
        hideStatsHud,
        toggleStatsHud,
        openStatsModal,
        closeStatsModal,
        updateStats,
        // Thumbnail API
        updateTabThumbnail,
        showThumbnailPreview,
        hideThumbnailPreview,
        // Modal API
        openModal,
        closeModal,
        closeAllModals,
        updateHistory,
        updateConsole,
        addConsoleLine,
        updateSettings,
        updateAboutInfo
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
