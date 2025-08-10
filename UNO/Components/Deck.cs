using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UNO.Cards;

namespace UNO
{
    class Deck
    {
        /// <summary>
        /// Red color. Used globally for all red cards.
        /// </summary>
        public static Color red = Color.FromArgb(231, 76, 60);

        /// <summary>
        /// Blue color. Used globally for all blue cards.
        /// </summary>
        public static Color blue = Color.FromArgb(52, 152, 219);

        /// <summary>
        /// Green color. Used globally for all green cards.
        /// </summary>
        public static Color green = Color.FromArgb(46, 204, 113);

        /// <summary>
        /// Yellow color. Used globally for all yellow cards.
        /// </summary>
        public static Color yellow = Color.FromArgb(241, 196, 15);

        /// <summary>
        /// Collection of all cards in the deck.
        /// </summary>
        public static List<Card> cards = new List<Card>();

        /// <summary>
        /// Shuffles the deck using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle()
        {
            Random rng = new Random();
            int n = cards.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        /// <summary>
        /// Creates a new playing deck with proper UNO card distribution
        /// </summary>
        public static void Create()
        {
            // Resets the card collection if it was previously filled.
            cards.Clear();

            // Creates a new instance of the Random class.
            Random r = new Random();

            // Array of all possible colors a card can have.
            Color[] colors = { red, blue, green, yellow };

            int zIndex = 0;

            // Create numbered cards (0-9) - Standard UNO distribution
            foreach (Color color in colors)
            {
                // Add one 0 card per color
                Card zeroCard = new Card(color, 0);
                zeroCard.ZIndex = zIndex++;
                zeroCard.Flip(true);
                zeroCard.Rotation = r.Next(-180, 180);
                cards.Add(zeroCard);

                // Add two cards for numbers 1-9 per color
                for (int number = 1; number <= 9; number++)
                {
                    for (int count = 0; count < 2; count++)
                    {
                        Card card = new Card(color, number);
                        card.ZIndex = zIndex++;
                        card.Flip(true);
                        card.Rotation = r.Next(-180, 180);
                        cards.Add(card);
                    }
                }

                // Add two +2 cards per color (8 total +2 cards)
                for (int count = 0; count < 2; count++)
                {
                    Card plus2Card = new Card(color, UNO.Cards.Type.Plus2);
                    plus2Card.ZIndex = zIndex++;
                    plus2Card.Flip(true);
                    plus2Card.Rotation = r.Next(-180, 180);
                    cards.Add(plus2Card);
                }

                // TODO: Add Skip and Reverse cards (2 of each color)
                // For now, adding more numbered cards to reach closer to 108 total
            }

            // Fill remaining slots with random cards to maintain deck size
            // A proper UNO deck has 108 cards total
            while (cards.Count < 108)
            {
                Color randomColor = colors[r.Next(colors.Length)];

                // Randomly choose between numbered card and +2 card
                if (r.Next(0, 4) == 0) // 25% chance for +2 card
                {
                    Card plus2Card = new Card(randomColor, UNO.Cards.Type.Plus2);
                    plus2Card.ZIndex = zIndex++;
                    plus2Card.Flip(true);
                    plus2Card.Rotation = r.Next(-180, 180);
                    cards.Add(plus2Card);
                }
                else
                {
                    int randomNumber = r.Next(0, 10);
                    Card fillerCard = new Card(randomColor, randomNumber);
                    fillerCard.ZIndex = zIndex++;
                    fillerCard.Flip(true);
                    fillerCard.Rotation = r.Next(-180, 180);
                    cards.Add(fillerCard);
                }
            }

            // Shuffle the deck after creation
            Shuffle();
        }

        /// <summary>
        /// Places the deck in the correct position on the game window. Called whenever the window is resized,
        /// to make sure the deck remains near the center.
        /// </summary>
        public static void Align(Form f)
        {
            // Loop through all cards in the deck to update their position.
            foreach (Card card in cards)
            {
                // Update each card's position to be near the center of the window.
                card.Position = card.TargetPosition = new Point(f.ClientRectangle.Width / 2 - card.Dimensions.Width / 2 - 170, f.ClientRectangle.Height / 2 - card.Dimensions.Height / 2);
            }
        }

        /// <summary>
        /// Update method for the deck. Ensures that each card's Update method is called.
        /// </summary>
        public static void Update()
        {
            // Loop through all cards in the deck.
            foreach (Card card in cards)
            {
                // Call the Update method on the current card.
                card.Update();
            }
        }

        /// <summary>
        /// Drawing method for the deck. Renders the deck to the screen.
        /// </summary>
        public static void Draw(Graphics g)
        {
            // Only draw the last 5 (or fewer) cards in the deck.
            // This is done to avoid performance issues with Windows Forms
            for (int i = cards.Count - ((cards.Count - 5 > 5) ? 5 : cards.Count); i < cards.Count; i++)
            {
                // Call the Draw method on the current card.
                cards[i].Draw(g);
            }
        }

        /// <summary>
        /// Click method for the deck. Handles what happens when the user clicks on the deck.
        /// </summary>
        public static void Click(Form f)
        {
            // List of cards that the user clicked on.
            // A user may click multiple overlapping cards.
            List<Card> clickedCards = new List<Card>();

            // Iterate backwards through the deck to safely modify the list during iteration.
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                // Get the current card.
                Card card = cards[i];

                // Get mouse position.
                Point p = f.PointToClient(Cursor.Position);

                // Card bounds (rectangle).
                Rectangle bounds = new Rectangle(card.Position.X, card.Position.Y, card.Dimensions.Width, card.Dimensions.Height);

                // Check if the mouse is within the card bounds.
                // If true during a click event, the card was clicked.
                if (bounds.Contains(p))
                {
                    // Add the clicked card to the list.
                    clickedCards.Add(card);
                }
            }

            // If no cards were clicked, exit the click event handler.
            if (clickedCards.Count == 0)
                return;

            // Cards were clicked.
            // Sort the clicked cards by Z-Index descending, and trigger Click method on the topmost card only.
            clickedCards.OrderByDescending(item => item.ZIndex).First().MouseClick();

            // Clear the clicked cards list for the next click.
            clickedCards.Clear();
        }

        /// <summary>
        /// Get the count of cards by type in the deck
        /// </summary>
        public static int GetCardTypeCount(UNO.Cards.Type cardType)
        {
            return cards.Count(card => card.Type == cardType);
        }

        /// <summary>
        /// Get the count of cards by color in the deck
        /// </summary>
        public static int GetCardColorCount(Color cardColor)
        {
            return cards.Count(card => card.Color.ToArgb() == cardColor.ToArgb());
        }
    }
}