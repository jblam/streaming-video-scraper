namespace JBlam.NetflixScrape.Extension {
    import models = JBlam.NetflixScrape.Core.Models;
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
                console.log(getState(document.body));
            });
            newPort.onDisconnect.addListener(p => port = getPort());
            return newPort;
        }
        catch (e) {
            console.error(e);
            debugger;
        }
    }



    class Slider {
        constructor(el: Element) {
            if (!el.classList.contains("slider")) { throw new Error("Root element does not contain the CSS class 'slider'"); }
            this.root = el;
            this.paginationRoot = el.querySelector(".pagination-indicator");
            this.pageCount = this.paginationRoot
                ? this.paginationRoot.querySelectorAll("li").length
                : null;
        }

        private readonly root: Element;
        private readonly paginationRoot: Element | null;

        public get visibleItems() {
            const selector = ".slider-item:not(.slider-item-)";
            let root = this.root;
            return Array.from(root.querySelectorAll(selector)).map(el => el.textContent);
        }

        public readonly pageCount: number;
        public get activePage() {
            let activePage = this.paginationRoot && this.paginationRoot.querySelector(".active");
            if (!activePage) { return null; }
            return this.pageCount - this.paginationRoot.querySelectorAll("li.active ~ li").length;
        }

        private click(selector: string) { (<HTMLElement>this.root.querySelector(selector)).click(); }
        public nextPage() { this.click(".handleNext"); }
        public prevPage() { this.click(".handlePrev"); }
    }
    export function getState(root: HTMLElement): models.UiStateModel {
        function getStateEnum(partialState: Partial<models.UiStateModel>): models.UiState {
            if (partialState.profileSelect && partialState.profileSelect.availableProfiles) {
                return "profileSelect";
            }
            if (partialState.search && partialState.search.searchTerm) {
                return "search";
            }
            if (partialState.browse) {
                return "browse";
            }
            if (partialState.watch) {
                return "watch";
            }
            return null;
        }
        function getProfileSelectModel(root: HTMLElement): models.ProfileSelectModel {
            const profileLinkClass = ".profile-link";
            let profileLink = root.querySelector(profileLinkClass);
            if (!profileLink) { return null; }
            let isProfileSelected = profileLink.parentElement.parentElement.classList.contains("account-dropdown-button");
            if (isProfileSelected) {
                return {
                    $type: "profileSelect",
                    availableProfiles: null,
                    availableProfilesExcludingKids: null,
                    selectedProfile: root.querySelector(profileLinkClass).parentElement.getAttribute("aria-label").replace(/ -.*/, "")
                };
            } else {
                let profiles = Array.from(root.querySelectorAll(profileLinkClass), x => x.textContent.trim());
                return {
                    $type: "profileSelect",
                    availableProfiles: profiles,
                    availableProfilesExcludingKids: profiles.filter(p => !p.toLowerCase().includes("kids")),
                    selectedProfile: null
                };
            }
        }
        function getBrowseModel(root: HTMLElement): models.BrowseModel {
            function getCategoryModel(el: Element): models.BrowseCategoryModel {
                const paginationIndicatorSelector = ".pagination-indicator li";
                let availablePageCount = el.querySelectorAll(paginationIndicatorSelector).length;
                let unselectedFollowingPageCount = el.querySelectorAll(paginationIndicatorSelector + ".active ~ li").length;
                return {
                    $type: "browseCategory",
                    title: (titleEl => titleEl && titleEl.textContent)(el.querySelector(".row-header-title")),
                    availableShowTitles: Array.from(el.querySelectorAll(".title-card"), el => el.textContent),
                    availablePageCount: availablePageCount,
                    pageIndex: Math.max(availablePageCount - unselectedFollowingPageCount - 1, 0)
                }
            }
            let allCategories = Array.from(root.querySelectorAll(".lolomoRow"), getCategoryModel);
            if (allCategories.length) {
                return {
                    $type: "browse",
                    categories: Array.from(root.querySelectorAll(".lolomoRow"), getCategoryModel).filter(cat => cat.availableShowTitles.length),
                    selection: null
                };
            }
            return null;
        }
        function getSearchModel(root: HTMLElement): models.SearchModel {
            let searchBox = root.querySelector(".searchBox input");
            if (searchBox) {
                return {
                    $type: "search",
                    availableShows: Array.from(root.querySelectorAll(".title-card"), el => el.textContent),
                    searchTerm: (<HTMLInputElement>root.querySelector(".searchInput input")).value
                }
            }
            return null;
        }
        function getDetailsModel(root: HTMLElement): models.ShowDetailsModel {
            let detailsContainer = root.querySelector(".jawBone");
            if (!detailsContainer) { return null; }
            let episodesSliderElement = detailsContainer.querySelector(".episodesContainer .slider");
            let episodesSlider = episodesSliderElement && new Slider(episodesSliderElement);
            let showTitle = detailsContainer.querySelector("h1, h2, h3");
            let activePane = detailsContainer.querySelector(".jawBonePane");
            function getEpisodesModel(detailsRoot: Element): models.EpisodeSelectModel {
                var episodesRoot = detailsRoot.querySelector(".episodesContainer");
                if (!episodesRoot) { return null; }
                return {
                    $type: "episodeSelect",
                    seasonTitles: [],
                    episodeTitles: episodesSlider.visibleItems,
                    selectedEpisodeIndex: 0,
                    selectedSeasonIndex: 0
                };
            }
            return {
                $type: "showDetails",
                showTitle: showTitle.textContent || showTitle.querySelector("img").alt,
                selectedDetailTab: activePane.id.split('-')[1],
                availableDetailsTabs: Array.from(detailsContainer.querySelectorAll("[role=tablist] [role=link]")).map(el => el.textContent),
                episodes: getEpisodesModel(detailsContainer)
            };
        }
        function getWatchModel(root: HTMLElement): models.WatchModel {
            let video = <HTMLVideoElement>root.querySelector(".VideoContainer video");
            if (video) return {
                $type: "watch",
                duration: video.duration,
                playbackState: video.readyState <= HTMLVideoElement.prototype.HAVE_METADATA
                    ? "waiting"
                    : video.paused
                        ? "paused"
                        : "playing",
                position: video.currentTime,
                showTitle: Array.from(root.querySelector(".video-title").children)
                    .map(c => Array.from(c.children))
                    .reduce((prev, cur) => prev.concat(cur))
                    .map(el => el.textContent).join(" ")
            };
            return null;
        }
        var output: models.UiStateModel = {
            $type: "uiState",
            browse: getBrowseModel(root),
            profileSelect: getProfileSelectModel(root),
            search: getSearchModel(root),
            details: getDetailsModel(root),
            watch: getWatchModel(root),
            state: null
        };
        output.state = getStateEnum(output);
        return output;
    }

}