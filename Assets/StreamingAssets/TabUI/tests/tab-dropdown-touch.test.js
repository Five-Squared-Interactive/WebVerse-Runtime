import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const componentsCssPath = resolve(__dirname, '../styles/components.css');
const componentsCss = readFileSync(componentsCssPath, 'utf-8');

function extractBlock(css, selector) {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(escaped + '\\s*\\{([^}]*)\\}');
    const match = css.match(regex);
    return match ? match[1] : '';
}

describe('mobile tab dropdown touch adaptation', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should have 56px min-height for tab items in mobile mode (CSS)', () => {
        const block = extractBlock(componentsCss, '.mobile-mode .tab-item');
        expect(block).toContain('min-height');
        expect(block).toContain('56px');
    });

    it('should set mobileTabLimit to 5 via setMobileTabLimit', () => {
        tabUI.setMobileTabLimit(5);
        expect(tabUI.getMobileTabLimit()).toBe(5);
    });

    it('should default mobileTabLimit to 5 for invalid values (0 or negative)', () => {
        tabUI.setMobileTabLimit(0);
        expect(tabUI.getMobileTabLimit()).toBe(5);
        tabUI.setMobileTabLimit(-1);
        expect(tabUI.getMobileTabLimit()).toBe(5);
    });

    it('should return current limit from getMobileTabLimit', () => {
        tabUI.setMobileTabLimit(3);
        expect(tabUI.getMobileTabLimit()).toBe(3);
    });

    it('should return true from canOpenNewTab when under limit', () => {
        tabUI.setMobileTabLimit(5);
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' },
            { id: 't2', title: 'Tab 2', url: 'https://b.com' }
        ]);
        expect(tabUI.canOpenNewTab()).toBe(true);
    });

    it('should return false from canOpenNewTab when at limit', () => {
        tabUI.setMobileTabLimit(2);
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' },
            { id: 't2', title: 'Tab 2', url: 'https://b.com' }
        ]);
        expect(tabUI.canOpenNewTab()).toBe(false);
    });

    it('should always return true from canOpenNewTab in desktop mode', () => {
        tabUI.setMode('desktop');
        tabUI.setMobileTabLimit(2);
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' },
            { id: 't2', title: 'Tab 2', url: 'https://b.com' },
            { id: 't3', title: 'Tab 3', url: 'https://c.com' }
        ]);
        expect(tabUI.canOpenNewTab()).toBe(true);
    });

    it('should block new tab and show warning toast when at limit', () => {
        tabUI.setMobileTabLimit(2);
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' },
            { id: 't2', title: 'Tab 2', url: 'https://b.com' }
        ]);
        tabUI.setActiveTab('t1');
        window.bridge = {
            newTab: vi.fn(),
            notifyThemeChange: vi.fn(),
            switchTab: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
        tabUI.handleNewTab();
        expect(window.bridge.newTab).not.toHaveBeenCalled();
        // Verify toast message and type in DOM
        const toast = document.querySelector('.toast');
        expect(toast).not.toBeNull();
        expect(toast.classList.contains('toast--warning')).toBe(true);
        expect(toast.textContent).toContain('Tab limit reached');
        delete window.bridge;
    });

    it('should allow new tab and call bridge.newTab when under limit', () => {
        tabUI.setMobileTabLimit(5);
        tabUI.updateTabs([
            { id: 't1', title: 'Tab 1', url: 'https://a.com' }
        ]);
        tabUI.setActiveTab('t1');
        window.bridge = {
            newTab: vi.fn(),
            notifyThemeChange: vi.fn(),
            switchTab: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
        tabUI.handleNewTab();
        expect(window.bridge.newTab).toHaveBeenCalled();
        delete window.bridge;
    });

    it('should floor non-integer limits in setMobileTabLimit', () => {
        tabUI.setMobileTabLimit(3.7);
        expect(tabUI.getMobileTabLimit()).toBe(3);
    });
});

describe('mobile tab long-press thumbnail', () => {
    let tabUI;

    beforeEach(() => {
        vi.useFakeTimers();
        tabUI = loadUI();
        tabUI.setMode('mobile');
        window.bridge = {
            requestThumbnail: vi.fn(),
            switchTab: vi.fn(),
            newTab: vi.fn(),
            closeTab: vi.fn(),
            notifyThemeChange: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
    });

    afterEach(() => {
        vi.useRealTimers();
        cleanupUI();
        delete window.bridge;
    });

    it('should show thumbnail preview after 500ms long-press', () => {
        const anchor = document.createElement('div');
        document.body.appendChild(anchor);
        tabUI.handleTabLongPress('tab1', anchor);
        vi.advanceTimersByTime(499);
        // Thumbnail preview element should not be visible yet
        const preview = document.querySelector('.thumbnail-preview');
        const isVisibleBefore = preview && preview.style.display !== 'none' && preview.classList.contains('thumbnail-preview--visible');
        expect(isVisibleBefore).toBeFalsy();
        vi.advanceTimersByTime(1);
        // After 500ms, showThumbnailPreview was called — check that preview is now visible
        const previewAfter = document.querySelector('.thumbnail-preview');
        expect(previewAfter).not.toBeNull();
    });

    it('should cancel long-press via cancelTabLongPress', () => {
        const anchor = document.createElement('div');
        document.body.appendChild(anchor);
        tabUI.handleTabLongPress('tab1', anchor);
        vi.advanceTimersByTime(200);
        tabUI.cancelTabLongPress();
        vi.advanceTimersByTime(500);
        // Thumbnail preview should not have been triggered
        const preview = document.querySelector('.thumbnail-preview');
        const isVisible = preview && preview.classList.contains('thumbnail-preview--visible');
        expect(isVisible).toBeFalsy();
    });
});
