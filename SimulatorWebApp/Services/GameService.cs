using SimulatorWebApp.Models;

namespace SimulatorWebApp.Services
{
    public class GameService
    {
        private readonly int popultionSize = 100;
        private readonly int roundsPerGeneration = 100;
        private readonly int roundLength = 500;

        private readonly int tournamentSize = 10;
        private readonly double crossoverD = .25;

        private readonly int coopVal = 3;
        private readonly int defectWinVal = 5;
        private readonly int defectLossVal = 0;
        private readonly int doubleDefectVal = 1;

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

        public List<(Individual, int)> EvaluateGeneration(List<Individual> population)
        {
            Random rng = new();
            List<int> results = [.. Enumerable.Repeat(0, popultionSize)];
            for (int round = 0; round < roundsPerGeneration; round++)
            {
                List<int[]> matchups = [.. Enumerable.Range(0, popultionSize).OrderBy(_ => rng.Next()).Chunk(2)];
                List<(int, int)> roundResults = [.. matchups.AsParallel().Select(ids => PlayMatch(population[ids[0]], population[ids[1]]))];
                for (int i = 0; i < matchups.Count; i++)
                {
                    results[matchups[i][0]] += roundResults[i].Item1;
                    results[matchups[i][1]] += roundResults[i].Item2;
                }
            }
            return [.. Enumerable.Range(0, popultionSize)
                .Select(i => (population[i], results[i]))];
        }

        private List<Individual> Tournament(List<(Individual, int)> population, Random? rng = null)
        {
            rng ??= new Random();
            population = [.. population.OrderBy(_ => rng.Next())];
            List<List<(Individual, int)>> tournamentPopulations = [];
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
            return [.. Enumerable.Range(0, population.Count).AsParallel()
                .Select(i => Crossover(reproducingPopulation[i], reproducingPopulation[i < population.Count - 1 ? i + 1 : 0], rng))
                .AsSequential()
                .OrderBy(_ => rng.Next())];
        }

        private Individual Crossover(Individual parent1, Individual parent2, Random? rng = null)
        {
            rng ??= new Random();
            var childMoodThreshold = Crossover(parent1.MoodThreshold, parent2.MoodThreshold, rng);
            var childMemoryLength = Crossover(parent1.MemoryLength, parent2.MemoryLength, rng);
            var childWeights = CrossoverMemory(childMemoryLength, parent1.Weights, parent2.Weights, rng);
            return new Individual(childMemoryLength, childMoodThreshold, childWeights);
        }

        private double Crossover(double a, double b, Random? rng = null)
        {
            rng ??= new Random();
            var beta = rng.NextDouble() * (1 + 2 * crossoverD) - crossoverD;
            return a * beta + b * (1 - beta);
        }

        private int Crossover(int a, int b, Random? rng = null)
        {
            return (int)Math.Round(Crossover((double)a, b, rng));
        }

        private double[,,] CrossoverMemory(int memoryLength, double[,,] a, double[,,] b, Random? rng = null)
        {
            rng ??= new Random();
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
