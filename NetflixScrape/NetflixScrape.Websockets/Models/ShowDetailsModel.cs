using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class ShowDetailsModel
    {
        public string ShowTitle { get; private set; }
        public string SelectedDetailTab { get; private set; }
        public ICollection<string> AvailableDetailsTabs { get; private set; }
        public EpisodeSelectModel Episodes { get; private set; }
    }
}
