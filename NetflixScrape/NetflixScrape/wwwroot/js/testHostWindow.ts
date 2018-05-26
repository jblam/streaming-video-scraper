/// <reference path="../../Extension/models.d.ts" />
namespace JBlam.NetflixScrape.Test {
    import models = JBlam.NetflixScrape.Core.Models;
    const viewRoot = <HTMLIFrameElement>document.getElementById("view-root");
    export function loadTemplate(templateFileName?: string) {
        if (!templateFileName) {
            viewRoot.src = '';
            return Promise.resolve(viewRoot.contentDocument);
        } else {
            let url = `/ui-state/${templateFileName}.html`;
            let resolve: (value: Document) => void;
            let reject: (reason: Error) => void;
            function loadHandler(evt: Event) {
                viewRoot.removeEventListener('load', loadHandler);
                viewRoot.removeEventListener('error', errorHandler);
                resolve(viewRoot.contentDocument);
            }
            function errorHandler(evt: ErrorEvent) {
                viewRoot.removeEventListener('load', loadHandler);
                viewRoot.removeEventListener('error', errorHandler);
                reject(evt.error);
            }
            viewRoot.addEventListener('load', loadHandler, false);
            viewRoot.addEventListener('error', errorHandler, false);
            var output = new Promise<Document>((res, rej) => {
                resolve = res;
                reject = rej;
            });
            viewRoot.src = url;
            return output;
        }
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
            var el = root.querySelector(".jawBoneContent.open");
            if (!el) { return null; }
            throw new Error("Not implemented");
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
    
    (async () => {
        var templateDocument = await loadTemplate('details-tvshow');
        console.log(await getState(templateDocument.body));
    })();
}