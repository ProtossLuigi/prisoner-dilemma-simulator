using SimulatorWebApp.Models;

namespace SimulatorWebApp.Services
{
    public class GameService
    {
        private int popultionSize = 100;
        private int roundsPerGeneration = 100;
        private int roundLength = 500;

        private int tournamentSize = 10;
        private double crossoverD = .25;

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

        private List<Individual> Tournament(List<(Individual, int)> population)
        {
            Random rng = new();
            population = population.OrderBy(_ => rng.Next()).ToList();
            List<List<(Individual, int)>> tournamentPopulations = new();
            var reproducingPopulation = Enumerable.Range(0, population.Count).AsParallel().Select(i =>
            {
                List<(Individual, int)> tournamentPopulation;
                if (i < population.Count - tournamentSize + 1)
                {
                    tournamentPopulation = population.GetRange(i, tournamentSize);
                }
                else
                {
                    tournamentPopulation = population.GetRange(i, population.Count - i);
                    tournamentPopulation.AddRange(population.GetRange(0, i + tournamentSize - population.Count));
                }
                return tournamentPopulation.MaxBy(individual => individual.Item2).Item1;
            }).OrderBy(_ => rng.Next()).ToList();
            return Enumerable.Range(0, population.Count).AsParallel().Select(i => Crossover(reproducingPopulation[i], reproducingPopulation[i < population.Count - 1 ? i + 1 : 0])).AsSequential().OrderBy(_ => rng.Next()).ToList();
        }

        private Individual Crossover(Individual parent1, Individual parent2)
        {
            Random rng = new();
            var childMoodThreshold = Crossover(parent1.moodThreshold, parent2.moodThreshold, rng);
            var childMemoryLength = Crossover(parent1.memoryLength, parent2.memoryLength, rng);
            var childWeights = CrossoverMemory(childMemoryLength, parent1.weights, parent2.weights, rng);
            return new Individual(childMemoryLength, childMoodThreshold, childWeights);
        }

        private double Crossover(double a, double b, Random? rng = null)
        {
            if (rng == null) rng = new Random();
            var beta = rng.NextDouble() * (1 + 2 * crossoverD) - crossoverD;
            return a * beta + b * (1 - beta);
        }

        private int Crossover(int a, int b, Random? rng = null)
        {
            return (int)Math.Round(Crossover((double)a, b, rng));
        }

        private double[,,] CrossoverMemory(int memoryLength, double[,,] a, double[,,] b, Random? rng = null)
        {
            if (rng == null) rng = new Random();
            var dims = Enumerable.Range(0, a.Rank).Select(i => a.GetLength(i)).ToArray();
            var newVal = new double[memoryLength, 2, 2];
            for (int i = 0; i < newVal.GetLength(0); i++)
            {
                for (int j = 0; j < newVal.GetLength(1); j++)
                {
                    for (int k = 0; k < newVal.GetLength(2); k++)
                    {
                        if (i < a.GetLength(0))
                        {
                            if (i < b.GetLength(0))
                            {
                                newVal[i, j, k] = Crossover(a[i, j, k], b[i, j, k], rng);
                            }
                            else
                            {
                                newVal[i, j, k] = a[i, j, k];
                            }
                        }
                        else
                        {
                            if (i < b.GetLength(0))
                            {
                                newVal[i, j, k] = b[i, j, k];
                            }
                            else
                            {
                                newVal[i, j, k] = SampleGaussian(rng, 0, 1);
                            }
                        }
                    }
                }
            }
            return newVal;
        }

        private static double SampleGaussian(Random random, double mean, double stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }
    }
}
