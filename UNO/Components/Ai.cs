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
                    if (Helper.CompareCards(card, lastPlayed))
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
            // checking to see if the Ai has won
            GameLogic.CheckGameEnd();
        }

        /// <summary>
        /// Takes a list of cards and picks the one that's most advantageous to play.
        /// </summary>
        private static Card ChooseBestCard(IEnumerable<Card> cards)
        {
            // Create a dictionary to hold counts of cards per color
            Dictionary<Color, int> balance = new Dictionary<Color, int>();

            // Fill the dictionary
            balance.Add(Deck.red, hand.Where(item => item.Color == Deck.red).Count());
            balance.Add(Deck.blue, hand.Where(item => item.Color == Deck.blue).Count());
            balance.Add(Deck.green, hand.Where(item => item.Color == Deck.green).Count());
            balance.Add(Deck.yellow, hand.Where(item => item.Color == Deck.yellow).Count());

            // Sort the dictionary by count descending — top color is the one with the most cards
            var ordered = balance.OrderBy(item => item.Value).Reverse();

            // Check the dictionary
            foreach (KeyValuePair<Color, int> entry in ordered)
            {
                // Loop through all playable cards
                foreach (Card card in cards)
                {
                    // Pick the first card that matches the most frequent color
                    if (card.Color == entry.Key)
                    {
                        return card;
                    }
                }
            }

            // If AI couldn't decide, just return the first playable card
            return cards.First();
        }

        /// <summary>
        /// Takes a card as a parameter — this card will be played by the ai.
        /// </summary>
        public static void PlayCard(Card card)
        {
            // Remove the card from the enemy's hand
            hand.Remove(card);

            // Add the card to the pile of played cards
            Pile.cards.Add(card);

            // Flip the card face-up (with animation)
            card.Flip();

            // Randomly rotate the card (just for visual effect)
            card.Settle(new Random().Next(-180, 180));

            // Align the game layout 
            Game.container.RefreshLayout();
        }
    }
}
