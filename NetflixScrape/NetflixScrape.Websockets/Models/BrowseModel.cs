using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class BrowseModel
    {
        /// <summary>
        /// Gets or sets the categories currently reachable in the DOM
        /// </summary>
        public IList<string> AvailableCategories { get; set; }
        /// <summary>
        /// Gets or sets a flat list of shows currently available in the DOM
        /// </summary>
        public ICollection<string> AvailableShows { get; set; }
    }
}
