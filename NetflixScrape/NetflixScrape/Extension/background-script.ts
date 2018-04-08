namespace JBlam.NetflixScrape.Extension {
    
    browser.webNavigation.onHistoryStateUpdated.addListener(async evt => {
        console.log("history state updated");
        let tabs = await browser.tabs.query({ currentWindow: true, active: true });
        for (let t of tabs) {
            await browser.tabs.sendMessage(t.id, `onHistoryStateUpdated: ${evt.url}`);
            console.log("messaged tab", t.id);
        }
    });

    browser.runtime.onMessage.addListener(message => {
        console.log("background received message:", message);
    });
}