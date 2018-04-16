using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class SelectCommand
    {
        /// <summary>
        /// Gets the scope of the selection
        /// </summary>
        public string Scope { get; private set; }
        /// <summary>
        /// Gets the identifier of the item to select
        /// </summary>
        public string Item { get; private set; }

        /// <summary>
        /// Gets the type of select action which should be invoked, or null to do the default select action
        /// </summary>
        public SelectAction? Action { get; private set; }
    }

    public enum SelectAction
    {
        Preview,
        Focus,
        Activate
    }
}
