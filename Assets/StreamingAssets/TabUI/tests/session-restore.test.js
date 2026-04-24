import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const bridgeJsPath = resolve(__dirname, '../scripts/bridge.js');
const bridgeJsSource = readFileSync(bridgeJsPath, 'utf-8');

/**
 * Load bridge.js after ui.js is loaded.
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
        const handlers = window.vuplex?._listeners?.message || [];
        const event = { data: JSON.stringify(data) };
        handlers.forEach(h => h(event));
    }
}

describe('restoreSession bridge message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('restoreSession message with tab data updates DOM with tabs', () => {
        const tabs = [
            { id: 'tab-1', url: 'http://world1.com', displayName: 'World 1', loadState: 'loaded', isActive: false },
            { id: 'tab-2', url: 'http://world2.com', displayName: 'World 2', loadState: 'loaded', isActive: false }
        ];

        simulateUnityMessage({ type: 'restoreSession', tabs: tabs, activeTabId: 'tab-2' });

        // DOM side effect — tab items should exist (exclude new-tab button)
        const tabItems = document.querySelectorAll('.tab-item:not(.tab-item--new)');
        expect(tabItems.length).toBe(2);
    });

    it('restoreSession message with empty tabs array clears tab list', () => {
        // First add some tabs
        tabUI.updateTabs([
            { id: 'tab-x', url: 'http://x.com', displayName: 'X', loadState: 'loaded', isActive: true }
        ]);
        expect(document.querySelectorAll('.tab-item:not(.tab-item--new)').length).toBe(1);

        // Now restore with empty
        simulateUnityMessage({ type: 'restoreSession', tabs: [], activeTabId: '' });

        const tabItems = document.querySelectorAll('.tab-item:not(.tab-item--new)');
        expect(tabItems.length).toBe(0);
    });

    it('restoreSession message with reloading tab triggers toast', () => {
        const tabs = [
            { id: 'tab-1', url: 'http://world1.com', displayName: 'World 1', loadState: 'loaded', isActive: false, reloading: true }
        ];

        simulateUnityMessage({ type: 'restoreSession', tabs: tabs, activeTabId: 'tab-1', hasReloadingTab: true });

        // Check DOM side effect — toast element should be added to toast container
        const toasts = document.querySelectorAll('.toast');
        expect(toasts.length).toBeGreaterThan(0);
        const toastText = Array.from(toasts).map(t => t.textContent).join(' ');
        expect(toastText).toContain('Reloading');
    });
});

describe('showRestorePrompt bridge message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('showRestorePrompt renders a modal with Restore session title', () => {
        simulateUnityMessage({ type: 'showRestorePrompt' });

        const modal = document.querySelector('[role="dialog"]');
        expect(modal).toBeTruthy();
        expect(modal.textContent).toContain('Restore session?');
    });

    it('showRestorePrompt modal has Accept and Decline buttons', () => {
        simulateUnityMessage({ type: 'showRestorePrompt' });

        const modal = document.querySelector('[role="dialog"]');
        expect(modal).toBeTruthy();

        const acceptBtn = modal.querySelector('[data-action="accept"]');
        const declineBtn = modal.querySelector('[data-action="decline"]');
        expect(acceptBtn).toBeTruthy();
        expect(declineBtn).toBeTruthy();
    });

    it('clicking Accept button calls bridge.acceptSessionRestore()', () => {
        simulateUnityMessage({ type: 'showRestorePrompt' });

        const modal = document.querySelector('[role="dialog"]');
        expect(modal).toBeTruthy();

        const postMessageSpy = vi.spyOn(window.vuplex, 'postMessage');

        const acceptBtn = modal.querySelector('[data-action="accept"]');
        acceptBtn.click();

        // Verify an acceptSessionRestore message was sent to Unity
        const calls = postMessageSpy.mock.calls.map(c => JSON.parse(c[0]));
        const restoreCall = calls.find(c => c.type === 'acceptSessionRestore');
        expect(restoreCall).toBeTruthy();

        postMessageSpy.mockRestore();
    });

    it('clicking Decline button calls bridge.declineSessionRestore()', () => {
        simulateUnityMessage({ type: 'showRestorePrompt' });

        const modal = document.querySelector('[role="dialog"]');
        expect(modal).toBeTruthy();

        const postMessageSpy = vi.spyOn(window.vuplex, 'postMessage');

        const declineBtn = modal.querySelector('[data-action="decline"]');
        declineBtn.click();

        const calls = postMessageSpy.mock.calls.map(c => JSON.parse(c[0]));
        const clearCall = calls.find(c => c.type === 'declineSessionRestore');
        expect(clearCall).toBeTruthy();

        postMessageSpy.mockRestore();
    });

    it('showRestorePrompt modal has correct accessibility attributes', () => {
        simulateUnityMessage({ type: 'showRestorePrompt' });

        const modal = document.querySelector('[role="dialog"]');
        expect(modal).toBeTruthy();
        expect(modal.getAttribute('aria-label')).toBeTruthy();
    });
});

describe('showReloadingToast bridge message handling', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('showReloadingToast shows toast with Reloading world text', () => {
        simulateUnityMessage({ type: 'showReloadingToast' });

        // Check DOM side effect — toast element should appear
        const toasts = document.querySelectorAll('.toast');
        expect(toasts.length).toBeGreaterThan(0);
        const toastText = Array.from(toasts).map(t => t.textContent).join(' ');
        expect(toastText).toContain('Reloading');
    });
});

describe('bridge outgoing session methods', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('bridge.acceptSessionRestore sends acceptSessionRestore message to Unity', () => {
        const postMessageSpy = vi.spyOn(window.vuplex, 'postMessage');

        window.bridge.acceptSessionRestore();

        const calls = postMessageSpy.mock.calls.map(c => JSON.parse(c[0]));
        const restoreCall = calls.find(c => c.type === 'acceptSessionRestore');
        expect(restoreCall).toBeTruthy();

        postMessageSpy.mockRestore();
    });

    it('bridge.declineSessionRestore sends declineSessionRestore message to Unity', () => {
        const postMessageSpy = vi.spyOn(window.vuplex, 'postMessage');

        window.bridge.declineSessionRestore();

        const calls = postMessageSpy.mock.calls.map(c => JSON.parse(c[0]));
        const clearCall = calls.find(c => c.type === 'declineSessionRestore');
        expect(clearCall).toBeTruthy();

        postMessageSpy.mockRestore();
    });
});
