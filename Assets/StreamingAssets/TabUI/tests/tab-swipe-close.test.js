import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('evaluateTabSwipeDismiss pure function', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should return dismiss when dx >= threshold (right swipe)', () => {
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 180, threshold: 80 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return none when dx < threshold', () => {
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 170, threshold: 80 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return dismiss for left swipe past threshold', () => {
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 200, endX: 120, threshold: 80 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return dismiss for right swipe past threshold', () => {
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 200, threshold: 80 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return none for null/undefined input (defensive)', () => {
        expect(tabUI.evaluateTabSwipeDismiss(null)).toEqual({ action: 'none' });
        expect(tabUI.evaluateTabSwipeDismiss(undefined)).toEqual({ action: 'none' });
        expect(tabUI.evaluateTabSwipeDismiss({})).toEqual({ action: 'none' });
    });

    it('should return none when startX or endX is missing', () => {
        expect(tabUI.evaluateTabSwipeDismiss({ startX: 100 })).toEqual({ action: 'none' });
        expect(tabUI.evaluateTabSwipeDismiss({ endX: 180 })).toEqual({ action: 'none' });
    });

    it('should use default threshold of 80px when not specified', () => {
        // 79px — below default threshold
        const result1 = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 179 });
        expect(result1).toEqual({ action: 'none' });
        // 80px — meets default threshold
        const result2 = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 180 });
        expect(result2).toEqual({ action: 'dismiss' });
    });
});

describe('swipe-to-close integration', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        tabUI.updateTabs([
            { id: 'tab1', title: 'Tab 1', url: 'https://one.com' },
            { id: 'tab2', title: 'Tab 2', url: 'https://two.com' },
            { id: 'tab3', title: 'Tab 3', url: 'https://three.com' }
        ]);
        tabUI.setActiveTab('tab2');
        window.bridge = {
            closeTab: vi.fn(),
            switchTab: vi.fn(),
            newTab: vi.fn(),
            notifyThemeChange: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
    });

    afterEach(() => {
        cleanupUI();
        delete window.bridge;
    });

    it('should call bridge.closeTab when swipe dismiss triggered', () => {
        tabUI.handleTabSwipeDismiss('tab1');
        expect(window.bridge.closeTab).toHaveBeenCalledWith('tab1');
    });

    it('should call bridge.closeTab on active tab swipe', () => {
        tabUI.handleTabSwipeDismiss('tab2');
        expect(window.bridge.closeTab).toHaveBeenCalledWith('tab2');
    });
});
