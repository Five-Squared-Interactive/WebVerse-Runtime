import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const bridgeJsPath = resolve(__dirname, '../scripts/bridge.js');
const bridgeJsSource = readFileSync(bridgeJsPath, 'utf-8');

/**
 * Load bridge.js after ui.js is loaded.
 * Bridge expects window.tabUI to exist.
 */
function loadBridge() {
    const fn = new Function(bridgeJsSource);
    fn.call(window);
}

/**
 * Simulate a message from Unity to the bridge.
 */
function simulateUnityMessage(data) {
    if (window.vuplex && window.vuplex.simulateMessage) {
        window.vuplex.simulateMessage(data);
    } else {
        // Dispatch message event on vuplex
        const handlers = window.vuplex?._listeners?.message || [];
        const event = { data: JSON.stringify(data) };
        handlers.forEach(h => h(event));
    }
}

describe('bridge setMode message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should call setMode("mobile") when bridge receives setMode message', () => {
        const spy = vi.spyOn(tabUI, 'setMode');
        simulateUnityMessage({ type: 'setMode', mode: 'mobile' });
        expect(spy).toHaveBeenCalledWith('mobile');
        spy.mockRestore();
    });

    it('should call setMode("tablet") when bridge receives setMode message', () => {
        const spy = vi.spyOn(tabUI, 'setMode');
        simulateUnityMessage({ type: 'setMode', mode: 'tablet' });
        expect(spy).toHaveBeenCalledWith('tablet');
        spy.mockRestore();
    });

    it('should handle unknown mode gracefully without crashing', () => {
        expect(() => {
            simulateUnityMessage({ type: 'setMode', mode: 'unknown_mode' });
        }).not.toThrow();
    });
});

describe('bridge setSafeArea message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should call setSafeArea with insets when bridge receives setSafeArea message', () => {
        const spy = vi.spyOn(tabUI, 'setSafeArea');
        const insets = { top: 44, bottom: 34, left: 0, right: 0 };
        simulateUnityMessage({ type: 'setSafeArea', ...insets });
        expect(spy).toHaveBeenCalledWith(expect.objectContaining(insets));
        spy.mockRestore();
    });

    it('should call setChromePosition when bridge receives setChromePosition message', () => {
        const spy = vi.spyOn(tabUI, 'setChromePosition');
        simulateUnityMessage({ type: 'setChromePosition', position: 'top' });
        expect(spy).toHaveBeenCalledWith('top');
        spy.mockRestore();
    });

    it('should call setOrientation when bridge receives setOrientation message', () => {
        const spy = vi.spyOn(tabUI, 'setOrientation');
        simulateUnityMessage({ type: 'setOrientation', orientation: 'landscape' });
        expect(spy).toHaveBeenCalledWith('landscape');
        spy.mockRestore();
    });

    it('should call setKeyboardState when bridge receives setKeyboardState message', () => {
        const spy = vi.spyOn(tabUI, 'setKeyboardState');
        simulateUnityMessage({ type: 'setKeyboardState', visible: true, height: 300 });
        expect(spy).toHaveBeenCalledWith(expect.objectContaining({ visible: true, height: 300 }));
        spy.mockRestore();
    });
});

describe('bridge auto-hide message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should call startAutoHideTimer when bridge receives startAutoHide message', () => {
        const spy = vi.spyOn(tabUI, 'startAutoHideTimer');
        simulateUnityMessage({ type: 'startAutoHide' });
        expect(spy).toHaveBeenCalled();
        spy.mockRestore();
    });

    it('should call stopAutoHideTimer when bridge receives stopAutoHide message', () => {
        const spy = vi.spyOn(tabUI, 'stopAutoHideTimer');
        simulateUnityMessage({ type: 'stopAutoHide' });
        expect(spy).toHaveBeenCalled();
        spy.mockRestore();
    });

    it('should call handleEdgeTap when bridge receives edgeTap message', () => {
        const spy = vi.spyOn(tabUI, 'handleEdgeTap');
        simulateUnityMessage({ type: 'edgeTap', y: 10, screenHeight: 800 });
        expect(spy).toHaveBeenCalledWith(10, 800);
        spy.mockRestore();
    });

    it('should call handlePlatformBack when bridge receives platformBack message', () => {
        const spy = vi.spyOn(tabUI, 'handlePlatformBack');
        simulateUnityMessage({ type: 'platformBack' });
        expect(spy).toHaveBeenCalled();
        spy.mockRestore();
    });

    it('should call setPlatform when bridge receives setPlatform message', () => {
        const spy = vi.spyOn(tabUI, 'setPlatform');
        simulateUnityMessage({ type: 'setPlatform', platform: 'android' });
        expect(spy).toHaveBeenCalledWith('android');
        spy.mockRestore();
    });
});

describe('bridge setMobileTabLimit message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should call setMobileTabLimit when bridge receives setMobileTabLimit message', () => {
        const spy = vi.spyOn(tabUI, 'setMobileTabLimit');
        simulateUnityMessage({ type: 'setMobileTabLimit', limit: 5 });
        expect(spy).toHaveBeenCalledWith(5);
        spy.mockRestore();
    });

    it('should not force-close tabs when limit is set below current tab count', () => {
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' },
            { id: 't2', title: 'Tab 2', url: 'https://b.com' },
            { id: 't3', title: 'Tab 3', url: 'https://c.com' },
            { id: 't4', title: 'Tab 4', url: 'https://d.com' },
            { id: 't5', title: 'Tab 5', url: 'https://e.com' }
        ]);
        tabUI.setActiveTab('t1');
        window.bridge = {
            closeTab: vi.fn(),
            switchTab: vi.fn(),
            newTab: vi.fn(),
            notifyThemeChange: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
        simulateUnityMessage({ type: 'setMobileTabLimit', limit: 3 });
        expect(window.bridge.closeTab).not.toHaveBeenCalled();
        // All 5 tabs still present — limit only prevents new tabs
        expect(tabUI.canOpenNewTab()).toBe(false);
        delete window.bridge;
    });
});
