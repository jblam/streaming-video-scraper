namespace JBlam.NetflixScrape.Extension {
    export interface PortMessage {
        event: string;
        url: string | Location;
    }

    let asyncSocket: Promise<WebSocket> = null;
    function connectSocket(url: string): Promise<WebSocket> {
        let resolve: (value: WebSocket) => void;
        let reject: (reason: any) => void;
        let output = new Promise<WebSocket>((res, rej) => {
            resolve = res;
            reject = rej;
        });
        console.log("connecting socket");
        try {
            let newSocket = new WebSocket(url);
            newSocket.addEventListener('message', onSocketMessage, false);
            newSocket.addEventListener('open', evt => resolve(newSocket), false);
        } catch (e) {
            reject(e);
        }
        return output;
    }
    let port: browser.runtime.Port = null;

    browser.runtime.onConnect.addListener(newport => {
        if (port) {
            port.onMessage.removeListener(onPortMessage);
            port.disconnect(); // TODO: will this result in Port.onDisconnect?
        }
        newport.onMessage.addListener(onPortMessage);
        newport.onDisconnect.addListener(onPortDisconnect);
        port = newport;
        console.log('Adding new port');
        asyncSocket = asyncSocket || connectSocket("ws://localhost:58687/ws-source");
    });
    function onPortDisconnect(disconnectingPort: browser.runtime.Port) {
        if (disconnectingPort == port) {
            console.log("active port disconnecting");
            port = null;
            setTimeout(closeSocket, 5000);
        } else {
            console.log("inactive port disconnecting");
        }
    }
    async function onPortMessage(message: PortMessage) {
        if (!message.url) {
            message.url = port.sender.url || "(no url)";
        }
        console.log("background received", message);
        (await asyncSocket).send(JSON.stringify(message));
    }
    async function closeSocket() {
        if (!port && asyncSocket) {
            console.log("disconnecting socket");
            let temp = asyncSocket;
            asyncSocket = null;
            try {
                (await temp).close();
                console.log("disconnected socket");
            } catch (e) {
                console.error("failed to disconnect socket", e);
            }
        } else {
            console.log("not disconnecting socket");
        }
    }
    function onSocketMessage(this: WebSocket, evt: MessageEvent) {
        port.postMessage({ event: 'remoteMessage', message: evt.data });
    }

    browser.webNavigation.onHistoryStateUpdated.addListener(async evt => {
        if (evt.tabId != port.sender.tab.id) {
            // event was received from a tab which is not *the* tab. ignore
            return;
        }
        let message = { event: 'onHistoryStateUpdated', url: evt.url };
        console.log(message);
        let portMessage = port.postMessage(message);
        let serverMessage = asyncSocket.then(s => s.send(JSON.stringify(message)));
        await Promise.all([serverMessage, portMessage]);
    }, { url: [{ hostEquals: 'netflix' }, { hostEquals: 'localhost' }, { hostEquals: '127.0.0.1' }] });




    

    // navigation URL meanings:
    // /browse: profile-select, or actually browsing
    // /browse?jbv={stuff}: 
    // /search?q={term}: search {term}
    // /watch/{id}?{stuff}: watching (play? paused?)
    //
    // flowchart:
    // [ receive 'loaded' ] -> get URL of port \
    // [ receive 'hash change' ] ----(url)-------- (1)
    // [ onHistoryStateUpdated' ] /
    //
    // (1) -- /browse ------ is profile select? -> [ output profile select ]
    //     |              |- is detail view?    -> [ output detail data ]
    //     |              |- otherwise          -> [ output browse data ]
    //     |
    //     |- /search --- [ output search ]
    //     |- /watch ---- [ output watch ]
    //
    //
    // profile-select:
    // inputs
    // - select profile (expect pushstate response from browser)
    // datamodel
    // - availableProfiles: string[]
    //
    // browse:
    // inputs
    // - up/down (next category)
    // - left/right (highlight next/prev in category)
    // - scroll left/right (scroll the category)
    // - jump to category by name
    // - detail selected show
    // - detail show by name
    // - play selected show
    // - play show by name
    // datamodel
    // - availableCategories: { title: string, availableShowTitles: string[], availablePages: number, pageIndex: number }[]
    // - selection: { categoryIndex: number|null, showIndex: number|null }
    //
    // details:
    // inputs
    // - select season
    // - select episode
    // - play next episode
    // - play selected episode
    // - scroll episodes
    // - back
    // datamodel
    // - seasons: string[] (TODO: can this only ever be "season n"?)
    // - seasonIndex: number
    // - episodes: string[] (TODO: simply absent if it's a movie?)
    // - episodeIndex: number
    // UI concept
    // [Play next]
    // Season [1↓] Episode [1↓] [Play]
    //
    // search:
    // inputs
    // - change search term
    // - play show by name
    // - up/down/left/right
    // - (details???)
    // - back
    // datamodel
    // - searchTerm: string
    // - visibleShows: string[]
    //
    // watch:
    // inputs
    // - pause (idempotent)
    // - play (idempotent)
    // - stop (back)
    // - seek (time)
    // datamodel
    // - runtimeSeconds: number
    // - positionSeconds: number
    // - playState: play|pause
}