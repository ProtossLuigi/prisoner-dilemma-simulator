using System.Linq;

namespace SimulatorWebApp.Models
{
    public class Individual
    {
        public int memoryLength { get; set; }
        public double moodThreshold { get; set; }
        public double[,,] weights { get; set; }

        public Individual(int memoryLength, double moodThreshold, double[,,] weights)
        {
            this.memoryLength = memoryLength;
            this.moodThreshold = moodThreshold;
            this.weights = weights;
        }

        public bool Act(List<(bool, bool)> gameState)
        {
            var len = gameState.Count < memoryLength ? gameState.Count : memoryLength;
            var currMood = 0d;
            for ( var i = 0; i < len; i++ )
            {
                for ( var player = 0; player < len; player++ )
                {
                    currMood += weights[i, player, (player == 0 ? gameState[i].Item1 : gameState[i].Item2) ? 1 : 0];
                }
            }
            return currMood > moodThreshold;
        }
    }
}
