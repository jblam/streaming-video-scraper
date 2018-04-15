using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class ShowDetailsModel
    {
        public ICollection<string> SeasonTitles { get; private set; }
        public int SelectedSeasonIndex { get; private set; }
        public ICollection<string> EpisodeTitles { get; private set; }
        public int SelectedEpisodeIndex { get; private set; }
    }
}
