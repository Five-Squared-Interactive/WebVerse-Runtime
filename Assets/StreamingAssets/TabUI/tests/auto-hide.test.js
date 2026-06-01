import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('auto-hide timer', () => {
    let tabUI;

    beforeEach(() => {
        vi.useFakeTimers();
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
        vi.useRealTimers();
    });

    it('should hide chrome after 3000ms when startAutoHideTimer is called', () => {
        tabUI.showChrome();
        const chrome = document.querySelector('.chrome');
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
        tabUI.startAutoHideTimer();
        vi.advanceTimersByTime(2999);
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
        vi.advanceTimersByTime(1);
        expect(chrome.classList.contains('chrome--hidden')).toBe(true);
    });

    it('should reset timer when resetAutoHideTimer is called', () => {
        tabUI.showChrome();
        const chrome = document.querySelector('.chrome');
        tabUI.startAutoHideTimer();
        vi.advanceTimersByTime(2000);
        tabUI.resetAutoHideTimer();
        vi.advanceTimersByTime(2000);
        // Should still be visible (timer was reset, only 2s into new 3s timer)
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
        vi.advanceTimersByTime(1000);
        expect(chrome.classList.contains('chrome--hidden')).toBe(true);
    });

    it('should cancel timer without restarting when stopAutoHideTimer is called', () => {
        tabUI.showChrome();
        const chrome = document.querySelector('.chrome');
        tabUI.startAutoHideTimer();
        vi.advanceTimersByTime(1000);
        tabUI.stopAutoHideTimer();
        vi.advanceTimersByTime(5000);
        // Chrome should still be visible — timer was stopped
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
    });

    it('should NOT start timer when keyboard is open', () => {
        tabUI.showChrome();
        const chrome = document.querySelector('.chrome');
        tabUI.setKeyboardState({ visible: true, height: 300 });
        tabUI.startAutoHideTimer();
        vi.advanceTimersByTime(5000);
        // Chrome should still be visible — keyboard suppresses auto-hide
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
    });
});

describe('isEdgeTap pure function', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should return true for tapY < 20 when chromePosition is top', () => {
        expect(tabUI.isEdgeTap(10, 800, 'top')).toBe(true);
    });

    it('should return true for tapY > (screenHeight - 20) when chromePosition is bottom', () => {
        expect(tabUI.isEdgeTap(790, 800, 'bottom')).toBe(true);
    });

    it('should return false for center-screen tap', () => {
        expect(tabUI.isEdgeTap(400, 800, 'bottom')).toBe(false);
        expect(tabUI.isEdgeTap(400, 800, 'top')).toBe(false);
    });

    it('should return false for tapY < 20 when chromePosition is bottom', () => {
        expect(tabUI.isEdgeTap(10, 800, 'bottom')).toBe(false);
    });

    it('should return false for tapY > (screenHeight - 20) when chromePosition is top', () => {
        expect(tabUI.isEdgeTap(790, 800, 'top')).toBe(false);
    });
});

describe('handleEdgeTap', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should show chrome when hidden and edge tap detected at matching position', () => {
        tabUI.setChromePosition('bottom');
        tabUI.hideChrome();
        tabUI.handleEdgeTap(790, 800);
        // Chrome should be visible again
        const chrome = document.querySelector('.chrome');
        expect(chrome.classList.contains('chrome--visible')).toBe(true);
    });

    it('should NOT show chrome when tap is not at edge', () => {
        tabUI.setChromePosition('bottom');
        tabUI.hideChrome();
        tabUI.handleEdgeTap(400, 800);
        const chrome = document.querySelector('.chrome');
        expect(chrome.classList.contains('chrome--hidden')).toBe(true);
    });
});
