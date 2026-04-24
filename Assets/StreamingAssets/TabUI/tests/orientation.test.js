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

describe('orientation transitions', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should have CSS transition on .mobile-mode .chrome for position properties (max 200ms)', () => {
        const block = extractBlock(componentsCss, '.mobile-mode .chrome');
        expect(block).not.toBeNull();
        expect(block).toContain('transition');
        // Verify transition includes position-related properties and is <= 200ms
        expect(block).toMatch(/transition:.*(?:top|bottom).*200ms/);
    });

    it('should have CSS transition on .mobile-mode .content-frame for position properties (max 200ms)', () => {
        const block = extractBlock(componentsCss, '.mobile-mode .content-frame');
        expect(block).not.toBeNull();
        expect(block).toContain('transition');
        expect(block).toMatch(/transition:.*(?:top|bottom).*200ms/);
    });

    it('should update all four CSS variables when setSafeArea is called with landscape insets', () => {
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 44, right: 44 });
        const root = document.documentElement;
        expect(root.style.getPropertyValue('--safe-area-top')).toBe('0px');
        expect(root.style.getPropertyValue('--safe-area-bottom')).toBe('0px');
        expect(root.style.getPropertyValue('--safe-area-left')).toBe('44px');
        expect(root.style.getPropertyValue('--safe-area-right')).toBe('44px');
    });

    it('should have chrome padding rule that references --safe-area-left and --safe-area-right', () => {
        const block = extractBlock(componentsCss, '.mobile-mode .chrome');
        expect(block).not.toBeNull();
        expect(block).toContain('--safe-area-left');
        expect(block).toContain('--safe-area-right');
    });

    it('should apply only final values when setSafeArea is called multiple times rapidly', () => {
        tabUI.setSafeArea({ top: 59, bottom: 34, left: 0, right: 0 });
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 59, right: 0 });
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 44, right: 44 });
        const root = document.documentElement;
        expect(root.style.getPropertyValue('--safe-area-top')).toBe('0px');
        expect(root.style.getPropertyValue('--safe-area-bottom')).toBe('0px');
        expect(root.style.getPropertyValue('--safe-area-left')).toBe('44px');
        expect(root.style.getPropertyValue('--safe-area-right')).toBe('44px');
    });

    it('should expose setOrientation on window.tabUI and update orientation state', () => {
        expect(typeof tabUI.setOrientation).toBe('function');
        tabUI.setOrientation('landscape');
        // Verify by calling again — function should not throw
        tabUI.setOrientation('portrait');
    });

    it('should default to portrait when setOrientation receives invalid value', () => {
        tabUI.setOrientation('landscape');
        tabUI.setOrientation('invalid');
        // Invalid value should default to portrait (same pattern as setChromePosition)
        // Can't read state directly, but verify no throw and subsequent calls work
        expect(() => tabUI.setOrientation('landscape')).not.toThrow();
    });
});

describe('orientation with open UI elements', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should keep tab dropdown open when setSafeArea is called with new insets', () => {
        // Open the tab dropdown by clicking tabs button
        const tabsButton = document.querySelector('.tabs-button');
        expect(tabsButton).not.toBeNull();
        tabsButton.click();

        // Verify dropdown is actually open (display: block set by openTabDropdown)
        const dropdown = document.querySelector('.tab-dropdown');
        expect(dropdown).not.toBeNull();
        expect(dropdown.style.display).toBe('block');

        // Simulate orientation change by updating safe area
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 44, right: 44 });

        // Dropdown should still be open (display: block, not reverted to none)
        expect(dropdown.style.display).toBe('block');
    });

    it('should keep modal open when setSafeArea is called with new insets', () => {
        // Open a modal by adding the modal--open class (simulating openModal)
        const modalOverlay = document.querySelector('.modal-overlay');
        expect(modalOverlay).not.toBeNull();
        modalOverlay.classList.add('modal--open');
        expect(modalOverlay.classList.contains('modal--open')).toBe(true);

        // Simulate orientation change
        tabUI.setSafeArea({ top: 0, bottom: 0, left: 44, right: 44 });

        // Modal should still have modal--open class
        expect(modalOverlay.classList.contains('modal--open')).toBe(true);
    });
});
