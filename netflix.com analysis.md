# Netflix

Scraping the UI and interacting with it via script.

## Profile Select

Choose between actual users, "kids", or "new profile". Url: `/browse`

```
function isProfileSelectRequired()
	// if you need to select a profile, there's likely more than one .profile-link;
	// if you're logged in, there's likely a single profile-link under some kind of drop-down
	return !document.querySelector(".profile-link").parentElement.parentElement.classList.contains("account-dropdown-button");
}

function getProfiles() {
	// .profile-link includes both the avatar image and the account name
	return [...document.querySelectorAll(".profile-link")].map(x => x.textContent.trim());
}

function selectProfile(profileName) {
	// .profile-link either is a linked element, or is a child of one
	[...document.querySelectorAll(".profile-link")].filter(x => x.textContent.includes(profileName))[0].click())("Kate");
}
```

## Browse

Content is in rows `div#row-`*`n`*`.rowContainer`, which scroll horizontally using buttons
`.handleNext` and `.handlePrev`. The "back" arrow may not exist until you've gone forwards.

Labels are preceding siblings `h2.rowHeader` although the actual visible text is in `.rowHeader.row-header-title`.

The common parent is `.lolomoRow`. This is a weird name so it looks brittle.

## Select

Selector `.title-cart a` gets all the available shows in-scope (i.e. in the row, or overall).
Clicking on one will expand that (i.e. bring up the episodes/start watching).

Clicking on one gives a `.jawBoneContent.open`; inside that there's `[role=link]` and `[role=button]`.

- overview `[role=link]`
  - next episode `.playLink`
  - my list
  - +1 / -1
  - tabs: Overview, Episodes, More Like This, Details
- episodes `[role=button]`
  - Series dropdown (inside `.episodesContainer`)
    - `.sub-menu-list` has all the series inside that as `[role=link].sub-menu-link`
	- [0] is constant (current selection); other available series will be after that
	
## Un-select

`[role=button].close-button` is the close link, only visible with a `.jawBoneContent.open`.

## Search

`.searchBox` is the parent of the search markup. It lives throughout the full lifecycle.

`.searchTab` is the "open the search" button. It dies when you click it.

`.searchInput` is a wrapper for the actual input; `.searchInput input` gets an input.

React does some [wierd monkey-patch thing](https://stackoverflow.com/a/46012210)
to the `value` property of `.searchInput input`, so you need to get the uninfected prototype method

```
var fuckOffReact = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value").set;
document.querySelector(".searchTab").click();
fuckOffReact.call(document.querySelector(".searchInput input"), "Search term");
$(".searchInput input").dispatchEvent(new Event("input", { bubbles: true }));
```