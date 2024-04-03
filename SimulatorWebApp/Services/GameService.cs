using SimulatorWebApp.Models;

namespace SimulatorWebApp.Services
{
    public class GameService
    {
        private int roundLength = 500;

        private int coopVal = 3;
        private int defectWinVal = 5;
        private int defectLossVal = 0;
        private int doubleDefectVal = 1;

        public GameService()
        {

        }

        public (int, int) PlayMatch(Individual player1, Individual player2)
        {
            var gameState = new List<(bool, bool)>();
            int player1Score = 0;
            int player2Score = 0;
            for (int i = 0; i < roundLength; i++)
            {
                gameState.Insert(0, (player1.Act(gameState), player2.Act(gameState)));

                if (gameState[0].Item1)
                {
                    if (gameState[0].Item2)
                    {
                        player1Score += coopVal;
                        player2Score += coopVal;
                    }
                    else
                    {
                        player1Score += defectLossVal;
                        player2Score += defectWinVal;
                    }
                }
                else
                {
                    if (gameState[0].Item2)
                    {
                        player1Score += defectWinVal;
                        player2Score += defectLossVal;
                    }
                    else
                    {
                        player1Score += doubleDefectVal;
                        player2Score += doubleDefectVal;
                    }
                }
            }
            return (player1Score, player2Score);
        }
    }
}
