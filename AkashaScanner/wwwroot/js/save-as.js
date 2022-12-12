(() => {
    async function saveAsJson(name, content) {
        const opts = {
            suggestedName: name,
            types: [{
                accept: { "application/json": ['.json'] },
            }],
        };
        try {
            const fileHandle = await window.showSaveFilePicker(opts);
            const writable = await fileHandle.createWritable();
            await writable.write(content);
            await writable.close();
            return true;
        } catch (err) {
            return false;
        }
    }

    window.__akasha_saveAsJson = saveAsJson;
})();
