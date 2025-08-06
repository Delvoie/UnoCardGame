using UNO.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UNO
{
    /// <summary>
    /// Pile of the last played cards.
    /// </summary>
    class Pile
    {
        /// <summary>
        /// Collection of the last played cards.
        /// </summary>
        public static List<Card> cards = new List<Card>();

        /// <summary>
        /// Creates a new pile.
        /// </summary>
        public static void Create()
        {
        }

        /// <summary>
        /// Aligns the pile of cards to its proper position.
        /// </summary>
        public static void Align(Form f)
        {
            // Instance of the Random class
            Random r = new Random();

            // Go through all cards in the pile
            foreach (Card card in cards)
            {
                // Set their position to the correct place a bit offset from center
                card.TargetPosition = new Point(f.ClientRectangle.Width / 2 - card.Dimensions.Width / 2 + 170, f.ClientRectangle.Height / 2 - card.Dimensions.Height / 2);
            }
        }

        /// <summary>
        /// Updates the pile of last played cards.
        /// </summary>
        public static void Update()
        {
            // Go through all cards
            foreach (Card card in cards)
            {
                // Update the currently iterated card
                card.Update();
            }
        }

        /// <summary>
        /// Renders the pile.
        /// </summary>
        public static void Draw(Graphics g)
        {
            // For performance reasons, draw only the last 4 or fewer cards.
            for (int i = cards.Count - ((cards.Count - 4 > 4) ? 4 : cards.Count); i < cards.Count; i++)
            {
                // Render the current card.
                cards[i].Draw(g);
            }
        }
    }
}
