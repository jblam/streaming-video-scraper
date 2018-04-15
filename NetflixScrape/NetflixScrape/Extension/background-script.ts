namespace JBlam.NetflixScrape.Extension {
    function logEvent(event: { name: string, addListener: (...args: any[]) => void }) {
        event.addListener((args: any) => console.log(event.name, args));
    }
    [
        Comms.browserPort.closed,
        Comms.browserPort.loaded,
        Comms.browserPort.stateChange,
        Comms.serverSocket.command
    ].forEach(logEvent);

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