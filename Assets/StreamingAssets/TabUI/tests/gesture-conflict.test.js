import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

// Phase 1: Scroll vs Swipe-to-Close Conflict Prevention (AC: #4)
describe('evaluateTabSwipeDismiss with vertical component', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should return none when vertical motion dominates (scroll gesture)', () => {
        // 85px horizontal, 100px vertical — angle ~49.6° (scroll dominates)
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 185, startY: 100, endY: 200 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return dismiss when horizontal motion dominates', () => {
        // 85px horizontal, 10px vertical — angle ~6.7° (swipe dominates)
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 185, startY: 100, endY: 110 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return none when angle exceeds 30 degrees', () => {
        // 80px horizontal, 50px vertical — angle ~32° (diagonal, rejected)
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 180, startY: 100, endY: 150 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return dismiss when angle is under 30 degrees and dx >= threshold', () => {
        // 100px horizontal, 50px vertical — angle ~26.6° (just under 30°, valid)
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 200, startY: 100, endY: 150 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return none at exactly 30 degree boundary', () => {
        // tan(30°) = 0.577, so for dx=80 → dy=46.2 → round to 47 for > 30°
        // atan2(47, 80) ≈ 30.4° → should reject
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 180, startY: 100, endY: 147 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should still work without Y coordinates (backward compatible)', () => {
        // No startY/endY — existing behavior preserved
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 180 });
        expect(result).toEqual({ action: 'dismiss' });
    });

    it('should return none for pure vertical swipe (0 horizontal, 100 vertical)', () => {
        const result = tabUI.evaluateTabSwipeDismiss({ startX: 100, endX: 100, startY: 100, endY: 200 });
        expect(result).toEqual({ action: 'none' });
    });
});

