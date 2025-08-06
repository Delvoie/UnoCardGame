using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;

namespace UNO.Cards
{
    /// <summary>
    /// Card state.
    /// Normal - card is normally visible
    /// Back - card is flipped face down (only the back is visible)
    /// </summary>
    public enum State
    {
        Normal,
        Back
    }

    /// <summary>
    /// Possible card types
    /// Classic - classic card with color and number
    /// </summary>
    public enum Type
    {
        Classic
    }

    /// <summary>
    /// Card class
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Card state
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// Card type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Card color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Card number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Actual position of the card
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Card Z-index – the higher the value, the closer it is on the Z-axis to the user
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Target position of the card
        /// </summary>
        public Point TargetPosition { get; set; }

        /// <summary>
        /// Card dimensions
        /// </summary>
        public Size Dimensions { get; set; }

        /// <summary>
        /// Card rotation
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Can we play this card – Playable highlight
        /// </summary>
        public bool Playable { get; set; }

        /// <summary>
        /// True if the card is face down
        /// </summary>
        public bool Flipped { get; set; }

        /// <summary>
        /// True if the card should flip face down – controls animation
        /// </summary>
        private bool flipDown = false;

        /// <summary>
        /// True if the card should flip face up – controls animation
        /// </summary>
        private bool flipUp = false;

        /// <summary>
        /// Card scale along the X-axis
        /// </summary>
        private float flipX = 1;

        /// <summary>
        /// True if the card should reset its rotation
        /// </summary>
        private bool settleRotation = false;

        /// <summary>
        /// Animation speed
        /// </summary>
        private int speed = 25;

        /// <summary>
        /// True if the zoom animation should run
        /// </summary>
        private bool zoom = false;

        /// <summary>
        /// Maximum zoom size
        /// </summary>
        private int zoomMaximum = 30;

        /// <summary>
        /// Font size increment during zoom
        /// </summary>
        private int zoomScale = -20;

        /// <summary>
        /// Target angle
        /// </summary>
        private int targetAngle = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public Card(Color color, int number)
        {
            // Initializes the initial internal state of the object 
            Color = color;
            Number = number;

            // Defines default card dimensions
            Dimensions = new Size(200, 250);

            // Default state
            State = State.Normal;

            // Default type
            Type = Type.Classic;
        }

        /// <summary>
        /// Card Update method
        /// </summary>
        public void Update()
        {
            // Should the card flip face down
            if (flipDown)
            {
                // Is it not fully flipped yet
                if (flipX > -1)
                {
                    // Rotate a bit more
                    flipX -= 0.1f;

                    // Has the card passed the dead angle (completely perpendicular to us) 
                    if (flipX < 0)
                    {
                        // Flip the card face down
                        State = State.Back;
                    }
                }
                else
                {
                    // Flip complete
                    flipDown = false;

                    // Update flip status
                    Flipped = true;
                }
            }

            // Should the card flip face up
            if (flipUp)
            {
                // Is it not fully flipped yet
                if (flipX < 1)
                {
                    // Rotate a bit more
                    flipX += 0.1f;

                    // Has the card passed the dead angle (completely perpendicular to us) 
                    if (flipX > 0)
                    {
                        // Flip the card face up
                        State = State.Normal;
                    }
                }
                else
                {
                    // Flip complete
                    flipUp = false;

                    // Update flip state
                    Flipped = false;
                }
            }

            // Update the card's position. If the actual position doesn't match the target,
            // move it closer, first by animation speed, then pixel-by-pixel for precision.
            if (Position.X > TargetPosition.X)
                Position = new Point(Position.X - ((Position.X - TargetPosition.X > speed) ? speed : 1), Position.Y);

            if (Position.X < TargetPosition.X)
                Position = new Point(Position.X + ((TargetPosition.X - Position.X > speed) ? speed : 1), Position.Y);

            if (Position.Y > TargetPosition.Y)
                Position = new Point(Position.X, Position.Y - ((Position.Y - TargetPosition.Y > speed) ? speed : 1));

            if (Position.Y < TargetPosition.Y)
                Position = new Point(Position.X, Position.Y + ((TargetPosition.Y - Position.Y > speed) ? speed : 1));

            // Handle rotation
            // Should the card settle
            if (settleRotation)
            {
                // Is rotation greater than the target angle
                if (Rotation > targetAngle)
                {
                    Rotation -= (Math.Abs(Rotation-targetAngle) > 15) ? 15 : 1;
                }
                // Is rotation less than the target angle
                else if (Rotation < targetAngle)
                {
                    Rotation += (Math.Abs(Rotation-targetAngle) > 15) ? 15 : 1;
                }
                // Target and actual rotation match
                else
                {
                    settleRotation = false;
                }
            }

            // Handle zoom animation
            if (zoom)
            {
                // Is zoom still less than max
                if (zoomScale < zoomMaximum)
                {
                    zoomScale += 2;
                }
                else
                {
                    // Max reached – end zoom
                    zoom = false;

                    // Reverse zoom scale
                    zoomScale = -zoomMaximum;
                }
            }
        }

