(() => {
    async function getInputValue(id) {
        const el = document.getElementById(id);
        if (el && el.nodeName === "INPUT") {
            return el.value;
        }
        return "";
    }

    window.__akasha_getInputValue = getInputValue;
})();
