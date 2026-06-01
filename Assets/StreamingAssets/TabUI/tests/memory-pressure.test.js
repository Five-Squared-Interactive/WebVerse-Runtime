import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const bridgeJsPath = resolve(__dirname, '../scripts/bridge.js');
const bridgeJsSource = readFileSync(bridgeJsPath, 'utf-8');

function loadBridge() {
    const fn = new Function(bridgeJsSource);
    fn.call(window);
}

function simulateUnityMessage(data) {
    if (window.vuplex && window.vuplex.simulateMessage) {
        window.vuplex.simulateMessage(data);
    } else {
        const handlers = window.vuplex?._listeners?.message || [];
        const event = { data: JSON.stringify(data) };
        handlers.forEach(h => h(event));
    }
}

describe('suspended tab rendering', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('tab with loadState suspended gets tab-item--suspended class on correct element', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'World A', loadState: 'suspended', isActive: false },
            { id: 'tab-2', url: 'http://b.com', displayName: 'World B', loadState: 'loaded', isActive: true }
        ]);

        const suspendedTab = document.querySelector('[aria-label="World A (suspended)"]');
        expect(suspendedTab).toBeTruthy();
        expect(suspendedTab.classList.contains('tab-item--suspended')).toBe(true);

        const loadedTab = document.querySelector('[aria-label="World B"]');
        expect(loadedTab).toBeTruthy();
        expect(loadedTab.classList.contains('tab-item--suspended')).toBe(false);
    });

    it('updateTabLoadState with suspended adds suspended class to correct tab', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'World A', loadState: 'loaded', isActive: false },
            { id: 'tab-2', url: 'http://b.com', displayName: 'World B', loadState: 'loaded', isActive: true }
        ]);

        // No suspended tabs initially
        expect(document.querySelectorAll('.tab-item--suspended').length).toBe(0);

        tabUI.updateTabLoadState('tab-1', 'suspended');

        const suspendedTab = document.querySelector('[aria-label="World A (suspended)"]');
        expect(suspendedTab).toBeTruthy();
        expect(suspendedTab.classList.contains('tab-item--suspended')).toBe(true);
    });

    it('tab list renders mix of loaded and suspended tabs correctly', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'World A', loadState: 'loaded', isActive: true },
            { id: 'tab-2', url: 'http://b.com', displayName: 'World B', loadState: 'suspended', isActive: false },
            { id: 'tab-3', url: 'http://c.com', displayName: 'World C', loadState: 'loaded', isActive: false },
            { id: 'tab-4', url: 'http://d.com', displayName: 'World D', loadState: 'suspended', isActive: false }
        ]);

        const allTabs = document.querySelectorAll('.tab-item:not(.tab-item--new)');
        const suspendedTabs = document.querySelectorAll('.tab-item--suspended');
        expect(allTabs.length).toBe(4);
        expect(suspendedTabs.length).toBe(2);
    });

    it('suspended tab retains displayName in dropdown', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'My World', loadState: 'suspended', isActive: false }
        ]);

        const tabName = document.querySelector('.tab-item__name');
        expect(tabName).toBeTruthy();
        expect(tabName.textContent).toContain('My World');
    });

    it('suspended tab retains URL in dropdown', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'My World', loadState: 'suspended', isActive: false }
        ]);

        const tabUrl = document.querySelector('.tab-item__url');
        expect(tabUrl).toBeTruthy();
        expect(tabUrl.textContent).toContain('a.com');
    });

    it('suspended tab has accessible aria-label with suspended indicator', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'World A', loadState: 'suspended', isActive: false }
        ]);

        const tab = document.querySelector('.tab-item--suspended');
        expect(tab).toBeTruthy();
        expect(tab.getAttribute('aria-label')).toBe('World A (suspended)');
    });

    it('suspended tab transitions back to loaded removes suspended class', () => {
        tabUI.updateTabs([
            { id: 'tab-1', url: 'http://a.com', displayName: 'World A', loadState: 'suspended', isActive: false },
            { id: 'tab-2', url: 'http://b.com', displayName: 'World B', loadState: 'loaded', isActive: true }
        ]);

        // Verify suspended
        expect(document.querySelectorAll('.tab-item--suspended').length).toBe(1);

        // Transition back to loaded
        tabUI.updateTabLoadState('tab-1', 'loaded');

        // Verify suspended class removed
        expect(document.querySelectorAll('.tab-item--suspended').length).toBe(0);
        const tab = document.querySelector('[aria-label="World A"]');
        expect(tab).toBeTruthy();
        expect(tab.classList.contains('tab-item--suspended')).toBe(false);
    });
});

describe('switching to suspended tab via bridge', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        loadBridge();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('showReloadingToast bridge message shows toast with Reloading text', () => {
        simulateUnityMessage({ type: 'showReloadingToast' });

        const toasts = document.querySelectorAll('.toast');
        expect(toasts.length).toBeGreaterThan(0);
        const toastText = Array.from(toasts).map(t => t.textContent).join(' ');
        expect(toastText).toContain('Reloading');
    });
});