        /// <summary>
        /// Drawing method. Renders the card based on its values.
        /// </summary>
        public void Draw(Graphics g)
        {
            // If the card is face down
            if (State == State.Back)
            {
                
                int borderPadding = 15;

                
                g.TranslateTransform(Position.X + (Dimensions.Width / 2), Position.Y + (Dimensions.Height / 2));

                
                g.RotateTransform(Rotation);

                
                g.ScaleTransform(flipX, 1);

                
                g.SmoothingMode = SmoothingMode.AntiAlias;

                
                int pbs = 10;

                
                if (Playable)
                {
                    
                    using (LinearGradientBrush brush = new LinearGradientBrush(new Point(-(Dimensions.Width / 2) - pbs, -(Dimensions.Height / 2) - pbs), new Point(Dimensions.Width + (pbs * 2), Dimensions.Height + (pbs * 2)), Color.FromArgb(255, 217, 0), Color.FromArgb(231, 66, 0)))
                        g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) - pbs, -(Dimensions.Height / 2) - pbs, Dimensions.Width + (pbs * 2), Dimensions.Height + (pbs * 2)), 10));
                }


                
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
                    g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) - 1, -(Dimensions.Height / 2) - 1, Dimensions.Width + 2, Dimensions.Height + 2), 10));

                
                g.FillPath(Brushes.White, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2), -(Dimensions.Height / 2), Dimensions.Width, Dimensions.Height), 10));

                
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(45, 45, 45)))
                    g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) + borderPadding, -(Dimensions.Height / 2) + borderPadding, Dimensions.Width - (borderPadding * 2), Dimensions.Height - (borderPadding * 2)), 10));

                
                g.RotateTransform(-45f);

               
                g.FillPath(Brushes.White, Paint.Ellipse(new Point(0, 0), 75, 105));

                
                g.ScaleTransform(flipX, 1);

               
                StringFormat format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;

               
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(45, 45, 45)))
                    g.DrawString("UNO", new Font("Segoe UI", 36, FontStyle.Bold), brush, new Point(), format);

                
                g.SmoothingMode = SmoothingMode.Default;

               
                g.ResetTransform();

                
                return;
            }

            // If the card is a classic type
            if (Type == Type.Classic)
            {
                
                int borderPadding = 15;

                
                g.TranslateTransform(Position.X + (Dimensions.Width / 2), Position.Y + (Dimensions.Height / 2));

                
                g.RotateTransform(Rotation);

                
                g.ScaleTransform(flipX, 1);

                
                g.SmoothingMode = SmoothingMode.AntiAlias;

                
                int pbs = 10;

                
                if (Playable)
                {
                    
                    using (LinearGradientBrush brush = new LinearGradientBrush(new Point(-(Dimensions.Width / 2) - pbs, -(Dimensions.Height / 2) - pbs), new Point(Dimensions.Width + (pbs * 2), Dimensions.Height + (pbs * 2)), Color.FromArgb(255, 217, 0), Color.FromArgb(231, 66, 0)))
                        g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) - pbs, -(Dimensions.Height / 2) - pbs, Dimensions.Width + (pbs * 2), Dimensions.Height + (pbs * 2)), 10));
                }

               
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
                    g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) - 1, -(Dimensions.Height / 2) - 1, Dimensions.Width + 2, Dimensions.Height + 2), 10));

                
                g.FillPath(Brushes.White, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2), -(Dimensions.Height / 2), Dimensions.Width, Dimensions.Height), 10));

               
                using (SolidBrush brush = new SolidBrush(Color))
                    g.FillPath(brush, Paint.RoundedRectangle(new Rectangle(-(Dimensions.Width / 2) + borderPadding, -(Dimensions.Height / 2) + borderPadding, Dimensions.Width - (borderPadding * 2), Dimensions.Height - (borderPadding * 2)), 10));

               
                g.RotateTransform(45f);

                
                g.FillPath(Brushes.White, Paint.Ellipse(new Point(0, 0), 75, 105));

                
                StringFormat format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;

                
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                
                g.RotateTransform(-45f);

                
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(45, 45, 45)))
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 76 - Math.Abs(zoomScale) + zoomMaximum, FontStyle.Bold | FontStyle.Italic), brush, new Point(-3, -3), format);

                
                using (SolidBrush brush = new SolidBrush(Color))
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 76 - Math.Abs(zoomScale) + zoomMaximum, FontStyle.Bold | FontStyle.Italic), brush, new Point(), format);

               
                if (Number == 6)
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 18, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline), Brushes.White, new Point(-76, -105));
                else
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 18, FontStyle.Bold | FontStyle.Italic), Brushes.White, new Point(-76, -105));
                
               
                g.RotateTransform(-180f);

               
                if (Number == 6)
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 18, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline), Brushes.White, new Point(-76, -105));
                else
                    g.DrawString(Number.ToString(), new Font("Segoe UI", 18, FontStyle.Bold | FontStyle.Italic), Brushes.White, new Point(-76, -105));

                
                g.SmoothingMode = SmoothingMode.Default;

                
                g.ResetTransform();
            }
        }

        /// <summary>
        /// Flip animation
        /// </summary>
        public void Flip(bool skipAnimation = false)
        {
            // Is the card face down
            if (Flipped)
            {
                // Start flip up animation
                flipUp = true;

                // Skip animation if parameter is true
                if (skipAnimation)
                {
                    // Stop the animation
                    flipUp = false;

                    // Set state to normal
                    State = State.Normal;

                    // Reset horizontal scale
                    flipX = 1;

                    // Card is no longer face down
                    Flipped = false;
                }
            }
            else
            {
                // Start flip down animation
                flipDown = true;

                // Skip animation if parameter is true
                if (skipAnimation)
                {                   
                    flipDown = false;                 
                    State = State.Back;                   
                    flipX = -1;
                    Flipped = true;
                }
            }
        }

        /// <summary>
        /// Set card angle with animation
        /// </summary>
        public void Settle(int angle)
        {
            // Update target angle
            targetAngle = angle;

            // Start rotation animation
            settleRotation = true;
        }

        /// <summary>
        /// Zoom animation – triggered when drawing a new card
        /// </summary>
        public async void Zoom()
        {
            // Small delay – to trigger animation right after the card enters the hand
            await Task.Delay(400);

            // Start zoom
            zoom = true;
        }

        /// <summary>
        /// Card click event
        /// </summary>
        public void MouseClick()
        {
            // Can the player play
            if (!Player.canPlay)
                return;

            // Is the card in player's hand
            if (Player.hand.Contains(this))
            {
                // Is the card playable
                if (Playable)
                {
                    // Play the card
                    Player.hand.Remove(this);                    
                    Pile.cards.Add(this);

                    // Set random rotation (for visual effect)
                    Settle(new Random().Next(-180, 180));

                    // Refresh layout to reposition cards
                    Game.container.RefreshLayout();
                }
                else
                {
                   
                    return;
                }
            }

            // Not in player's hand, is it in the deck
            if (Deck.cards.Contains(this))
            {
                // Yes, draw one card
                Player.Draw(1);
            }

            // Disable playable outline (for discard pile cards)
            Playable = false;

            // Update card
            Update();

            // End player's turn
            Player.End();

            // Pass turn to Ai
            Ai.Play();
        }
    }
}
