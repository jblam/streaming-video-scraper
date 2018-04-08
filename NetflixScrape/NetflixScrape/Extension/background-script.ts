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
    });
}