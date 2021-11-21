// Register the plugin or PI
function registerPluginOrPI(inEvent, inUUID) {
    if (websocket) {
        const json = {
            'event': inEvent,
            'uuid': inUUID
        };

        websocket.send(JSON.stringify(json));
    }
}

// Save settings
function saveSettings(inAction, inUUID, inSettings) {
    if (websocket) {
        const json = {
            'action': inAction,
            'event': 'setSettings',
            'context': inUUID,
            'payload': inSettings
        };

        websocket.send(JSON.stringify(json));
    }
}

function requestSettings(inUUID) {
    if (websocket) {
        const json = {
            'event': 'getSettings',
            'context': inUUID
        };

        websocket.send(JSON.stringify(json));
    }
}

// Save global settings
function saveGlobalSettings(inUUID) {
    if (websocket) {
        const json = {
            'event': 'setGlobalSettings',
            'context': inUUID,
            'payload': globalSettings
        };

        websocket.send(JSON.stringify(json));
    }
}

// Request global settings for the plugin
function requestGlobalSettings(inUUID) {
    if (websocket) {
        const json = {
            'event': 'getGlobalSettings',
            'context': inUUID
        };

        websocket.send(JSON.stringify(json));
    }
}

// Log to the global log file
function log(inMessage) {
    // Log to the developer console
    const time = new Date();
    const timeString = time.toLocaleDateString() + ' ' + time.toLocaleTimeString();
    console.log(timeString, inMessage);

    // Log to the Stream Deck log file
    if (websocket) {
        const json = {
            'event': 'logMessage',
            'payload': {
                'message': inMessage
            }
        };

        websocket.send(JSON.stringify(json));
    }
}

// Set data to plugin
function sendToPlugin(inAction, inContext, inData) {
    if (websocket) {
        const json = {
            'action': inAction,
            'event': 'sendToPlugin',
            'context': inContext,
            'payload': inData
        };

        websocket.send(JSON.stringify(json));
    }
}

// Load the localizations
function getLocalization(inLanguage, inCallback) {
    const url = '../' + inLanguage + '.json';
    const xhr = new XMLHttpRequest();
    xhr.open('GET', url, true);

    xhr.onload = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            try {
                data = JSON.parse(xhr.responseText);
                const localization = data['Localization'];
                inCallback(true, localization);
            } catch (e) {
                inCallback(false, 'Localizations is not a valid json.');
            }
        } else {
            inCallback(false, 'Could not load the localizations.');
        }
    };

    xhr.onerror = function () {
        inCallback(false, 'An error occurred while loading the localizations.');
    };

    xhr.ontimeout = function () {
        inCallback(false, 'Localization timed out.');
    };

    xhr.send();
}