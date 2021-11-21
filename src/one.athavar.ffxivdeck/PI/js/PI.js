function PI(inContext, inLanguage, inStreamDeckVersion, inPluginVersion) {
    // Init PI
    const instance = this;

    // Public localizations for the UI
    this.localization = {};

    // Load the localizations
    getLocalization(inLanguage, function (inStatus, inLocalization) {
        if (inStatus) {
            // Save public localization
            instance.localization = inLocalization['PI'];

            // Localize the PI
            instance.localize();
        } else {
            log(inLocalization);
        }
    });

    // Localize the UI
    this.localize = function () {
        // Check if localizations were loaded
        if (instance.localization == null) {

        }
    };

    // message changed
    function iconChanged(inEvent) {
        // Save the new icon

        settings.icon = inEvent.target.value;
        instance.saveSettings("icon");
    }

    this.loadPi = function () {
        log("load called");


        // set value
        const input = document.getElementById('icon-input');
        input.value = settings.icon === undefined ? 0 : settings.icon;

        // Add event listener
        input.addEventListener('change', iconChanged);

        // Localize the PI
        instance.load();

        // Show PI
        document.getElementById('pi').style.display = 'block';
    }

    // Private function to return the action identifier
    function getAction() {
        let action;

        // Find out type of action
        if (instance instanceof ChatPI) {
            action = 'one.athavar.ffxivdeck.chat';
        } else if (instance instanceof GearSetPI) {
            action = 'one.athavar.ffxivdeck.gearset';
        }

        return action;
    }

    // Public function to save the settings
    this.saveSettings = function (element) {

        const inData = {'piEvent': 'valueChanged'};
        if (element !== undefined) {
            inData.value = element;
        }

        const action = getAction();
        saveSettings(action, inContext, settings);
        sendToPlugin(action, inContext, inData);
    }

    // Public function to send data to the plugin
    this.sendToPlugin = function (inData) {
        sendToPlugin(getAction(), inContext, inData);
    }
}