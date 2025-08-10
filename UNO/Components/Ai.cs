using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UNO.Cards;
using UNO.Components;

namespace UNO
{
    class Ai
    {
        /// <summary>
        /// Collection of cards the AI is currently holding.
        /// </summary>
        public static List<Card> hand = new List<Card>();

        /// <summary>
        /// AI update method.
        /// </summary>
        public static void Update()
        {
            // Go through all cards in hand
            foreach (Card card in hand)
            {
                // Then call their individual update methods
                card.Update();
            }
        }

        /// <summary>
        /// Renders the AI's cards that are in hand.
        /// </summary>
        public static void Draw(Form f, Graphics g)
        {
            foreach (Card card in hand)
            {
                // Draw the current card
                card.Draw(g);
            }
        }

        /// <summary>
        /// Handles positioning of each card in the AI's hand
        /// </summary>
        public static void AlignHand(Form f)
        {
            // Total width of all cards in hand
            int handWidth = 0;

            // Offset between individual cards.
            int xOffset = -(hand.Count * 7);

            // Calculate the total width of the hand including offset
            foreach (Card card in hand)
            {
                // Add the width of the current card to total hand width
                handWidth += card.Dimensions.Width + xOffset;
            }

            // Starting position for the first card in hand
            // Ensures cards are always centered
            int start = (f.ClientRectangle.Width / 2) - (handWidth / 2) + (xOffset / 2);

            // Render the AI's hand
            for (int i = 0; i < hand.Count; i++)
            {
                // Set the target position of the card to the new calculated position
                hand[i].TargetPosition = new Point(start + ((hand[i].Dimensions.Width + xOffset) * i), 20);
            }
        }

        /// <summary>
        /// AI's draw method. Not a rendering method 
        /// </summary>
        public static async void Draw(int count)
        {
            // Repeat X times, where X is the number of cards to draw
            foreach (var i in Enumerable.Range(0, count))
            {
                // Check if there are cards left in the deck
                if (Deck.cards.Count == 0)
                    Deck.Create();

                // Check if the AI still has fewer than 20 cards (max hand size)
                if (hand.Count == 20)
                    return;

                // Top card from the deck
                Card top = Deck.cards[Deck.cards.Count - 1];

                // Remove the top card from the deck
                Deck.cards.Remove(top);

                // Reset card rotation
                top.Settle(0);

                // Add card to AI hand
                hand.Add(top);

                // Refresh the layout so the movement animation from deck to hand plays
                Game.container.RefreshLayout();

                // Add delay so cards are drawn gradually and not all at once
                await Task.Delay(350);
            }
        }

        /// <summary>
        /// AI play method 
        /// </summary>
        public async static void Play()
        {
            // Create a random delay between 0.75 and 1.5 seconds to simulate AI "thinking"
            await Task.Delay(new Random().Next(750, 1500));

            // Was a card already played
            if (Pile.cards.Count == 0)
            {
                // Play the least valuable card in hand
                PlayCard(ChooseBestCard(hand));
            }
            else
            {
                // A card has already been played
                // Get the last played card
                Card lastPlayed = Pile.cards.Last();

                // List of all cards the Ai can potentially play 
                List<Card> playable = new List<Card>();

                // Check each card in hand
                foreach (Card card in hand)
                {
                    // Check if the current card is compatible with the last played one
                    if (CanPlayCard(card, lastPlayed))
                    {
                        // The card is playable — add to the list
                        playable.Add(card);
                    }
                }

                // Can the Ai play anything?
                if (playable.Count == 0)
                {
                    // No playable cards — draw one
                    Draw(1);
                }
                else
                {
                    // The Ai can play only one option
                    if (playable.Count == 1)
                    {
                        // No choice — play the only card
                        PlayCard(playable.First());
                    }
                    else
                    {
                        // Ai has multiple options Use helper method to pick the best card
                        PlayCard(ChooseBestCard(playable));
                    }
                }

                // Clear playable list for next time
                playable.Clear();
            }

            // Hand the turn over to the player
            Player.Play();
        }

