using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class UiStateModel
    {
        /// <summary>
        /// Gets or sets the current UI state
        /// </summary>
        public UiState State { get; set; }

        /// <summary>
        /// Gets or sets the UI search model, if available in this state
        /// </summary>
        public SearchModel Search { get; set; }

        /// <summary>
        /// Gets or sets the available and selected profiles, if available in this state
        /// </summary>
        public ProfileSelectModel ProfileSelect { get; set; }

        public BrowseModel Browse { get; set; }
    }

    public enum UiState
    {
        ProfileSelect,
        Browse,
        Search,
        Watch
    }
}
