using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNO.Cards;

namespace UNO.Components
{
    /// <summary>
    /// Helper class. Contains things that didn’t fit anywhere else.
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Compares two cards. Returns true if they can be played on top of each other.
        /// </summary>
        public static bool CompareCards(Card c1, Card c2)
        {
            // Compare colors
            if (c1.Color == c2.Color)
            {
                return true;
            }

            // Compare numbers
            if (c1.Number == c2.Number)
            {
                return true;
            }

            // Cards have nothing in common, return false
            return false;
        }
    }
}