        /// <summary>
        /// Checks if a card can be played on top of another card (AI version)
        /// </summary>
        /// <param name="cardToPlay">The card AI wants to play</param>
        /// <param name="topCard">The current top card of the discard pile</param>
        /// <returns>True if the card can be played, false otherwise</returns>
        private static bool CanPlayCard(Card cardToPlay, Card topCard)
        {
            // If there's no top card (shouldn't happen in normal gameplay), allow any card
            if (topCard == null)
                return true;

            // Can play if colors match
            if (cardToPlay.Color.ToArgb() == topCard.Color.ToArgb())
                return true;

            // Can play if it's the same type of special card
            if (cardToPlay.Type == topCard.Type && cardToPlay.Type != UNO.Cards.Type.Classic)
                return true;

            // Can play if numbers match (for classic cards)
            if (cardToPlay.Type == UNO.Cards.Type.Classic && topCard.Type == UNO.Cards.   Type.Classic && cardToPlay.Number == topCard.Number)
                return true;

            return false;
        }

        /// <summary>
        /// Takes a list of cards and picks the one that's most advantageous to play.
        /// Enhanced to prioritize +2 cards strategically
        /// </summary>
        private static Card ChooseBestCard(IEnumerable<Card> cards)
        {
            // Convert to list for easier manipulation
            List<Card> cardList = cards.ToList();

            // Strategy 1: If AI has many cards and player has few cards, prioritize +2 cards
            if (hand.Count > Player.hand.Count && Player.hand.Count <= 3)
            {
                Card plus2Card = cardList.FirstOrDefault(c => c.Type == UNO.Cards.Type.Plus2);
                if (plus2Card != null)
                {
                    return plus2Card;
                }
            }

            // Strategy 2: If AI has few cards left, avoid playing +2 cards unless necessary
            // (since player might counter with their own +2)
            if (hand.Count <= 2)
            {
                Card nonPlus2Card = cardList.FirstOrDefault(c => c.Type != UNO.Cards.Type.Plus2);
                if (nonPlus2Card != null)
                {
                    return nonPlus2Card;
                }
            }

            // Strategy 3: Color balancing - play cards of colors AI has fewer of
            // Create a dictionary to hold counts of cards per color
            Dictionary<Color, int> balance = new Dictionary<Color, int>();

            // Fill the dictionary
            balance.Add(Deck.red, hand.Where(item => item.Color == Deck.red).Count());
            balance.Add(Deck.blue, hand.Where(item => item.Color == Deck.blue).Count());
            balance.Add(Deck.green, hand.Where(item => item.Color == Deck.green).Count());
            balance.Add(Deck.yellow, hand.Where(item => item.Color == Deck.yellow).Count());

            // Sort the dictionary by count ascending — top color is the one with the fewest cards
            // (We want to get rid of colors we have fewer of)
            var ordered = balance.OrderBy(item => item.Value);

            // Check the dictionary for colors we have fewer of first
            foreach (KeyValuePair<Color, int> entry in ordered)
            {
                // Look for regular cards of this color first
                foreach (Card card in cardList.Where(c => c.Type == UNO.Cards.Type.Classic))
                {
                    if (card.Color == entry.Key)
                    {
                        return card;
                    }
                }

                // Then look for +2 cards of this color
                foreach (Card card in cardList.Where(c => c.Type == UNO.Cards.Type.Plus2))
                {
                    if (card.Color == entry.Key)
                    {
                        return card;
                    }
                }
            }

            // If AI couldn't decide using strategies, just return the first playable card
            return cardList.First();
        }

        /// <summary>
        /// Takes a card as a parameter — this card will be played by the ai.
        /// Enhanced to handle special card effects
        /// </summary>
        public static void PlayCard(Card card)
        {
            // Remove the card from the AI's hand
            hand.Remove(card);

            // Add the card to the pile of played cards
            Pile.cards.Add(card);

            // Flip the card face-up (with animation)
            card.Flip();

            // Randomly rotate the card (just for visual effect)
            card.Settle(new Random().Next(-180, 180));

            // Handle special card effects
            HandleSpecialCardEffect(card);

            // Align the game layout 
            Game.container.RefreshLayout();
        }

        /// <summary>
        /// Handle special effects when AI plays special cards
        /// </summary>
        private static void HandleSpecialCardEffect(Card playedCard)
        {
            switch (playedCard.Type)
            {
                case UNO.Cards.Type.Plus2:
                    // Force player to draw 2 cards
                    Player.Draw(2);

                    // Optional: You could add a message or visual effect here
                    // MessageBox.Show("Player draws 2 cards!", "AI played Draw 2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                default:
                    // No special effect for classic cards
                    break;
            }
        }
    }
}