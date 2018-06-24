using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class ProfileSelectModel
    {
        /// <summary>
        /// Gets the defined profiles which are selectable in the UI
        /// </summary>
        public ICollection<string> AvailableProfiles { get; set; }

        public IEnumerable<string> AvailableProfilesExcludingKids => AvailableProfiles.Where(p => p != "Kids");

        /// <summary>
        /// Gets or sets the selected profile, or null if no profile is currently selected
        /// </summary>
        public string SelectedProfile { get; set; }
    }
}
