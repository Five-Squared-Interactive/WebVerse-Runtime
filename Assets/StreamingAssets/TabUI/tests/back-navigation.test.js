import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('evaluateBackAction pure function', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should return navigate-back when Android has back history', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: true, chromeVisible: true, hasOverlayOpen: false, platform: 'android' });
        expect(result).toEqual({ action: 'navigate-back' });
    });

    it('should return hide-chrome when Android has no history but chrome visible', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: true, hasOverlayOpen: false, platform: 'android' });
        expect(result).toEqual({ action: 'hide-chrome' });
    });

    it('should return show-exit-dialog when Android has no history and chrome hidden', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: false, hasOverlayOpen: false, platform: 'android' });
        expect(result).toEqual({ action: 'show-exit-dialog' });
    });

    it('should return navigate-back when iOS has back history', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: true, chromeVisible: true, hasOverlayOpen: false, platform: 'ios' });
        expect(result).toEqual({ action: 'navigate-back' });
    });

    it('should return hide-chrome when iOS has no history but chrome visible', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: true, hasOverlayOpen: false, platform: 'ios' });
        expect(result).toEqual({ action: 'hide-chrome' });
    });

    it('should return none when iOS has no history and chrome hidden (iOS handles exit)', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: false, hasOverlayOpen: false, platform: 'ios' });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return close-overlay when any overlay is open (highest priority)', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: true, chromeVisible: true, hasOverlayOpen: true, platform: 'android' });
        expect(result).toEqual({ action: 'close-overlay' });
    });

    it('should return close-overlay even with no history when overlay open', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: false, hasOverlayOpen: true, platform: 'ios' });
        expect(result).toEqual({ action: 'close-overlay' });
    });

    it('should return none for null/undefined/empty input (defensive)', () => {
        expect(tabUI.evaluateBackAction(null)).toEqual({ action: 'none' });
        expect(tabUI.evaluateBackAction(undefined)).toEqual({ action: 'none' });
        expect(tabUI.evaluateBackAction({})).toEqual({ action: 'none' });
    });

    it('should return none for desktop platform', () => {
        const result = tabUI.evaluateBackAction({ canGoBack: true, chromeVisible: true, hasOverlayOpen: false, platform: 'desktop' });
        expect(result).toEqual({ action: 'none' });
    });

    it('should ignore invalid platform in setPlatform and keep previous value', () => {
        tabUI.setPlatform('android');
        // Verify android is set by checking evaluateBackAction behavior
        const androidResult = tabUI.evaluateBackAction({ canGoBack: false, chromeVisible: false, hasOverlayOpen: false, platform: 'android' });
        expect(androidResult).toEqual({ action: 'show-exit-dialog' });
        // Now set invalid — should be ignored
        tabUI.setPlatform('invalid');
        tabUI.setPlatform(null);
        tabUI.setPlatform(undefined);
        // Platform should still be android — verify via handlePlatformBack triggering exit dialog
        tabUI.updateNavState(false, false, false);
        tabUI.hideChrome();
        window.bridge = { goBack: vi.fn(), showExitDialog: vi.fn(), notifyThemeChange: vi.fn(), switchTab: vi.fn(), notifyOverlayOpened: vi.fn(), notifyOverlayClosed: vi.fn() };
        tabUI.handlePlatformBack();
        expect(window.bridge.showExitDialog).toHaveBeenCalled();
        delete window.bridge;
    });
});

describe('handlePlatformBack integration', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        // Set up mock bridge (loadUI doesn't load bridge.js)
        window.bridge = {
            goBack: vi.fn(),
            showExitDialog: vi.fn(),
            notifyThemeChange: vi.fn(),
            switchTab: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
    });

    afterEach(() => {
        cleanupUI();
        delete window.bridge;
    });

    it('should call bridge.goBack when canGoBack is true', () => {
        tabUI.setPlatform('android');
        tabUI.updateNavState(true, false, false);
        tabUI.showChrome();
        tabUI.handlePlatformBack();
        expect(window.bridge.goBack).toHaveBeenCalled();
    });

    it('should hide chrome when canGoBack is false and chrome is visible', () => {
        tabUI.setPlatform('android');
        tabUI.updateNavState(false, false, false);
        tabUI.showChrome();
        tabUI.handlePlatformBack();
        const chrome = document.querySelector('.chrome');
        expect(chrome.classList.contains('chrome--hidden')).toBe(true);
    });

    it('should call bridge.showExitDialog when Android, no history, chrome hidden', () => {
        tabUI.setPlatform('android');
        tabUI.updateNavState(false, false, false);
        tabUI.hideChrome();
        tabUI.handlePlatformBack();
        expect(window.bridge.showExitDialog).toHaveBeenCalled();
    });

    it('should do nothing on iOS when no history and chrome hidden (iOS handles exit)', () => {
        tabUI.setPlatform('ios');
        tabUI.updateNavState(false, false, false);
        tabUI.hideChrome();
        tabUI.handlePlatformBack();
        // Chrome should stay hidden (no exit dialog on iOS)
        const chrome = document.querySelector('.chrome');
        expect(chrome.classList.contains('chrome--hidden')).toBe(true);
        expect(window.bridge.showExitDialog).not.toHaveBeenCalled();
        expect(window.bridge.goBack).not.toHaveBeenCalled();
    });

    it('should close overlay when dropdown is open', () => {
        tabUI.setPlatform('android');
        tabUI.updateNavState(true, false, false);
        tabUI.showChrome();
        // Open tab dropdown — click triggers toggleTabDropdown which sets
        // state.tabDropdownOpen=true and display='block' synchronously
        const tabsButton = document.querySelector('.tabs-button');
        expect(tabsButton).not.toBeNull();
        tabsButton.click();
        // Verify dropdown opened (display set synchronously, class via rAF)
        const tabDropdown = document.getElementById('tab-dropdown');
        expect(tabDropdown.style.display).toBe('block');
        expect(tabsButton.getAttribute('aria-expanded')).toBe('true');
        tabUI.handlePlatformBack();
        // closeAllDropdowns → closeTabDropdown sets aria-expanded=false synchronously
        // (style.display='none' is deferred via setTimeout(200), so check aria instead)
        expect(tabsButton.getAttribute('aria-expanded')).toBe('false');
    });
});
