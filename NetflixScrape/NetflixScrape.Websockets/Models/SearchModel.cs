using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class SearchModel
    {
        /// <summary>
        /// Gets or sets the search term currently shown in the UI
        /// </summary>
        public string SearchTerm { get; set; }
        /// <summary>
        /// Gets or sets the available shows listed by the UI
        /// </summary>
        public ICollection<string> AvailableShows { get; set; }
    }
}
