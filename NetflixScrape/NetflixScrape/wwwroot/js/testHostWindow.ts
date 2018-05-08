/// <reference path="../../Extension/models.d.ts" />
namespace JBlam.NetflixScrape.Test {
    import models = JBlam.NetflixScrape.Core.Models;
    const viewRoot = document.getElementById("view-root");
    function getTemplate(id: string) { return document.getElementById(id) as HTMLTemplateElement; }
    const templates = {
        profileSelected: getTemplate("profile-selected"),
        profileSelect: getTemplate("profile-select"),
        browseRow: getTemplate("browse-row"),
        showDetails: getTemplate("show-details")
    };
    function clearRoot() { while (viewRoot.hasChildNodes()) viewRoot.removeChild(viewRoot.firstChild); }
    export function setProfileSelect() {
        clearRoot();
        let clone = document.importNode(templates.profileSelect.content, true);
        viewRoot.appendChild(clone);
    }
    export function setBrowse() {
        clearRoot();
        viewRoot.appendChild(document.importNode(templates.profileSelected.content, true));
        const categoryCount = 3;
        const showCount = 8;
        for (var i = 0; i < categoryCount; i++) {
            let clone = document.importNode(templates.browseRow.content, true);
            viewRoot.appendChild(clone);
            let category = viewRoot.lastElementChild;
            category.querySelector(".row-header-title").textContent = `cat ${i}`;
            let templateShowCard = category.querySelector(".title-card");
            for (var j = 0; j < showCount; j++) {
                let clonedShow = templateShowCard.cloneNode();
                clonedShow.textContent = `cat ${i} show ${j}`;
                templateShowCard.parentElement.appendChild(clonedShow);
            }
            templateShowCard.parentElement.removeChild(templateShowCard);
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
                    title: el.querySelector(".row-header-title").textContent,
                    availableShowTitles: Array.from(el.querySelectorAll(".title-card"), el => el.textContent),
                    availablePageCount: 0,
                    pageIndex: 0
                }
            }
            let allCategories = Array.from(root.querySelectorAll(".lolomoRow"), getCategoryModel);
            if (allCategories.length) {
                return {
                    $type: "browse",
                    categories: Array.from(root.querySelectorAll(".lolomoRow"), getCategoryModel),
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

    setProfileSelect();
    console.log(getState(viewRoot));
}