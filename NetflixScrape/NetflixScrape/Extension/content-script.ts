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

    browser.runtime.onMessage.addListener(message => {
        console.log("received message:", message);
        feedbackElement.textContent = message;
    });

    browser.runtime.sendMessage(`page loaded: ${location}`);

    window.addEventListener('hashchange', async evt => {
        await browser.runtime.sendMessage(`hash changed: ${evt.newURL}`);
        console.log("sent hashchange", evt);
    })
}