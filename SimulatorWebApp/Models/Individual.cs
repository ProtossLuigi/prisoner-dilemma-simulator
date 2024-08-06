namespace SimulatorWebApp.Models
{
    public class Individual
    {
        public int MemoryLength { get; set; }
        public double MoodThreshold { get; set; }
        public double[,,] Weights { get; set; }

        public static readonly Individual AlwaysCooperate = new(0, 0, new double[0, 2, 2]);
        public static readonly Individual AlwaysDefect = new(0, 1, new double[0, 2, 2]);
        public static readonly Individual TitForTat = new(1, 0, new double[1, 2, 2] { { { 0, 0 }, { 0, 0 } } });

        public Individual(int memoryLength, double moodThreshold, double[,,] weights)
        {
            MemoryLength = memoryLength;
            MoodThreshold = moodThreshold;
            Weights = weights;
        }

        public bool Act(List<(bool, bool)> gameState, int playerId)
        {
            var len = gameState.Count < MemoryLength ? gameState.Count : MemoryLength;
            var currMood = 0d;
            for ( var i = 0; i < len; i++ )
            {
                for ( var player = 0; player < len; player++ )
                {
                    currMood += Weights[i, player, (player == playerId ? gameState[i].Item1 : gameState[i].Item2) ? 1 : 0];
                }
            }
            return currMood >= MoodThreshold;
        }

        public bool IsNice()
        {
            return MoodThreshold <= 0;
        }
    }
}
