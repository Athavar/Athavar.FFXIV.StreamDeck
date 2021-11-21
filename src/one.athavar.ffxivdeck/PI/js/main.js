// Global web socket
let websocket = null;

// Global plugin settings
let globalSettings = {};

// Global settings
let settings = {};

// Global cache
let cache = {};

// Setup the websocket and handle communication
function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    // Parse parameter from string to object
    const actionInfo = JSON.parse(inActionInfo);
    const info = JSON.parse(inInfo);

    const streamDeckVersion = info['application']['version'];
    const pluginVersion = info['plugin']['version'];

    // Save global settings
    settings = actionInfo['payload']['settings'];

    // Retrieve language
    const language = info['application']['language'];

    // Retrieve action identifier
    const action = actionInfo['action'];

    // Open the web socket to Stream Deck
    // Use 127.0.0.1 because Windows needs 300ms to resolve localhost
    websocket = new WebSocket('ws://127.0.0.1:' + inPort);

    // WebSocket is connected, send message
    websocket.onopen = function () {
        // Register property inspector to Stream Deck
        registerPluginOrPI(inRegisterEvent, inUUID);

        // Request the global settings of the plugin
        requestGlobalSettings(inUUID);

        // Request the settings of the plugin
        requestSettings(inUUID);
    };

    // Create actions
    let pi;

    if (action === 'one.athavar.ffxivdeck.chat') {
        pi = new ChatPI(inUUID, language, streamDeckVersion, pluginVersion);
    } else if (action === 'one.athavar.ffxivdeck.gearset') {
        pi = new GearSetPI(inUUID, language, streamDeckVersion, pluginVersion);
    }

    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        const jsonObj = JSON.parse(evt.data);
        const event = jsonObj['event'];
        const jsonPayload = jsonObj['payload'];

        log(jsonObj)
        if (event === 'didReceiveGlobalSettings') {
            // Set global plugin settings
            globalSettings = jsonPayload['settings'];
        } else if (event === 'didReceiveSettings') {
            // Save global settings after default was set
            settings = jsonPayload['settings'];
        } else if (event === 'sendToPropertyInspector') {
            // Save global cache
            cache = jsonPayload;

            // load pi
            pi.loadPi();
        }
    };
}