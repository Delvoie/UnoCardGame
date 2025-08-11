using System;
using System.Windows.Forms;

namespace UNO
{
    public partial class WinLogic : Form
    {
        public enum GameResult
        {
            Win,
            Lose
        }

        public WinLogic(GameResult result)
        {
            InitializeComponent();
            InitializeUI(result);
        }

        // method to update the UI elements depending on if you win or lose
        private void InitializeUI(GameResult result)
        {
            // setting the window's text
            this.Text = result == GameResult.Win ? "Victory!" : "Defeat!";

            // setting the label's text, depending on the game result
            this.winLoseLabel.Text = result == GameResult.Win
                ? "You won! Do you want to play again? Or exit the application?"
                : "You lost! Would you like to play again? Or do you wish to quit?";


        }
        // reset button functionality
        private void resetButton_Click(object sender, EventArgs e)
        {
            // restarts the application
            Application.Restart();
        }
        // exit button functionality
        private void exitButton_Click(object sender, EventArgs e)
        {
            {
                // exit's the application
                Application.Exit();
            }
        }
    }
    }