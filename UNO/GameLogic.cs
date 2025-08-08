using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UNO.WinLogic;
using System.Windows.Forms;
using UNO.Components;

namespace UNO
{
    
    // class to store the game's logic
    public static class GameLogic
    {
        // ensuring the win/loss methods are only ever called the one time (when you've won or lost)
        private static bool winOrLossHandled = false;
        // lock object to make sure the game can only end one time
        private static readonly object gameEndLock = new object();
        // method to check the game's current status (win/loss)
        public static void CheckGameEnd()
        {
            // locking to prevent the game from ending more than once and subsequently causing a bug
            lock (gameEndLock)
            
                // if the game ending is already handled, this will do nothing
                if (winOrLossHandled)
                    return;
            // checking the player hand to see if they've got any cards left
                if (Player.hand.Count == 0)
                {
                // earlier stated bool flips to true if game is won
                    winOrLossHandled = true;
                    // if game is won, it'll show the end game dialogue for winning
                    ShowEndGameDialogue(GameResult.Win);
                }
                // otherwise, if the Ai player's hand is empty
                else if (Ai.hand.Count == 0)
                {
                // then the earlier stated bool still flips to true because you've lost
                    winOrLossHandled = true;
                    // and this time you're shown the dialogue for losing
                    ShowEndGameDialogue(GameResult.Lose);
                }
            }
        // method to display the endgame dialogue
        private static void ShowEndGameDialogue(GameResult result)
        {
            // showing the form for if you've won or lost
            using (var form = new WinLogic(result))
            {
                // showing the dialogue
                form.ShowDialog();
            }
        }
    }
}



