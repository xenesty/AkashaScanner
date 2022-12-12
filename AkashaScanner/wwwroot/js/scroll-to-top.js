(() => {
    const oldPushState = history.pushState;
    history.pushState = (state, unused, url) => {
        if (url) {
            const parsed = new URL(url);
            if (location.pathname !== parsed.pathname) {
                const mainContent = document.getElementById("main-content");
                if (mainContent) {
                    mainContent.scrollTo(0, 0);
                }
            }
        }
        return oldPushState.call(history, state, unused, url);
    };
})();