// Phase 2: Edge Zone and Existing Gesture Validation Regression Guards (AC: #1, #2, #3, #5)
describe('gesture conflict regression guards', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    // AC1: iOS edge zone exclusion for evaluateSwipe
    it('evaluateSwipe: should suppress swipe starting inside left edge zone (x=19)', () => {
        const result = tabUI.evaluateSwipe({ startX: 19, startY: 200, endX: 119, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('evaluateSwipe: should suppress swipe starting at edge boundary (x=20, edge zone is < 20)', () => {
        // EDGE_ZONE = 20, check is startX < EDGE_ZONE, so x=20 is allowed
        const result = tabUI.evaluateSwipe({ startX: 20, startY: 200, endX: 120, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'previous' });
    });

    it('evaluateSwipe: should allow swipe starting just outside edge zone (x=21)', () => {
        const result = tabUI.evaluateSwipe({ startX: 21, startY: 200, endX: 121, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'previous' });
    });

    // AC1: iOS right edge zone exclusion
    it('evaluateSwipe: should suppress swipe starting inside right edge zone (x=371, screenWidth=390)', () => {
        const result = tabUI.evaluateSwipe({ startX: 371, startY: 200, endX: 271, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('evaluateSwipe: should allow swipe starting at right edge boundary (x=370, screenWidth=390)', () => {
        // screenWidth - EDGE_ZONE = 370, check is startX > 370, so x=370 is allowed
        const result = tabUI.evaluateSwipe({ startX: 370, startY: 200, endX: 270, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'next' });
    });

    // AC2: Threshold regression
    it('evaluateSwipe: should return none for 39px swipe (below 40px threshold)', () => {
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 139, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    // AC3: Angle regression
    it('evaluateSwipe: should return none for angle > 30 degrees', () => {
        // dx=50, dy=42 → angle ~40° → rejected
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 150, endY: 242, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    // AC5: Center-screen tap doesn't reactivate chrome
    it('isEdgeTap: center-screen tap should not activate chrome (bottom position)', () => {
        expect(tabUI.isEdgeTap(400, 800, 'bottom')).toBe(false);
    });

    it('isEdgeTap: near-bottom-edge tap should activate chrome', () => {
        expect(tabUI.isEdgeTap(790, 800, 'bottom')).toBe(true);
    });

    it('isEdgeTap: near-top-edge tap should activate chrome (top position)', () => {
        expect(tabUI.isEdgeTap(5, 800, 'top')).toBe(true);
    });

    it('isEdgeTap: center-screen tap should not activate chrome (top position)', () => {
        expect(tabUI.isEdgeTap(400, 800, 'top')).toBe(false);
    });
});

// Phase 3: createTabElement Touch Event Wiring (AC: #4)
describe('tab item touch event wiring', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        tabUI.updateTabs([
            { id: 'tab1', title: 'Tab 1', url: 'https://one.com' },
            { id: 'tab2', title: 'Tab 2', url: 'https://two.com' }
        ]);
        tabUI.setActiveTab('tab1');
        window.bridge = {
            closeTab: vi.fn(),
            switchTab: vi.fn(),
            newTab: vi.fn(),
            requestThumbnail: vi.fn(),
            notifyThemeChange: vi.fn(),
            notifyOverlayOpened: vi.fn(),
            notifyOverlayClosed: vi.fn()
        };
    });

    afterEach(() => {
        cleanupUI();
        delete window.bridge;
    });

    function getTabItem(tabId) {
        // Tab items are rendered in the dropdown; open it first
        const tabsButton = document.querySelector('.tabs-button');
        if (tabsButton) tabsButton.click();
        const items = document.querySelectorAll('.tab-item');
        for (const item of items) {
            if (item.getAttribute('aria-label') === tabId || item.textContent.includes(tabId)) {
                return item;
            }
        }
        // Fallback: return first non-active item
        return items.length > 1 ? items[1] : items[0];
    }

    function createTouchEvent(type, clientX, clientY) {
        return new TouchEvent(type, {
            bubbles: true,
            cancelable: true,
            touches: type === 'touchend' ? [] : [{ clientX, clientY, identifier: 0 }],
            changedTouches: [{ clientX, clientY, identifier: 0 }]
        });
    }

    it('should add tab-item--swiping class on touchstart', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        expect(item.classList.contains('tab-item--swiping')).toBe(true);
    });

    it('should apply translateX on horizontal touchmove exceeding 10px', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        item.dispatchEvent(createTouchEvent('touchmove', 115, 202));
        // 15px horizontal, 2px vertical → angle ~7.6° → horizontal swipe mode
        expect(item.style.transform).toContain('translateX');
    });

    it('should NOT apply translateX on vertical touchmove (scroll priority)', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        item.dispatchEvent(createTouchEvent('touchmove', 103, 250));
        // 3px horizontal, 50px vertical → angle ~86.6° → vertical scroll, no swipe
        expect(item.style.transform).not.toContain('translateX');
    });

    it('should call bridge.closeTab on touchend with sufficient horizontal swipe', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        // Move enough to trigger swipe mode
        item.dispatchEvent(createTouchEvent('touchmove', 185, 205));
        // End with 85px horizontal, 5px vertical → dismiss
        item.dispatchEvent(createTouchEvent('touchend', 185, 205));
        expect(window.bridge.closeTab).toHaveBeenCalled();
    });

    it('should snap back on touchend with insufficient horizontal swipe', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        item.dispatchEvent(createTouchEvent('touchmove', 130, 203));
        item.dispatchEvent(createTouchEvent('touchend', 130, 203));
        // 30px horizontal — below 80px threshold → snap back
        expect(window.bridge.closeTab).not.toHaveBeenCalled();
        expect(item.classList.contains('tab-item--snapping')).toBe(true);
    });

    it('should reset on touchcancel', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        item.dispatchEvent(createTouchEvent('touchmove', 150, 203));
        item.dispatchEvent(createTouchEvent('touchcancel', 150, 203));
        expect(item.style.transform).toBe('');
        expect(item.style.opacity).toBe('');
        expect(item.classList.contains('tab-item--snapping')).toBe(true);
    });

    it('should NOT switch from scroll to swipe when finger curves horizontally (gesture lock)', () => {
        const item = getTabItem('tab2');
        expect(item).toBeTruthy();
        // Start touch
        item.dispatchEvent(createTouchEvent('touchstart', 100, 200));
        // First move: vertical dominant (50px vertical, 3px horizontal → ~86° → scroll locked)
        item.dispatchEvent(createTouchEvent('touchmove', 103, 250));
        expect(item.style.transform).not.toContain('translateX');
        // Second move: cumulative now horizontal-ish (80px horizontal, 50px vertical → ~32°)
        // But gesture was already locked to scroll — should NOT start swiping
        item.dispatchEvent(createTouchEvent('touchmove', 180, 250));
        expect(item.style.transform).not.toContain('translateX');
    });
});
