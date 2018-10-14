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
        public ICollection<BrowseCategoryModel> Categories { get; private set; }
        /// <summary>
        /// Gets or sets a flat list of shows currently available in the DOM
        /// </summary>
        public BrowseSelectionModel Selection { get; private set; }
    }
}
