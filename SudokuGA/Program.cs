using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuGA
{
    public class Program
    {
        struct ASudokuGrid
        {
            public int[,] Grid;
            public ASudokuGrid(int[,] newGrid)
            {
                Grid = newGrid;
            }
            //public SetGrid(int[,] newGrid)
            //{

            //}
        }

        static Random rand = new Random();
        static int[,] ProblemSudokuGrid;// = new int[9,9];
        static int PopulationSize = 1000;
        static int MaxGenerations = 10000;
        static int Generation = 0;
        static float MutationRate = 0.1f;
        static float SelectionRate = 1.0f;
        static int k = 150;
        static List<ASudokuGrid> Population = new List<ASudokuGrid>();
        static List<Tuple<int, int>> AnsweredPosition;
    static void Main(string[] args)
        {

            InitProblemGrid();
            AnsweredPosition = GetPositionsToIgnore(ProblemSudokuGrid);

            GenerateIntialPopulations();

            RunEpoch();

            int[,] TestGrid = new int[9, 9]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            int[,] TestGrid2 = new int[9, 9]
            {
                { 5, 3, 4, 6, 7, 8, 9, 1, 2 },
                { 6, 7, 2, 1, 9, 5, 3, 4, 8 },
                { 1, 9, 8, 3, 4, 2, 5, 6, 7 },
                { 8, 5, 9, 7, 6, 1, 4, 2, 3 },
                { 4, 2, 6, 8, 5, 3, 7, 9, 1 },
                { 7, 1, 3, 9, 2, 4, 8, 5, 6 },
                { 9, 6, 1, 5, 3, 7, 2, 8, 4 },
                { 2, 8, 7, 4, 1, 9, 6, 3, 5 },
                { 3, 4, 5, 2, 8, 6, 1, 7, 9 }
            };
        

            Console.WriteLine(GridsFitness(TestGrid2));
            //int randomGrid1 = rand.Next(0, PopulationSize);
            //int randomGrid2 = rand.Next(0, PopulationSize);

            //PrintGrid(GridToString(Population[randomGrid1].Grid));
            //Console.WriteLine(GridsFitness(Population[randomGrid2].Grid));
            //PrintGrid(GridToString(Population[9].Grid));
            //Console.WriteLine(GridsFitness(Population[9].Grid));

            ////PrintFitnessForEntirePopulationBetweenRange(Population, 0.0f, 0.7f);
            //PrintFitnessForEntirePopulation(Population);
            //int[,] newChild = MateViaCrossover(Population[randomGrid1].Grid, Population[randomGrid2].Grid);
            //PrintGrid(GridToString(newChild));
            //Console.WriteLine(GridsFitness(newChild));

            Console.ReadKey();
        }

        public static void InitProblemGrid()
        {
            ProblemSudokuGrid = new int[9, 9]
            {
                { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
                { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
                { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
                { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
                { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
                { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
                { 0, 6, 0, 6, 0, 0, 2, 8, 0 },
                { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
                { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
            };
        }

        static void GenerateIntialPopulations()
        {
            //Create random complete grids
            for (int i = 0; i < PopulationSize; ++i)
            {
                //rand = new Random();
                Population.Add(new ASudokuGrid((int[,])ProblemSudokuGrid.Clone()));

                for (int x = 0; x < Population[i].Grid.GetLength(0); ++x)
                {
                    for (int y = 0; y < Population[i].Grid.GetLength(1); ++y)
                    {
                        if (Population[i].Grid[x, y] == 0)
                        {
                            int newRandNum = rand.Next(1, 9);
                            //For some reason this is setting it to the ProblemGrid as well.
                            Population[i].Grid[x, y] = newRandNum;
                        }
                    }
                }
            }
        }

        static void PrintGrid(List<string> Row)
        {

            Row.ForEach(row =>
            {
                Console.WriteLine(row);
            });
        }

        static List<string> GridToString(int[,] Grid)
        {
            List<string> Row = new List<string>(new string[9]);
            int index = 0;
            for (int x = 0; x < Grid.GetLength(0); ++x)
            {
                for (int y = 0; y < Grid.GetLength(1); ++y)
                {
                    Row[index] += Grid[x, y].ToString() + "|";
                }
                index++;
            }
            return Row;
        }

        static float GridsFitness(int[,] Grid)
        {
            //Seems to count two rows at a time? 
            List<int> RowInts = new List<int>();
            List<int> ColInts = new List<int>();
            List<List<int>> Subgrids = new List<List<int>>();
            float DuplicateCount = 0.0f;

            for (int x = 0; x < Grid.GetLength(0); ++x)
            {
                for (int y = 0; y < Grid.GetLength(1); ++y)
                {
                    RowInts.Add(Grid[x,y]);
                    ColInts.Add(Grid[y, x]);
                    //Doesn't work as I thought it should
                    //if (x % 3 == 0 && y % 3 == 0)
                    //{
                    //    Subgrids.Add(new List<int>());
                    //}
                    //else
                    //{
                    //    Subgrids[Subgrids.Count - 1].Add(Grid[y, x]);
                    //}
                }

                if (RowInts.Count != RowInts.Distinct().Count())
                {
                    DuplicateCount = DuplicateCount + RowInts.Count - RowInts.Distinct().Count();
                }
                if (ColInts.Count != ColInts.Distinct().Count())
                {
                    DuplicateCount = DuplicateCount + ColInts.Count - ColInts.Distinct().Count();
                }
                //Doesn't work as I thought it should
                //Subgrids.ForEach(SubGrid =>
                //{
                //    if (SubGrid.Count != SubGrid.Distinct().Count())
                //    {
                //        DuplicateCount = DuplicateCount + ColInts.Count - ColInts.Distinct().Count();
                //    }
                //});
                RowInts.Clear();
                ColInts.Clear();

            }

            return DuplicateCount / 144.0f;
        }

        static void PrintFitnessForEntirePopulation(List<ASudokuGrid> Population)
        {
            Population.ForEach(SudokuGrid =>
            {
                Console.WriteLine(GridsFitness(SudokuGrid.Grid));
            });
        }

        /// <summary>
        /// Prints only those whos fitness lies in a certain range
        /// </summary>
        /// <param name="Population"></param>
        static void PrintFitnessForEntirePopulationBetweenRange(List<ASudokuGrid> Population, float min, float max)
        {
            Population.ForEach(SudokuGrid =>
            {
                float fitness = GridsFitness(SudokuGrid.Grid);
                if (fitness >= min && fitness <= max)
                {
                    Console.WriteLine(fitness);
                }
            });
        }

        /// <summary>
        /// Gets the x y coord of already answered positions so we can ignore them when we have to/can. 
        /// </summary>
        /// <param name="Grid"></param>
        /// <returns></returns>
        static List<Tuple<int, int>> GetPositionsToIgnore(int[,] Grid)
        {
            List<Tuple<int, int>> ListOfPositionsToIgnore = new List<Tuple<int, int>>();

            for (int x = 0; x < Grid.GetLength(0); ++x)
            {
                for (int y = 0; y < Grid.GetLength(1); ++y)
                {
                    if (Grid[x, y] != 0)
                    {
                        ListOfPositionsToIgnore.Add(new Tuple<int, int>(x, y));
                    }
                }
            }

            return ListOfPositionsToIgnore;
        }

        static List<int> GetGridAsGenes(int[,] Parent)
        {
            List<int> Genes = new List<int>();

            for (int x = 0; x < Parent.GetLength(0); ++x)
            {
                for (int y = 0; y < Parent.GetLength(1); ++y)
                {
                    Genes.Add(Parent[x, y]);
                }
            }

            return Genes;
        }

        static List<int[,]> MateViaCrossover(int[,] Mommy, int[,] Daddy)
        {
            List<int> MommyGenes = new List<int>();
            List<int> DaddyGenes = new List<int>();
            List<int> NewChildGenes1 = new List<int>();
            List<int> NewChildGenes2 = new List<int>();
            List<int[,]> Children = new List<int[,]>();

            int[,] ChildGrid = new int[9, 9];

            MommyGenes = GetGridAsGenes(Mommy);
            DaddyGenes = GetGridAsGenes(Daddy);

            int RandomCrossOverPoint = rand.Next(0, 80);

            NewChildGenes1.AddRange(MommyGenes.GetRange(0, RandomCrossOverPoint));
            NewChildGenes1.AddRange(DaddyGenes.GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));

            NewChildGenes2.AddRange(DaddyGenes.GetRange(0, RandomCrossOverPoint));
            NewChildGenes2.AddRange(MommyGenes.GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));


            Children.Add(ConvertRawGenesToGrid(NewChildGenes1));
            Children.Add(ConvertRawGenesToGrid(NewChildGenes2));

            return Children;
        }

        static int[,] ConvertRawGenesToGrid(List<int> Genes)
        {
            int[,] Grid = new int[9,9];
            for (int i = 0; i < Genes.Count; i++)
            {
                Grid[i / 9, i % 9] = Genes[i];
            }
            return Grid; 
        }

        static float BestFitnessInPopulation(bool DisplayGrid)
        {
            float bestFitness = 1.0f;
            ASudokuGrid BestGrid = new ASudokuGrid();
            for (int i = 0; i < Population.Count; i++)
            {
                BestGrid = new ASudokuGrid((Population[i].Grid));
                float currentFitness = GridsFitness(BestGrid.Grid);
                if (currentFitness < bestFitness)
                {
                    bestFitness = currentFitness;
                }
            }

            if (DisplayGrid)
            {
                PrintGrid(GridToString(BestGrid.Grid));
            }

            return bestFitness;
        }

        static ASudokuGrid TournamentSelection()
        {
            //List<ASudokuGrid> TempPopulation = new List<ASudokuGrid>(Population);
            //List<ASudokuGrid> FilteredPopulation = new List<ASudokuGrid>();
            ASudokuGrid BestGrid = new ASudokuGrid();// = null;// = new int[9,9];
            ASudokuGrid CurrentGrid = new ASudokuGrid();// = null;
            float BestGridFitness = 1.0f;

            for (int i = 0; i < k; i++)
            {
                //Randomly select from the population who is going to compete
                int randIndex = rand.Next(0, PopulationSize);
                CurrentGrid = Population[randIndex];
                //TempPopulation.RemoveAt(randIndex);
                float CurrentGridFitness = GridsFitness(CurrentGrid.Grid);
                if (BestGrid.Grid != null)
                    BestGridFitness = GridsFitness(BestGrid.Grid);
                if (BestGrid.Grid == null || CurrentGridFitness < BestGridFitness)
                {
                    BestGrid = new ASudokuGrid((int[,])CurrentGrid.Grid.Clone());
                }
            }

            return new ASudokuGrid((int[,])BestGrid.Grid.Clone());
        }

        static List<ASudokuGrid> RouletteWheelSelection()
        {
            return null;
        }

        static int[,] Mutate(int[,] Grid)
        {
            int[,] MutatedGrid = new int[9,9];
            List<int> GridGenes = new List<int>();
            GridGenes = GetGridAsGenes(Grid);

            for (int i = 0; i < GridGenes.Count; i++)
            {
                for (int a = 0; a < AnsweredPosition.Count; a++)
                {
                    if ((AnsweredPosition[a].Item1 == (i / 9) && AnsweredPosition[a].Item2 == (i % 9)))
                    {
                        MutatedGrid[AnsweredPosition[a].Item1, AnsweredPosition[a].Item2] = GridGenes[i];
                        break;
                    }
                    else
                    {
                        if (rand.NextDouble() < MutationRate)
                        {
                            //Mutate randomly
                            MutatedGrid[i / 9, i % 9] = rand.Next(1, 9);
                        }
                    }
                }
            }

            //for (int i = 0; i < GridGenes.Count; i++)
            //{
            //    AnsweredPosition.ForEach(pos =>
            //    {

            //    if ((pos.Item1 == (i / 9) && pos.Item2 == (i % 9)))
            //    {
            //        MutatedGrid[pos.Item1, pos.Item2] = GridGenes[i];
            //            break;
            //    }
            //    else
            //    {
            //        if (rand.NextDouble() < MutationRate)
            //        {
            //            //Mutate randomly
            //            MutatedGrid[i / 9, i % 9] = rand.Next(1, 9);
            //        }
            //    }

            //    });
            //}


            return MutatedGrid;
        }

        static void RunEpoch()
        {
            for (int g = 0; g < MaxGenerations; g++)
            {
                List<ASudokuGrid> NewPopulation = new List<ASudokuGrid>();
                //Apply Selection Pressures

                //Mate to New Populataion
                for (int i = 0; i < PopulationSize/2; i++)
                {
                    List<int[,]> NewChilldren = MateViaCrossover(TournamentSelection().Grid, TournamentSelection().Grid);
                    NewChilldren.ForEach(child => 
                    {
                        NewPopulation.Add(new ASudokuGrid(child));
                    });
                }

                //Mutate Children
                for (int m = 0; m < NewPopulation.Count; m++)
                {
                    NewPopulation[m] = new ASudokuGrid(Mutate(NewPopulation[m].Grid));
                }

                //Old Original Populatio is now the new Population, rinse and repeat. 
                Population = NewPopulation;
                Generation++;
                float bestFitness = BestFitnessInPopulation(false);

                if (bestFitness == 0)
                {
                    Console.WriteLine("Current Generation " + Generation + " Solution Foumd! ");
                    BestFitnessInPopulation(true);
                    break;
                }
                else
                {
                    Console.WriteLine("Current Generation " + Generation + " best fitness " + bestFitness);
                }
            }
        }
    }
}
