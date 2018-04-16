using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public class MoveCommand
    {
        /// <summary>
        /// Gets the identifier of the thing which should be moved
        /// </summary>
        public string Dimension { get; private set; }
        /// <summary>
        /// Gets the direction in which movement should occur
        /// </summary>
        public Direction Direction { get; private set; }
    }
    public enum Direction
    {
        None = 0,
        Forwards = 1,
        Backwards = -1
    }
}
