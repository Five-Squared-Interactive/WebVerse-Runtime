import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { loadUI, cleanupUI } from './setup.js';

describe('setMode', () => {
    let tabUI;

    beforeEach(() => {
        tabUI = loadUI();
    });

    afterEach(() => {
        cleanupUI();
    });

    it('should add mobile-mode class when setMode("mobile") is called', () => {
        tabUI.setMode('mobile');
        expect(document.body.classList.contains('mobile-mode')).toBe(true);
    });

    it('should remove vr-mode class when switching to mobile', () => {
        tabUI.setMode('vr');
        expect(document.body.classList.contains('vr-mode')).toBe(true);
        tabUI.setMode('mobile');
        expect(document.body.classList.contains('vr-mode')).toBe(false);
        expect(document.body.classList.contains('mobile-mode')).toBe(true);
    });

    it('should add both mobile-mode and tablet-mode when setMode("tablet") is called', () => {
        tabUI.setMode('tablet');
        expect(document.body.classList.contains('mobile-mode')).toBe(true);
        expect(document.body.classList.contains('tablet-mode')).toBe(true);
    });

    it('should remove mobile-mode and tablet-mode when setMode("desktop") is called', () => {
        tabUI.setMode('tablet');
        tabUI.setMode('desktop');
        expect(document.body.classList.contains('mobile-mode')).toBe(false);
        expect(document.body.classList.contains('tablet-mode')).toBe(false);
    });

    it('should remove mobile-mode and tablet-mode when switching to vr, and add vr-mode', () => {
        tabUI.setMode('tablet');
        tabUI.setMode('vr');
        expect(document.body.classList.contains('mobile-mode')).toBe(false);
        expect(document.body.classList.contains('tablet-mode')).toBe(false);
        expect(document.body.classList.contains('vr-mode')).toBe(true);
    });

    it('should remove vr-mode when switching from vr to desktop', () => {
        tabUI.setMode('vr');
        tabUI.setMode('desktop');
        expect(document.body.classList.contains('vr-mode')).toBe(false);
    });

    it('should handle rapid mode switching and only retain final mode classes', () => {
        tabUI.setMode('mobile');
        tabUI.setMode('desktop');
        tabUI.setMode('tablet');
        tabUI.setMode('vr');
        expect(document.body.classList.contains('vr-mode')).toBe(true);
        expect(document.body.classList.contains('mobile-mode')).toBe(false);
        expect(document.body.classList.contains('tablet-mode')).toBe(false);
    });

    it('should apply correct classes through full mode cycle', () => {
        tabUI.setMode('mobile');
        expect(document.body.classList.contains('mobile-mode')).toBe(true);
        tabUI.setMode('tablet');
        expect(document.body.classList.contains('mobile-mode')).toBe(true);
        expect(document.body.classList.contains('tablet-mode')).toBe(true);
        tabUI.setMode('desktop');
        expect(document.body.classList.contains('mobile-mode')).toBe(false);
        expect(document.body.classList.contains('tablet-mode')).toBe(false);
        expect(document.body.classList.contains('vr-mode')).toBe(false);
    });
});
