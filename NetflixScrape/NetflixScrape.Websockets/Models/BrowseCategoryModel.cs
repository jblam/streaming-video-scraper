using System.Collections.Generic;

namespace JBlam.NetflixScrape.Core.Models
{
    public class BrowseCategoryModel
    {
        /// <summary>
        /// Gets the category title
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Gets the available show titles within this category
        /// </summary>
        public ICollection<string> AvailableShowTitles { get; private set; }
        /// <summary>
        /// Gets the count of available scrollable pages of items within the category
        /// </summary>
        public int AvailablePageCount { get; private set; }
        /// <summary>
        /// Gets the currently-visible page index
        /// </summary>
        public int PageIndex { get; private set; }
    }
}
