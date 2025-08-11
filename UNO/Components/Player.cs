using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UNO.Cards;
using UNO.Components;

namespace UNO
{
    /// <summary>
    /// Player class. Handles the cards in their hand and processes their events such as mouse clicks, etc.
    /// </summary>
    class Player
    {
        /// <summary>
        /// Collection of cards the player has in hand
        /// </summary>
        public static List<Card> hand = new List<Card>();

        /// <summary>
        /// True if the player is on their turn and can play
        /// </summary>
        public static bool canPlay = true;

        /// <summary>
        /// Player update method
        /// </summary>
        public static void Update()
        {
            // Go through all cards in hand
            foreach (Card card in hand)
            {
                // Can the player play?
                if (canPlay)
                {
                    // Is there a card already played on the pile?
                    if (Pile.cards.Count > 0)
                    {
                        // Top card of the pile
                        Card pileTop = Pile.cards.Last();

                        // Can this card be played?
                        if (Helper.CompareCards(card, pileTop))
                        {
                            // Card is playable
                            card.Playable = true;
                        }
                        else
                        {
                            // Card cannot be played now
                            card.Playable = false;
                        }
                    }
                    else
                    {
                        // No card has been played on the pile yet. Most likely the game just started, so we can play any card.
                        card.Playable = true;
                    }
                }
                else
                {
                    // Player cannot play – disable the playable outline
                    card.Playable = false;
                }

                // Finally, call the card’s own update method
                card.Update();
            }

            // Number of cards we can play
            int playable = 0;

            // Go through all cards in hand
            foreach (Card card in hand)
            {
                // Can this card be played?
                if (card.Playable)
                    // Yes – increase total playable card count
                    playable++;
            }

            // Is the player on their turn?
            if (canPlay)
            {
                // Helper variable – top card of the deck
                Card top = Deck.cards.Last();

                // If we can’t play any card
                if (playable == 0)
                {
                    // We have nothing to play
                    // Highlight the top card of the deck
                    top.Playable = true;
                }
                else
                {
                    // We have something to play
                    // No need to highlight the top card of the deck
                    top.Playable = false;
                }

                // Update the top card
                top.Update();
            }
        }

        /// <summary>
        /// Draws the player’s hand
        /// </summary>
        public static void Draw(Form f, Graphics g)
        {
            // Go through all cards in hand
            for (int i = 0; i < hand.Count; i++)
            {
                // Get the current card – helper variable
                Card card = hand[i];

                // Set Z-Index
                card.ZIndex = i;

                // Finally, draw the card – call its draw method
                card.Draw(g);
            }
        }

        /// <summary>
        /// Aligns the player’s hand
        /// </summary>
        public static void AlignHand(Form f)
        {
            // Total width of all cards in hand
            int handWidth = 0;

            // Offset of each card. (how much each card will overlap)
            int xOffset = -(hand.Count * 7);

            // Calculate the total width of cards including the offset
            // by going through all cards in hand
            foreach (Card card in hand)
            {
                // Increase total hand width by the width of the current card
                handWidth += card.Dimensions.Width + xOffset;
            }

            // Starting position for the first card in hand
            int start = (f.ClientRectangle.Width / 2) - (handWidth / 2) + (xOffset / 2);

            // Position the hand
            for (int i = 0; i < hand.Count; i++)
            {
                // Set the card’s target position based on calculations above
                // This system allows card movement animations.
                hand[i].TargetPosition = new Point(start + ((hand[i].Dimensions.Width + xOffset) * i), f.ClientRectangle.Height - hand[i].Dimensions.Height - 20);
            }
        }

        /// <summary>
        /// Player’s draw method. (Not drawing/rendering). 
        /// This method takes an integer parameter that represents the number of cards the player should draw.
        /// </summary>
        public static async void Draw(int count)
        {
            // Repeat X times where X is the number of cards to draw (count)
            foreach (var i in Enumerable.Range(0, count))
            {
                // Check if there are any cards left in the deck
                if (Deck.cards.Count == 0)
                    Deck.Create();

                // Check if the player still has less than the maximum of 20 cards in hand
                if (hand.Count == 20)
                    return;

                // Top card of the deck
                Card top = Deck.cards[Deck.cards.Count - 1];

                // Remove top card from deck (player draws it)
                Deck.cards.Remove(top);

                // Reset the card’s rotation
                top.Settle(0);

                // Flip the card face up so we can see it
                top.Flip();

                // Trigger the card’s zoom animation
                top.Zoom();

                // Add card to the hand
                hand.Add(top);

                // Update layout and send all cards where they belong
                Game.container.RefreshLayout();

                // Add a small delay so we don’t draw all cards instantly
                await Task.Delay(350);
            }
        }

        /// <summary>
        /// Handles the player’s click actions (events)
        /// </summary>
        public static void Click(Form f)
        {
            // Collection of cards that were clicked – initialize it for now
            List<Card> clickedCards = new List<Card>();

            // Go through all cards in hand
            foreach (Card card in hand)
            {
                // Get mouse position
                Point p = f.PointToClient(Cursor.Position);

                // Card bounds
                Rectangle bounds = new Rectangle(card.Position.X, card.Position.Y, card.Dimensions.Width, card.Dimensions.Height);

                // Did the user click this card?
                if (bounds.Contains(p))
                {
                    // Add the card to the collection
                    clickedCards.Add(card);
                }
            }

            // Did we click on any card?
            if (clickedCards.Count == 0)                
                return;

            // Trigger the click method of the card closest to the player
            clickedCards.OrderByDescending(item => item.ZIndex).First().MouseClick();

            // Reset the clicked cards collection for the next click event
            clickedCards.Clear();
        }

        // Player’s play method.
        public static void Play()
        {
            // Allow the player to play
            canPlay = true;
        }

        /// <summary>
        /// Ends the player’s turn
        /// </summary>
        public static void End()
        {
            // End the player’s turn
            canPlay = false;
        }
    }
}
