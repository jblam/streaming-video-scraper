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
            return "browse";
        }
        function getProfileSelectModel(root: HTMLElement): models.ProfileSelectModel {
            let isProfileSelected = root.querySelector(".profile-link").parentElement.parentElement.classList.contains("account-dropdown-button");
            let profiles = Array.from(root.querySelectorAll(".profile-link"), x => x.textContent.trim());
            if (isProfileSelected) {
                return {
                    $type: "profileSelect",
                    availableProfiles: null,
                    availableProfilesExcludingKids: null,
                    selectedProfile: profiles[0]
                };
            } else {
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
                // TODO: scroll positioning
                return {
                    $type: "browseCategory",
                    title: (titleEl => titleEl && titleEl.textContent)(el.querySelector(".row-header-title")),
                    availableShowTitles: Array.from(el.querySelectorAll(".title-card"), el => el.textContent),
                    availablePageCount: 0,
                    pageIndex: 0
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
            let searchBox = root.querySelector(".searchBox");
            if (searchBox) {
                // TODO: search
                return {
                    $type: "search",
                    availableShows: null,
                    searchTerm: null
                }
            }
            return null;
        }
        function getDetailsModel(root: HTMLElement): models.ShowDetailsModel {
            var el = root.querySelector(".jawBoneContent.open");
            if (!el) { return null; }
            throw new Error("Not implemented");
        }
        var output: models.UiStateModel = {
            $type: "uiState",
            browse: getBrowseModel(root),
            profileSelect: getProfileSelectModel(root),
            search: getSearchModel(root),
            details: getDetailsModel(root),
            state: null
        };
        output.state = getStateEnum(output);
        return output;
    }
    
    (async () => {
        var templateDocument = await loadTemplate('browse');
        console.log(await getState(templateDocument.body));
    })();
}