﻿namespace JBlam.NetflixScrape.Extension.Comms {
    interface PortCommandResponse {
        key: number,
        response: ServerCommandResponse
    }
    interface PortCommand {
        key: number,
        command: ServerCommand
    }

    /** event args raised when content page loads, or changes its URL */
    export interface BrowserLoadedArgs {
        /** the URL which has loaded in a browser tab */
        url: string | Location | URL;
    };

    /** Represents a persistent object wrapper around the currently-active content tab message port */
    export interface IBrowserPort {
        /** event raised when the content tab loads or changes URL */
        readonly loaded: Util.IEvent<BrowserLoadedArgs>;
        /** event raised when the content tab state changes */
        readonly stateChange: Util.IEvent<BrowserState>;
        /** the current on-screen content tab state, or null if no content tab is available */
        readonly currentState: BrowserState;
        /** Event rasied when no content tabs are accessible.
         *
         * Implementations should not raise this event if the content page is immediately reloaded e.g. during navigation.
         */
        readonly closed: Util.IEvent<void>;
        /** Attempts to invoke a command on the content tab, when that tab becomes available
         * @param command the command to invoke
         */
        executeCommandAsync<T extends ServerCommand>(command: T): Promise<ServerCommandResponse>;
    }
    /** represents a persistent object wrapper around the socket connection to the server */
    export interface IServerSocket {
        /** event raised when the server issues a command */
        readonly command: Util.IEvent<ServerCommand>;
        /** notifies the server that the content tab state has changed
         * @param state the state model to notify
         */
        postState(state: BrowserState): Promise<void>;
    }



    function isLoadArgs(message: object): message is BrowserLoadedArgs {
        return "url" in message;
    }
    function isStateChange(message: object): message is BrowserState {
        return false; // TODO!
    }
    function isCommandResponse(message: object): message is PortCommandResponse {
        return "key" in message && "response" in message;
    }

    const browserPortTimeout_ms = 1000;
    class BrowserPort implements IBrowserPort {
        constructor() {
            this.stateChange.addListener(state => this.currentState = state);
            this.loaded.addListener(loadArgs => this.currentState = null);
            browser.runtime.onConnect.addListener(port => {
                if (this.closingTimeout) {
                    clearTimeout(this.closingTimeout);
                    console.log("Clearing timeout", this.closingTimeout, performance.now());
                    this.closingTimeout = null;
                }
                port.onDisconnect.addListener(p => {
                    if (p !== this.futurePort.value()) {
                        console.log("obsolete port closed");
                        return;
                    }
                    this.futurePort.reset();
                    this.currentState = null;
                    if (this.closingTimeout) {
                        console.log("Timeout already exists", this.closingTimeout, performance.now());
                    } else {
                        this.closingTimeout = setTimeout(() => {
                            console.log("Timeout elapsed", this.closingTimeout, performance.now());
                            console.log(this.futurePort, port);
                            this.closingTimeout = null;
                            this.closed.raise(null);
                        }, browserPortTimeout_ms);
                    }
                    console.log("Active timeout", this.closingTimeout, performance.now());
                });
                port.onMessage.addListener((message: BrowserLoadedArgs | BrowserState | PortCommandResponse | {}) => {
                    if (isLoadArgs(message)) {
                        // the content tab may not be aware of its own URL, so the incoming message property may be unset
                        message.url = message.url || port.sender.url || null;
                        this.loaded.raise(message);
                    } else if (isStateChange(message)) {
                        this.stateChange.raise(message);
                    } else if (isCommandResponse(message)) {
                        this.resolver.resolve(message.key, message.response);
                    } else {
                        let key = (message as any).key;
                        if (typeof key == "number") {
                            this.resolver.reject(key, new Error(`Unrecognisable response "${JSON.stringify(message)}"`));
                        } else {
                            throw new Error(`Unrecognisable port message "${JSON.stringify(message)}"`);
                        }
                    }
                });
                this.futurePort.set(port);
            });
            browser.webNavigation.onHistoryStateUpdated.addListener(async evt => {
                if (evt.tabId != (await this.futurePort).sender.tab.id) {
                    // event was received from a tab which is not *the* tab. ignore.
                    return;
                }
                this.loaded.raise({ url: evt.url });
            }, { url: [{ hostEquals: 'netflix' }, { hostEquals: 'localhost' }, { hostEquals: '127.0.0.1' }] });
        }
        public readonly loaded = new Util.Event<BrowserLoadedArgs>("loaded");
        public readonly stateChange = new Util.Event<BrowserState>("stateChange");
        public readonly closed = new Util.Event<void>("closed");
        public currentState: BrowserState;
        public async executeCommandAsync(command: ServerCommand): Promise<ServerCommandResponse> {
            let resolution = this.resolver.enregister();
            (await this.futurePort).postMessage({ key: resolution.key, command });
            return resolution.promise;
        }
        
        private readonly resolver = new Util.PromiseResolver<ServerCommandResponse>();
        private readonly futurePort = new Util.Future<browser.runtime.Port>();
        private closingTimeout: number|null = null;
    }
    export const browserPort: IBrowserPort = new BrowserPort();

    class ServerSocket implements IServerSocket {
        constructor(port: IBrowserPort) {
            port.loaded.addListener(evt => this.ensureSocketConnected());
            port.closed.addListener(async () => {
                let closingSocket = this.futureSocket;
                this.futureSocket = null;
                (await closingSocket).close();
            });
        }
        readonly command = new Util.Event<ServerCommand>("command");
        async postState(state: BrowserState) {
            if (!this.futureSocket) {
                // This should theoretically be impossible; if a content tab is open,
                // a port should be open; when a port opens, ensureSocketConnected()
                // will ensure that a future-socket is created
                throw new Error("No open socket");
            }
            (await this.futureSocket).send(JSON.stringify(state));
        }

        private ensureSocketConnected() {
            if (!this.futureSocket) {
                this.futureSocket = new Util.Future<WebSocket>();
                let newSocket = new WebSocket("ws://localhost:58687/ws-source");
                newSocket.addEventListener('message', this.onSocketMessage, false);
                newSocket.addEventListener('open', evt => this.futureSocket.set(newSocket), false);
                newSocket.addEventListener('error', evt => this.futureSocket.error(evt), false);
            }
        }
        private onSocketMessage(evt: MessageEvent) {
            let command = evt.data as ServerCommand;
            this.command.raise(command);
        }
        private futureSocket: Util.Future<WebSocket> = null;
    }
    export const serverSocket: IServerSocket = new ServerSocket(browserPort);
}