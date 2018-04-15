namespace JBlam.NetflixScrape.Extension {
    const feedbackElement = document.createElement('div');
    feedbackElement.style.position = 'absolute';
    feedbackElement.style.top = '10px';
    feedbackElement.style.left = '10px';
    feedbackElement.style.minWidth = '33%';
    feedbackElement.style.maxWidth = '66%';
    feedbackElement.style.opacity = '0.25';
    feedbackElement.style.border = 'solid 2px black';
    feedbackElement.style.background = 'white';
    feedbackElement.style.color = 'red';
    document.body.appendChild(feedbackElement);
    feedbackElement.textContent = "Hi, this is a script";
    console.log("This is a script");

    let port: browser.runtime.Port;
    window.addEventListener('hashchange', async evt => {
        if (port) {
            port.postMessage({ event: 'hash changed', url: evt.newURL });
            console.log("sent hashchange", evt);
        } else {
            console.log("no port available", evt);
        }
    });
    port = getPort();

    function getPort() {
        console.log("attempting to connect");
        try {
            const newPort = browser.runtime.connect();
            console.log("created port");
            newPort.postMessage({ event: 'loaded', url: null });
            newPort.onMessage.addListener(message => {
                console.log("received message:", message);
                feedbackElement.textContent = 'message';
            });
            newPort.onDisconnect.addListener(p => port = getPort());
            return newPort;
        }
        catch (e) {
            console.error(e);
            debugger;
        }
    }
}