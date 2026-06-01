import { describe, it, expect } from 'vitest';
import { readFileSync } from 'fs';
import { resolve } from 'path';

const tokensCss = readFileSync(resolve(__dirname, '../styles/tokens.css'), 'utf-8');

/**
 * Extract the content of a CSS rule block by selector.
 * Returns the text between { and } for the given selector.
 */
function extractBlock(css, selector) {
    // Escape special regex chars in selector (dots, etc)
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(escaped + '\\s*\\{([^}]*)\\}', 's');
    const match = css.match(regex);
    return match ? match[1] : null;
}

/**
 * Extract a CSS variable value from a block string.
 */
function getVariable(block, varName) {
    const regex = new RegExp(varName.replace(/[-]/g, '\\-') + '\\s*:\\s*([^;]+);');
    const match = block.match(regex);
    return match ? match[1].trim() : null;
}

describe('tokens.css mobile-mode variables', () => {
    it('should have a .mobile-mode rule block', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        expect(block).not.toBeNull();
    });

    it('should set --bar-height to 56px in mobile-mode', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        expect(getVariable(block, '--bar-height')).toBe('56px');
    });

    it('should set --tabs-button-size to 48px in mobile-mode', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        expect(getVariable(block, '--tabs-button-size')).toBe('48px');
    });

    it('should set --nav-btn-size to 44px in mobile-mode', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        expect(getVariable(block, '--nav-btn-size')).toBe('44px');
    });

    it('should set --touch-target-min to 44px in mobile-mode', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        expect(getVariable(block, '--touch-target-min')).toBe('44px');
    });

    it('should have a .mobile-mode.tablet-mode rule block', () => {
        const block = extractBlock(tokensCss, '.mobile-mode.tablet-mode');
        expect(block).not.toBeNull();
    });

    it('should not override unrelated color variables in mobile-mode', () => {
        const block = extractBlock(tokensCss, '.mobile-mode');
        if (block) {
            expect(getVariable(block, '--color-bg')).toBeNull();
            expect(getVariable(block, '--color-text')).toBeNull();
        }
    });
});
