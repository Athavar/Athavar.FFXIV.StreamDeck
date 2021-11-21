function GearSetPI(inContext, inLanguage, inStreamDeckVersion, inPluginVersion) {
    // Init GearSetPI
    let instance = this;

    const defaultSlot = "1";
    const defaultGlamourPlate = "0";


    // Inherit from PI
    PI.call(this, inContext, inLanguage, inStreamDeckVersion, inPluginVersion);


    function slotChanged(inEvent) {
        // Save the changed slot
        settings.slot = inEvent.target.value;
        instance.saveSettings();
    }

    function glamourChanged(inEvent) {
        // Save the changed slot
        settings.glamourPlate = inEvent.target.value;
        instance.saveSettings();
    }


    this.load = function () {
        // Add text input
        document.getElementById('placeholder').innerHTML = " \
            <div type='textarea' class='sdpi-item'> \
                <div class='sdpi-item-label' id='slot-label'>" + instance.localization["GearSet"] + "</div> \
                <input class='sdpi-item-value' type='text' placeholder='...' id='slot-input' required/> \
            </div> \
            <div type='textarea' class='sdpi-item'> \
                <div class='sdpi-item-label' id='glam-label'>" + instance.localization["GlamourPlate"] + "</div> \
                <input class='sdpi-item-value' type='number' min='0' max='15' id='glam-input' required/> \
            </div>";

        // set value
        const input = document.getElementById('slot-input');
        input.value = settings.slot ?? defaultSlot;
        const input2 = document.getElementById('glam-input');
        input2.value = settings.glamourPlate ?? defaultGlamourPlate;

        // Add event listener
        input.addEventListener('change', slotChanged);
        input2.addEventListener('change', glamourChanged);
    }
}