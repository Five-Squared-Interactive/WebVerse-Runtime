import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('setSafeArea', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should set CSS custom properties for all four insets', () => {
        tabUI.setSafeArea({ top: 44, bottom: 34, left: 0, right: 0 });
        const style = document.documentElement.style;
        expect(style.getPropertyValue('--safe-area-top')).toBe('44px');
        expect(style.getPropertyValue('--safe-area-bottom')).toBe('34px');
        expect(style.getPropertyValue('--safe-area-left')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-right')).toBe('0px');
    });

    it('should set all variables to 0px when given zero insets', () => {
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 0, right: 0 });
        const style = document.documentElement.style;
        expect(style.getPropertyValue('--safe-area-top')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-bottom')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-left')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-right')).toBe('0px');
    });

    it('should default missing values to 0', () => {
        tabUI.setSafeArea({ top: 44 });
        const style = document.documentElement.style;
        expect(style.getPropertyValue('--safe-area-top')).toBe('44px');
        expect(style.getPropertyValue('--safe-area-bottom')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-left')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-right')).toBe('0px');
    });

    it('should update values when called multiple times (not accumulate)', () => {
        tabUI.setSafeArea({ top: 44, bottom: 34, left: 0, right: 0 });
        tabUI.setSafeArea({ top: 20, bottom: 0, left: 10, right: 10 });
        const style = document.documentElement.style;
        expect(style.getPropertyValue('--safe-area-top')).toBe('20px');
        expect(style.getPropertyValue('--safe-area-bottom')).toBe('0px');
        expect(style.getPropertyValue('--safe-area-left')).toBe('10px');
        expect(style.getPropertyValue('--safe-area-right')).toBe('10px');
    });

    it('should be exposed on window.tabUI', () => {
        expect(typeof tabUI.setSafeArea).toBe('function');
    });
});
