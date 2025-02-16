// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

/**
 * History.
 */
let history = {};

/**
 * Daemon Process ID.
 */
let dPID = null;

/**
 * Daemon Port.
 */
let dPort = null;

/**
 * Daemon Certificate.
 */
let dCert = null;

/**
 * Daemon ID.
 */
let daemonID = null;

/**
 * Next Tab ID.
 */
let nextTabID = null;

/**
 * The World Load Timeout.
 */
let worldLoadTimeout = null;

/**
 * Lightweight Runtime Path.
 */
let lightweightRuntimePath = null;

/**
 * @function PopulateHistoryTable Populate the history table.
 * @param {*} hist History entries.
 */
function PopulateHistoryTable(hist) {
    var table = document.getElementById("history");

    hist.reverse().forEach(entry => {
        var row = table.insertRow();
        var cell1 = row.insertCell(0);
        var cell2 = row.insertCell(1);
        cell1.innerHTML = new Date(entry.timestamp).toLocaleString();
        cell2.innerHTML = "<a href='#' onclick=\"LoadURL('" + entry.site + "');\">" + entry.site + "</a>";
    });
}

/**
 * Get the Query Parameters from the URL.
 */
function GetQueryParams() {
    params = new URLSearchParams(window.location.search)
    history = params.get("history");
    dPID = params.get("daemon_pid");
    dPort = params.get("daemon_port");
    dCert = params.get("daemon_cert");
    daemonID = params.get("main_app_id");
    nextTabID = params.get("tab_id");
    worldLoadTimeout = params.get("world_load_timeout");
    lightweightRuntimePath = params.get("lw_runtime_path");
}

/**
 * @function LoadURL Load a URL via message to application.
 * @param {*} url URL to load.
 */
function LoadURL(url) {
    window.vuplex.postMessage("WEBVERSE.INTERNAL.LOADURL." + url);
}

GetQueryParams();
PopulateHistoryTable(JSON.parse(history));