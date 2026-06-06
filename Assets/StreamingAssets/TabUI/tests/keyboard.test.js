import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { loadUI, cleanupUI } from './setup.js';

const componentsCss = readFileSync(resolve(__dirname, '../styles/components.css'), 'utf-8');
const tokensCss = readFileSync(resolve(__dirname, '../styles/tokens.css'), 'utf-8');

/**
 * Extract the content of a CSS rule block by selector.
 */
function extractBlock(css, selector) {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(escaped + '\\s*\\{([^}]*)\\}', 's');
    const match = css.match(regex);
    return match ? match[1] : null;
}

describe('keyboard state management', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should set --keyboard-height CSS variable when setKeyboardState is called with visible=true', () => {
        tabUI.setKeyboardState({ visible: true, height: 300 });
        const root = document.documentElement;
        expect(root.style.getPropertyValue('--keyboard-height')).toBe('300px');
    });

    it('should add keyboard-open class to body when keyboard is visible', () => {
        tabUI.setKeyboardState({ visible: true, height: 300 });
        expect(document.body.classList.contains('keyboard-open')).toBe(true);
    });

    it('should remove keyboard-open class and reset --keyboard-height when keyboard is hidden', () => {
        tabUI.setKeyboardState({ visible: true, height: 300 });
        tabUI.setKeyboardState({ visible: false, height: 0 });
        expect(document.body.classList.contains('keyboard-open')).toBe(false);
        expect(document.documentElement.style.getPropertyValue('--keyboard-height')).toBe('0px');
    });

    it('should update --keyboard-height when keyboard resizes without removing keyboard-open', () => {
        tabUI.setKeyboardState({ visible: true, height: 300 });
        tabUI.setKeyboardState({ visible: true, height: 350 });
        expect(document.body.classList.contains('keyboard-open')).toBe(true);
        expect(document.documentElement.style.getPropertyValue('--keyboard-height')).toBe('350px');
    });

    it('should update state.keyboardVisible and state.keyboardHeight via CSS variable side effects', () => {
        expect(typeof tabUI.setKeyboardState).toBe('function');
        // Verify state update through CSS variable (proves state.keyboardHeight was set)
        tabUI.setKeyboardState({ visible: true, height: 250 });
        expect(document.documentElement.style.getPropertyValue('--keyboard-height')).toBe('250px');
        expect(document.body.classList.contains('keyboard-open')).toBe(true);
        // Verify hidden state resets both
        tabUI.setKeyboardState({ visible: false, height: 0 });
        expect(document.documentElement.style.getPropertyValue('--keyboard-height')).toBe('0px');
        expect(document.body.classList.contains('keyboard-open')).toBe(false);
    });

    it('should handle null/undefined argument defensively', () => {
        expect(() => tabUI.setKeyboardState(null)).not.toThrow();
        expect(() => tabUI.setKeyboardState(undefined)).not.toThrow();
        expect(() => tabUI.setKeyboardState({})).not.toThrow();
        // After defensive calls, keyboard should be hidden
        expect(document.body.classList.contains('keyboard-open')).toBe(false);
        expect(document.documentElement.style.getPropertyValue('--keyboard-height')).toBe('0px');
    });
});

describe('keyboard CSS rules', () => {
    it('should have CSS rule for .mobile-mode.chrome-bottom.keyboard-open .chrome with bottom referencing --keyboard-height', () => {
        const block = extractBlock(componentsCss, '.mobile-mode.chrome-bottom.keyboard-open .chrome');
        expect(block).not.toBeNull();
        expect(block).toContain('--keyboard-height');
    });

    it('should NOT reposition chrome-top when keyboard is open', () => {
        // chrome-top should NOT have a keyboard-open override that changes top
        const block = extractBlock(componentsCss, '.mobile-mode.chrome-top.keyboard-open .chrome');
        // Either the rule doesn't exist, or if it does, it shouldn't change top
        if (block) {
            expect(block).not.toContain('top:');
        }
    });

    it('should have --keyboard-height default in tokens.css', () => {
        expect(tokensCss).toContain('--keyboard-height');
    });

    it('should reposition toast container above keyboard when bottom chrome and keyboard open', () => {
        const block = extractBlock(componentsCss, '.mobile-mode.chrome-bottom.keyboard-open .toast-container');
        expect(block).not.toBeNull();
        expect(block).toContain('--keyboard-height');
    });
});

describe('keyboard and content frame', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should NOT have content-frame resize rule when keyboard is open (MNFR7)', () => {
        // Content frame must NOT resize when keyboard opens
        const block = extractBlock(componentsCss, '.mobile-mode.keyboard-open .content-frame');
        // Rule should either not exist or not change height/top/bottom
        if (block) {
            expect(block).not.toMatch(/\b(height|top|bottom)\s*:/);
        }
    });
});

describe('keyboard bridge integration', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
        tabUI.setMode('mobile');
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should handle Enter key in URL bar by blurring (dismissing keyboard)', () => {
        // Mock bridge.navigate since bridge.js is not loaded in test environment
        window.bridge = { navigate: function() {} };

        const urlBar = document.getElementById('url-bar');
        expect(urlBar).not.toBeNull();
        urlBar.value = 'https://example.com';
        urlBar.focus();

        const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true });
        urlBar.dispatchEvent(event);

        // After Enter, URL bar should have been blurred (blur dismisses keyboard on mobile)
        expect(document.activeElement).not.toBe(urlBar);
    });
});
