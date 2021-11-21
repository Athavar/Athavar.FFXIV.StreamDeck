function ChatPI(inContext, inLanguage, inStreamDeckVersion, inPluginVersion) {
    // Init ChatPI
    let instance = this;

    let defaultMessage = "";

    // Inherit from PI
    PI.call(this, inContext, inLanguage, inStreamDeckVersion, inPluginVersion);


    // message changed
    function messageChanged(inEvent) {
        // Save the changed message
        settings.message = inEvent.target.value;
        instance.saveSettings();
    }


    this.load = function () {
        // Add text input
        document.getElementById('placeholder').innerHTML = "<div type='textarea' class='sdpi-item'> \
                                <div class='sdpi-item-label' id='message-label'>" + instance.localization["Message"] + "</div> \
                                <input class='sdpi-item-value' type='text' placeholder='...' id='message-input' required/> \
                            </div>";

        // set value
        const input = document.getElementById('message-input');
        input.value = settings.message ?? defaultMessage;

        // Add event listener
        input.addEventListener('change', messageChanged);
    }
}