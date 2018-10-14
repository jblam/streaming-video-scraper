namespace JBlam.NetflixScrape.Extension {
    /** model type representing the on-screen state in the browser tab */
    export type BrowserState = { state: string /* TODO */ };
    /** model type representing a server-issued command */
    export interface ServerCommand { id: number };
    /** model type representing the content-tab response to a command
     * @see ServerCommand
     */
    export type ServerCommandResponse = OkCommandResponse | RejectedCommandResponse | ErrorCommandResponse;
    interface OkCommandResponse {
        /** the server-issued command identifier */
        id: number,
        /** the outcome description */
        outcome: "ok"
    };
    interface RejectedCommandResponse {
        /** the server-issued command identifier */
        id: number,
        /** the outcome description */
        outcome: "rejected",
        /** the reason why the command was rejected */
        reason: string
    }
    interface ErrorCommandResponse {
        /** the server-issued command identifier */
        id: number,
        outcome: "error",
        message?: string
    }

    function logEvent(event: { name: string, addListener: (...args: any[]) => void }) {
        event.addListener((args: any) => console.log(event.name, args));
    }
    [
        Comms.browserPort.closed,
        Comms.browserPort.loaded,
        Comms.browserPort.stateChange,
        Comms.serverSocket.command
    ].forEach(logEvent);


    function isValidCommand(command: ServerCommand, state: BrowserState): boolean {
        throw new Error("Not implemented");
    }

    Comms.browserPort.loaded.addListener(msg => Comms.serverSocket.postState({ state: msg.url.toString() }));
    Comms.serverSocket.command.addListener(processCommand);
    async function processCommand<T extends ServerCommand>(command: T): Promise<ServerCommandResponse> {
        try {
            if (!isValidCommand(command, Comms.browserPort.currentState)) {
                return { id: command.id, outcome: "rejected", reason: "command not valid for current browser state" };
            }
            return await Comms.browserPort.executeCommandAsync(command);
        } catch (e) {
            return { id: command.id, outcome: "error", message: (e instanceof Error) ? e.message : <string>e };
        }
    }

    export function sendMessage(message: any) {
        return Comms.browserPort.executeCommandAsync({ id: -1, message });
    }


    

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