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
    let ports: browser.runtime.Port[] = [];

    browser.runtime.onConnect.addListener(port => {
        port.onMessage.addListener(message => onPortMessage(port, message as PortMessage));
        ports.push(port);
        console.log(`Adding new port; now ${ports.length} ports`);
        if (ports.length > 0) {
            asyncSocket = asyncSocket || connectSocket("ws://localhost:58687/ws-source");
        }
        port.onDisconnect.addListener(onPortDisconnect);
    });
    function onPortDisconnect(port: browser.runtime.Port) {
        console.log("port disconnecting");
        let oldPorts = ports;
        ports = oldPorts.filter(p => p != port);
        console.log(`Removed ${oldPorts.length - ports.length} ports; ${ports.length} remain`);
        if (ports.length == 0) {
            setTimeout(closeSocket, 5000);
        }
    }
    async function onPortMessage(port: browser.runtime.Port, message: PortMessage) {
        if (!message.url) {
            message.url = port.sender.url || "(no url)";
        }
        console.log("background received", message);
        (await asyncSocket).send(JSON.stringify(message));
    }
    async function closeSocket() {
        if (ports.length == 0 && asyncSocket) {
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
        for (let p of ports) {
            p.postMessage({ event: 'remoteMessage', message: evt.data });
        }
    }

    browser.webNavigation.onHistoryStateUpdated.addListener(async evt => {
        let message = { event: 'onHistoryStateUpdated', url: evt.url };
        console.log(message);
        let portMessages = ports.map(p => p.postMessage(message));
        let serverMessage = asyncSocket.then(s => s.send(JSON.stringify(message)));
        await Promise.all([serverMessage, ...portMessages]);
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