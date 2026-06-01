import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('evaluateSwipe pure function', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should return switch-tab next for valid left swipe (100px, ~3° angle)', () => {
        const result = tabUI.evaluateSwipe({ startX: 200, startY: 200, endX: 100, endY: 205, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'next' });
    });

    it('should return switch-tab previous for valid right swipe (100px)', () => {
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 200, endY: 205, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'previous' });
    });

    it('should return none for swipe below 40px threshold (30px)', () => {
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 130, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return none for swipe with > 30° angle deviation (50px horizontal, 40° angle)', () => {
        // tan(40°) ≈ 0.839, so dy ≈ 50 * 0.839 ≈ 42
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 150, endY: 242, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return none when startX is within left edge zone (startX=10, screenWidth=390)', () => {
        const result = tabUI.evaluateSwipe({ startX: 10, startY: 200, endX: 110, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return none when startX is within right edge zone (startX=380, screenWidth=390)', () => {
        const result = tabUI.evaluateSwipe({ startX: 380, startY: 200, endX: 280, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return valid switch for startX just outside edge zone (startX=25, screenWidth=390)', () => {
        const result = tabUI.evaluateSwipe({ startX: 25, startY: 200, endX: 125, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'previous' });
    });

    it('should return none for null/undefined/empty input (defensive)', () => {
        expect(tabUI.evaluateSwipe(null)).toEqual({ action: 'none' });
        expect(tabUI.evaluateSwipe(undefined)).toEqual({ action: 'none' });
        expect(tabUI.evaluateSwipe({})).toEqual({ action: 'none' });
    });

    it('should return switch-tab for exactly 40px threshold (boundary — implementation uses < not <=)', () => {
        // Implementation: Math.abs(dx) < SWIPE_THRESHOLD → exactly 40px passes
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 140, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'previous' });
    });

    it('should return none for 39px (just below threshold)', () => {
        const result = tabUI.evaluateSwipe({ startX: 100, startY: 200, endX: 139, endY: 200, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });

    it('should return switch-tab for angle just under 30° (29.7°)', () => {
        // dy=57, dx=100 → atan2(57,100) ≈ 29.68° — just under threshold, should pass
        const result = tabUI.evaluateSwipe({ startX: 200, startY: 200, endX: 100, endY: 257, screenWidth: 390 });
        expect(result).toEqual({ action: 'switch-tab', direction: 'next' });
    });

    it('should return none for angle just over 30° (30.1°)', () => {
        // dy=58, dx=100 → atan2(58,100) ≈ 30.11° — just over threshold, should fail
        const result = tabUI.evaluateSwipe({ startX: 200, startY: 200, endX: 100, endY: 258, screenWidth: 390 });
        expect(result).toEqual({ action: 'none' });
    });
});

describe('handleSwipeTabSwitch integration', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
        // Set up tabs: 3 tabs with second active
        tabUI.updateTabs([
            { id: 'tab1', title: 'Tab 1', url: 'https://one.com' },
            { id: 'tab2', title: 'Tab 2', url: 'https://two.com' },
            { id: 'tab3', title: 'Tab 3', url: 'https://three.com' }
        ]);
        tabUI.setActiveTab('tab2');
        // Set up mock bridge with switchTab (loadUI doesn't load bridge.js)
        window.bridge = { switchTab: vi.fn(), notifyThemeChange: vi.fn() };
    });

    afterEach(() => {
        cleanupUI();
        delete window.bridge;
    });

    it('should call bridge.switchTab with next tab ID when direction is next', () => {
        tabUI.handleSwipeTabSwitch('next');
        expect(window.bridge.switchTab).toHaveBeenCalledWith('tab3');
    });

    it('should call bridge.switchTab with previous tab ID when direction is previous', () => {
        tabUI.handleSwipeTabSwitch('previous');
        expect(window.bridge.switchTab).toHaveBeenCalledWith('tab1');
    });

    it('should NOT call bridge.switchTab when on last tab and direction is next', () => {
        tabUI.setActiveTab('tab3');
        tabUI.handleSwipeTabSwitch('next');
        expect(window.bridge.switchTab).not.toHaveBeenCalled();
    });

    it('should NOT call bridge.switchTab when on first tab and direction is previous', () => {
        tabUI.setActiveTab('tab1');
        tabUI.handleSwipeTabSwitch('previous');
        expect(window.bridge.switchTab).not.toHaveBeenCalled();
    });

    it('should NOT call bridge.switchTab when only one tab exists', () => {
        tabUI.updateTabs([{ id: 'solo', title: 'Solo Tab', url: 'https://solo.com' }]);
        tabUI.setActiveTab('solo');
        tabUI.handleSwipeTabSwitch('next');
        expect(window.bridge.switchTab).not.toHaveBeenCalled();
        tabUI.handleSwipeTabSwitch('previous');
        expect(window.bridge.switchTab).not.toHaveBeenCalled();
    });
});
