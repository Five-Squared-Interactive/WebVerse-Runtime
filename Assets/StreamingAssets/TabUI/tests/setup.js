/**
 * Test setup helper — loads ui.js IIFE into jsdom's window.
 * Call loadUI() in beforeEach to get a fresh window.tabUI instance.
 */
import { readFileSync } from 'fs';
import { resolve } from 'path';

const uiJsPath = resolve(__dirname, '../scripts/ui.js');
const uiJsSource = readFileSync(uiJsPath, 'utf-8');

const indexHtmlPath = resolve(__dirname, '../index.html');
const indexHtmlSource = readFileSync(indexHtmlPath, 'utf-8');

// Extract body innerHTML from index.html (between <body> and </body>),
// excluding <script> tags (we load JS ourselves).
function extractBodyContent(html) {
    const bodyMatch = html.match(/<body[^>]*>([\s\S]*)<\/body>/i);
    if (!bodyMatch) return '';
    // Remove script tags — we execute JS manually
    return bodyMatch[1].replace(/<script[^>]*>[\s\S]*?<\/script>/gi, '');
}

const bodyContent = extractBodyContent(indexHtmlSource);

/**
 * Loads ui.js into the current jsdom window.
 * Sets up DOM from actual index.html so all element IDs are present.
 * Returns window.tabUI.
 */
export function loadUI() {
    // Set body content from actual index.html
    document.body.innerHTML = bodyContent;
    document.body.className = '';

    // Remove previous tabUI if exists
    delete window.tabUI;

    // Mock canvas getContext since jsdom doesn't support canvas
    HTMLCanvasElement.prototype.getContext = function() {
        return {
            clearRect: function() {},
            fillRect: function() {},
            beginPath: function() {},
            moveTo: function() {},
            lineTo: function() {},
            stroke: function() {},
            fill: function() {},
            arc: function() {},
            strokeStyle: '',
            fillStyle: '',
            lineWidth: 1,
            font: '',
            textAlign: '',
            textBaseline: '',
            globalAlpha: 1,
            save: function() {},
            restore: function() {},
            measureText: function() { return { width: 0 }; },
        };
    };

    // Execute the IIFE in current jsdom context
    const fn = new Function(uiJsSource);
    fn.call(window);

    return window.tabUI;
}

/**
 * Cleans up after test — removes body classes and tabUI reference.
 */
export function cleanupUI() {
    document.body.className = '';
    document.body.innerHTML = '';
    delete window.tabUI;
}
