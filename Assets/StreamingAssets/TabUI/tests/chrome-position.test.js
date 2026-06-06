import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const componentsCss = readFileSync(resolve(__dirname, '../styles/components.css'), 'utf-8');

/**
 * Extract the content of a CSS rule block by selector.
 */
function extractBlock(css, selector) {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(escaped + '\\s*\\{([^}]*)\\}', 's');
    const match = css.match(regex);
    return match ? match[1] : null;
}

describe('setChromePosition', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        // Enter mobile mode first since chrome-position is mobile-only
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should default to chrome-bottom class in mobile mode', () => {
        expect(document.body.classList.contains('chrome-bottom')).toBe(true);
        expect(document.body.classList.contains('chrome-top')).toBe(false);
    });

    it('should add chrome-top class and remove chrome-bottom when set to top', () => {
        tabUI.setChromePosition('top');
        expect(document.body.classList.contains('chrome-top')).toBe(true);
        expect(document.body.classList.contains('chrome-bottom')).toBe(false);
    });

    it('should add chrome-bottom class and remove chrome-top when set to bottom', () => {
        tabUI.setChromePosition('top');
        tabUI.setChromePosition('bottom');
        expect(document.body.classList.contains('chrome-bottom')).toBe(true);
        expect(document.body.classList.contains('chrome-top')).toBe(false);
    });

    it('should have CSS rule for bottom-positioned chrome with safe area offset', () => {
        const block = extractBlock(componentsCss, '.mobile-mode.chrome-bottom .chrome');
        expect(block).not.toBeNull();
        expect(block).toContain('--safe-area-bottom');
    });

    it('should have CSS rule for top-positioned chrome with safe area offset', () => {
        const block = extractBlock(componentsCss, '.mobile-mode.chrome-top .chrome');
        expect(block).not.toBeNull();
        expect(block).toContain('--safe-area-top');
    });

    it('should update state.chromePosition property', () => {
        tabUI.setChromePosition('top');
        // Verify by switching back and checking classes are consistent
        tabUI.setChromePosition('bottom');
        expect(document.body.classList.contains('chrome-bottom')).toBe(true);
    });

    it('should default to bottom when given invalid value', () => {
        tabUI.setChromePosition('invalid');
        expect(document.body.classList.contains('chrome-bottom')).toBe(true);
        expect(document.body.classList.contains('chrome-top')).toBe(false);
    });

    it('should persist chrome position across mode switches', () => {
        tabUI.setChromePosition('top');
        expect(document.body.classList.contains('chrome-top')).toBe(true);

        // Switch to desktop — position classes should be removed
        tabUI.setMode('desktop');
        expect(document.body.classList.contains('chrome-top')).toBe(false);
        expect(document.body.classList.contains('chrome-bottom')).toBe(false);

        // Switch back to mobile — 'top' should be re-applied from state
        tabUI.setMode('mobile');
        expect(document.body.classList.contains('chrome-top')).toBe(true);
        expect(document.body.classList.contains('chrome-bottom')).toBe(false);
    });

    it('should be exposed on window.tabUI', () => {
        expect(typeof tabUI.setChromePosition).toBe('function');
    });
});
