using MathNet.Numerics.Distributions;
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
            public float fitness;
            public ASudokuGrid(int[,] newGrid)
            {
                Grid = newGrid;
                fitness = GridsFitness(newGrid);
            }
            public void UpdateFitness()
            {
                fitness = GridsFitness(Grid);
            }
            //public SetGrid(int[,] newGrid)
            //{

            //}
        }

        static Random rand = new Random();
        static int[,] ProblemSudokuGrid;// = new int[9,9];
        static int PopulationSize = 3800;
        static int MaxGenerations = 1000;
        static int Generation = 0;
        static float Phi = 0;
        static int NumberOfMutations = 0;
        static float Sigma = 1.0f;
        static float MutationRate = 0.07f;
        static float SelectionRate = .9f;
        static float CrossoverRate = 1.0f;
        static int k = 2;
        static bool ReseedEnabled = true;
        static int Stale = 0;
        static int StaleLimit = 15;
        static float PopulationReSeedPercent = 0.85f;
        static List<ASudokuGrid> Population = new List<ASudokuGrid>();
        static List<Tuple<int, int>> AnsweredPosition;
        static List<int> ValidUsableNumber = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        static void Main(string[] args)
        {

            InitProblemGrid();
            AnsweredPosition = GetPositionsToIgnore(ProblemSudokuGrid);

            GenerateIntialPopulations();
            RunEpoch();

            //MateViaCrossoverCycle(Population[0], Population[1]);

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

            int[,] AnswerGrid = new int[9, 9]
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


            //for (int i = 0; i < 8; i++)
            //{
            //    //PrintGrid(GridToString(Population[i].Grid));
            //    Console.WriteLine(GridsFitness(Population[i].Grid));
            //    Console.WriteLine();
            //}

            //SortPopulationByFitness();
            //Console.WriteLine("Sorted?");
            //for (int i = 0; i < 8; i++)
            //{
            //    //PrintGrid(GridToString(Population[i].Grid));
            //    Console.WriteLine(GridsFitness(Population[i].Grid));
            //    Console.WriteLine();
            //}

            //Console.WriteLine(GridsFitness(TestGrid));
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
                { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
                { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
                { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
            };
        }

        static void GenerateIntialPopulations()
        {
            Population.Clear();
            List<int> ValidUsableNumberClone = ValidUsableNumber.ToList();

            //Create random complete grids
            for (int i = 0; i < PopulationSize; ++i)
            {
                //rand = new Random();
                Population.Add(new ASudokuGrid((int[,])ProblemSudokuGrid.Clone()));

                for (int x = 0; x < Population[i].Grid.GetLength(0); ++x)
                {

                    //I need to filter out answered positions from the list of ValidUsableNumbers for every row before I attempt to add valid Usable numbers
                    //As already answered positions should be excluded from the ValidUsableNumbers list.
                    for (int preY = 0; preY < 9; preY++)
                    {
                        if (AnsweredPosition.Exists(pos => pos.Item1 == x && pos.Item2 == preY))
                        {
                            ValidUsableNumberClone.Remove(Population[i].Grid[x, preY]);
                        }
                    }

                    for (int y = 0; y < Population[i].Grid.GetLength(1); ++y)
                    {
                        //If not an answered pos, fill in with a new random number
                        if (!AnsweredPosition.Exists(pos => pos.Item1 == x && pos.Item2 == y))
                        {

                            int newRandNum = ValidUsableNumberClone.OrderBy(n => rand.Next()).Take(1).ToList()[0];
                            while (DoesIntAlreadyExistInRow(Population[i].Grid, x, newRandNum))
                            {
                                ValidUsableNumberClone.Remove(newRandNum);
                                if (ValidUsableNumberClone.Count > 0)
                                    newRandNum = ValidUsableNumberClone.OrderBy(n => rand.Next()).Take(1).ToList()[0];
                            }
                            Population[i].Grid[x, y] = newRandNum;
                        }
                    }
                    //Reset for next Row
                    ValidUsableNumberClone = ValidUsableNumber.ToList(); ;
                }
                if (DoesGridRowsHaveDoubles(Population[i])) 
                {
                    Console.WriteLine("Why");
                }
            }
        }

        static void RegenerateFromIndex(int index)
        {
            List<int> ValidUsableNumberClone = ValidUsableNumber.ToList();
            //Create random complete grids
            for (int i = PopulationSize - index; i < PopulationSize; ++i)
            {
                Population.RemoveAt(i);
                Population.Insert(i, new ASudokuGrid((int[,])ProblemSudokuGrid.Clone()));
                for (int x = 0; x < Population[i].Grid.GetLength(0); ++x)
                {
                    for (int y = 0; y < Population[i].Grid.GetLength(1); ++y)
                    {
                        //If not an answered pos, fill in with a new random number
                        if (!AnsweredPosition.Exists(pos => pos.Item1 == x && pos.Item2 == y))
                        {
                            int newRandNum = ValidUsableNumberClone.OrderBy(n => rand.Next()).Take(1).ToList()[0];
                            while (DoesIntAlreadyExistInRow(Population[i].Grid, x, newRandNum))
                            {
                                ValidUsableNumberClone.Remove(newRandNum);
                                if (ValidUsableNumberClone.Count > 0)
                                    newRandNum = ValidUsableNumberClone.OrderBy(n => rand.Next()).Take(1).ToList()[0];
                            }
                            Population[i].Grid[x, y] = newRandNum;
                        }
                    }
                    //Reset for next Row
                    ValidUsableNumberClone = ValidUsableNumber.ToList(); ;
                }
                Population[i].UpdateFitness();
            }
        }

        static void PrintGrid(List<string> GridAsString)
        {

            GridAsString.ForEach(element =>
            {
                Console.WriteLine(element);
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
            int YCounter = 1;
            int XCounter = 1;

            //May want/need to wrap into a method, this converts a 2D Grid into sub grids for a 9x9 grid.
            for (int SubgridIndex = 1; SubgridIndex <= 9; SubgridIndex++)
            {
                Subgrids.Add(new List<int>());
                for (int x = ((Grid.GetLength(0) / 3) * XCounter) - (Grid.GetLength(0) / 3); x < (Grid.GetLength(0) / 3) * XCounter; ++x)
                {
                    //Y Works well, do something for X now. 
                    for (int y = (Grid.GetLength(1) - (3 * (3 - YCounter))) - 3; y < (Grid.GetLength(1) - (3 * (3 - YCounter))); ++y)
                    {
                        Subgrids[Subgrids.Count - 1].Add(Grid[x,y]);
                    }
                }
                if (SubgridIndex % 3 == 0)
                {
                    YCounter = 1;
                    XCounter++;
                }
                else
                {
                    YCounter++;
                }
            }
            //Count doubles for subgrids
            Subgrids.ForEach(SubGrid =>
            {
                if (SubGrid.Count != SubGrid.Distinct().Count())
                {
                    DuplicateCount = DuplicateCount + SubGrid.Count - SubGrid.Distinct().Count();
                }
            });

            //Count for rows and cols
            for (int x = 0; x < (Grid.GetLength(0)); ++x)
            {
                for (int y = 0; y < (Grid.GetLength(1)); ++y)
                {
                    RowInts.Add(Grid[x, y]);
                    ColInts.Add(Grid[y, x]);
                }

                if (RowInts.Count != RowInts.Distinct().Count())
                {
                    //DuplicateCount = DuplicateCount + RowInts.Count - RowInts.Distinct().Count();
                }
                if (ColInts.Count != ColInts.Distinct().Count())
                {
                    DuplicateCount = DuplicateCount + ColInts.Count - ColInts.Distinct().Count();
                }
                RowInts.Clear();
                ColInts.Clear();
            }

            return DuplicateCount / 144.0f;//216.0f;
        }

        static bool DoesGridRowsHaveDoubles(ASudokuGrid Grid)
        {
            List<int> RowInts = new List<int>();
            for (int x = 0; x < (Grid.Grid.GetLength(0)); ++x)
            {
                for (int y = 0; y < (Grid.Grid.GetLength(1)); ++y)
                {
                    RowInts.Add(Grid.Grid[x, y]);
                }

                if (RowInts.Count != RowInts.Distinct().Count())
                {
                    return true;
                }
                RowInts.Clear();
            }
            return false;
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

        /// <summary>
        /// Probably don't need this after all
        /// </summary>
        /// <param name="Grid"></param>
        /// <returns></returns>
        //static List<int> GetIndexesToIgnore(int[,] Grid)
        //{
        //    List<int> ListOfPositionsToIgnore = new List<int>();

        //    for (int x = 0; x < Grid.GetLength(0); ++x)
        //    {
        //        for (int y = 0; y < Grid.GetLength(1); ++y)
        //        {
        //            if (Grid[x, y] != 0)
        //            {
        //                ListOfPositionsToIgnore.Add();
        //            }
        //        }
        //    }

        //    return ListOfPositionsToIgnore;
        //}

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

        //Bad because it can produce doubles 
        static List<ASudokuGrid> MateViaCrossover(ASudokuGrid Mommy, ASudokuGrid Daddy)
        {
            List<int> MommyGenes = new List<int>();
            List<int> DaddyGenes = new List<int>();
            List<int> NewChildGenes1 = new List<int>();
            List<int> NewChildGenes2 = new List<int>();
            List<ASudokuGrid> Children = new List<ASudokuGrid>();

            int[,] ChildGrid = new int[9, 9];

            MommyGenes = GetGridAsGenes(Mommy.Grid);
            DaddyGenes = GetGridAsGenes(Daddy.Grid);

            if (rand.NextDouble() <= CrossoverRate)
            {
                int RandomCrossOverPoint = rand.Next(0, 80);

                NewChildGenes1.AddRange(MommyGenes.GetRange(0, RandomCrossOverPoint));
                NewChildGenes1.AddRange(DaddyGenes.GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));

                NewChildGenes2.AddRange(DaddyGenes.GetRange(0, RandomCrossOverPoint));
                NewChildGenes2.AddRange(MommyGenes.GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));


                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(NewChildGenes1).Clone()));
                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(NewChildGenes2).Clone()));
            }
            else
            {
                //Asexual reproduction 
                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(MommyGenes).Clone()));
                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(DaddyGenes).Clone()));
            }


            return Children;
        }

        //static List<ASudokuGrid> MateViaCrossoverCycle(ASudokuGrid Mommy, ASudokuGrid Daddy)
        //{
        //    List<int> MommyGenes = new List<int>();
        //    List<int> DaddyGenes = new List<int>();
        //    MommyGenes = GetGridAsGenes(Mommy.Grid);
        //    DaddyGenes = GetGridAsGenes(Daddy.Grid);

        //    List<int> NewChildGenes1 = MommyGenes;
        //    List<int> NewChildGenes2 = DaddyGenes;

        //    List<ASudokuGrid> Children = new List<ASudokuGrid>();

        //    List<int> NewRow1 = new List<int>();
        //    List<int> NewRow2 = new List<int>();

        //    int P1 = 1;
        //    int P2 = 1;
        //    int StopPoint = 0;
        //    for (int i = 0; i < 9; i++)
        //    {
        //        if()
        //        P2 = Mommy.Grid[0, P1 - 1];
        //        NewRow1.Add(P2);
        //        P1 = Daddy.Grid[0, P2 - 1];
        //        NewRow2.Add(P1);
        //        P1 = P2;
        //    }

        //    return Children;
        //}

        static List<ASudokuGrid> MateViaCrossoverCycle(ASudokuGrid Mommy, ASudokuGrid Daddy)
        {
            List<int> MommyGenes = new List<int>();
            List<int> DaddyGenes = new List<int>();
            MommyGenes = GetGridAsGenes(Mommy.Grid);
            DaddyGenes = GetGridAsGenes(Daddy.Grid);

            List<int> NewChildGenes1 = MommyGenes;
            List<int> NewChildGenes2 = DaddyGenes;

            ASudokuGrid Child1 = new ASudokuGrid((int[,])Mommy.Grid.Clone());
            ASudokuGrid Child2 = new ASudokuGrid((int[,])Daddy.Grid.Clone());

            List<int> Child1Row = new List<int>();
            List<int> Child2Row = new List<int>();

            List<ASudokuGrid> Children = new List<ASudokuGrid>();

            if (rand.NextDouble() <= CrossoverRate)
            {
                int CrossoverPoint_1 = rand.Next(0, 8);
                int CrossoverPoint_2 = rand.Next(1, 9);
                //Idea seems to be to pick a point a and b crossover range, ex. 0 - 9 or 4 - 5
                //Can't have them equal that wouldn't be a range. 
                while (CrossoverPoint_1 == CrossoverPoint_2)
                {
                    CrossoverPoint_1 = rand.Next(0, 8);
                    CrossoverPoint_2 = rand.Next(1, 9);
                }
                //need 1 to be the starting point (min in range) so if it is larger than 2 swap. 
                if (CrossoverPoint_1 > CrossoverPoint_2)
                {
                    int temp = CrossoverPoint_1;
                    CrossoverPoint_1 = CrossoverPoint_2;
                    CrossoverPoint_2 = temp;
                }

                for (int i = CrossoverPoint_1; i < CrossoverPoint_2; i++)
                {
                    //Pluck rows out in C# fashion
                    for (int y = 0; y < 9; y++)
                    {
                        Child1Row.Add(Child1.Grid[i, y]);
                        Child2Row.Add(Child2.Grid[i, y]);
                    }
                    if (Child1Row.Count != Child1Row.Distinct().Count() || Child2Row.Count != Child2Row.Distinct().Count())
                    {
                        Console.Write("Why");
                    }
                    //Crossover Rows in Cycle Fashion
                    List<List<int>> RowsCrossed = CrossoverRows(Child1Row, Child2Row);
                    Child1Row = RowsCrossed[0];
                    Child2Row = RowsCrossed[1];
                    //Put crossed rows back in to their slots, make check here to not replace known values from original grid
                    for (int y = 0; y < 9; y++)
                    {
                        Child1.Grid[i, y] = Child1Row[y];
                        Child2.Grid[i, y] = Child2Row[y];
                    }
                    //Rest for next row/attempt
                    Child1Row.Clear();
                    Child2Row.Clear();
                }
            }

            //Return Crossed Children 
            return Children;
        }

        //Thils will not work if rows have doubles, each row should have a unique number for this to work. 
        static List<List<int>> CrossoverRows(List<int> Row1, List<int> Row2)
        {
            List<List<int>> ChilldrenRows = new List<List<int>>();
            List<int> ChildRow1 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<int> ChildRow2 = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            List<int> Remaining = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            int Cycle = 0;

            while (ChildRow1.Exists(val => val == 0) && ChildRow2.Exists(val => val == 0))
            {
                if (Cycle % 2 == 0)
                {
                    int index = FindUnusedIndex(Row1, Remaining);
                    int start = Row1[index];
                    Remaining.Remove(Row1[index]);
                    ChildRow1[index] = Row1[index];
                    ChildRow2[index] = Row2[index];
                    int next = Row2[index];

                    //Cycle should end when the next value we hit is the same as the value we started. 
                    while (next != start)
                    {
                        //If next value exists in row1, return the position/index that value exists at
                        index = FindValue(Row1, next);
                        ChildRow1[index] = Row1[index];
                        Remaining.Remove(Row1[index]);
                        ChildRow2[index] = Row2[index];
                        next = Row2[index];
                    }
                    Cycle++;
                }
                else //Odd Cycle, flip the values to do the reverse
                {
                    int index = FindUnusedIndex(Row1, Remaining);
                    int start = Row1[index];
                    Remaining.Remove(Row1[index]);
                    ChildRow1[index] = Row2[index];
                    ChildRow2[index] = Row1[index];
                    int next = Row2[index];

                    //Cycle should end when the next value we hit is the same as the value we started. 
                    while (next != start)
                    {
                        //If next value exists in row1, return the position/index that value exists at
                        index = FindValue(Row1, next);
                        ChildRow1[index] = Row2[index];
                        Remaining.Remove(Row1[index]);
                        ChildRow2[index] = Row1[index];
                        next = Row2[index];
                    }
                    Cycle++;
                }
            }
            ChilldrenRows.Add(ChildRow1);
            ChilldrenRows.Add(ChildRow2);

            return ChilldrenRows;
        }

        static int FindUnusedIndex(List<int> ParentRow, List<int> remaining)
        {
            for (int i = 0; i < ParentRow.Count; i++)
            {
                if (remaining.Exists(val => val == ParentRow[i]))
                {
                    return i; 
                }
            }
            return 0;
        }

        static int FindValue(List<int> ParentRow, int value)
        {
            for (int i = 0; i < ParentRow.Count; i++)
            {
                if (ParentRow[i] == value)
                {
                    return i;
                }
            }
            return 0; 
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
                BestGrid = new ASudokuGrid((int[,])(Population[i].Grid.Clone()));
                float currentFitness = BestGrid.fitness;//GridsFitness(BestGrid.Grid);
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
            ASudokuGrid WeakestGrid = new ASudokuGrid();// = null;
            float BestGridFitness = 1.0f;
            float WeakestGridFitness = 1.0f;
            for (int i = 0; i < k; i++)
            {
                //Randomly select from the population who is going to compete
                int randIndex = rand.Next(0, Population.Count - 1);
                CurrentGrid = Population[randIndex];
                //TempPopulation.RemoveAt(randIndex);
                float CurrentGridFitness = CurrentGrid.fitness;//GridsFitness(CurrentGrid.Grid);
                if (BestGrid.Grid != null)
                    BestGridFitness = BestGrid.fitness;//GridsFitness(BestGrid.Grid);


                if (BestGrid.Grid == null || CurrentGridFitness < BestGridFitness)
                {
                    if(BestGrid.Grid != null)
                        WeakestGrid = new ASudokuGrid((int[,])BestGrid.Grid.Clone());
                    BestGrid = new ASudokuGrid((int[,])CurrentGrid.Grid.Clone());
                }
                else
                {
                    WeakestGrid = new ASudokuGrid((int[,])CurrentGrid.Grid.Clone());
                    BestGrid = new ASudokuGrid((int[,])BestGrid.Grid.Clone());
                }
            }

            if (rand.NextDouble() < SelectionRate)
            {
                return new ASudokuGrid((int[,])BestGrid.Grid.Clone());
            }
            else
            {
                return new ASudokuGrid((int[,])WeakestGrid.Grid.Clone());
            }
        }

        static List<ASudokuGrid> RouletteWheelSelection()
        {
            return null;
        }

        static int[,] Mutate(ASudokuGrid Grid)
        {
            int[,] MutatedGrid = (int[,])Grid.Grid.Clone();
            List<int> GridGenes = new List<int>();
            GridGenes = GetGridAsGenes(Grid.Grid);
            float OriginalFitness = Grid.fitness;//GridsFitness(Grid);
            bool HasMutated = false;
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
                        //Swapping when we know it is safe, instead of just randomly switching or always swapping has helped the GA make progress significantly, as this helps prevent back tracking. 
                        if (rand.NextDouble() <= MutationRate)
                        {
                            //Mutate randomly
                            int RandRowA = rand.Next(0, 8);
                            int RandColA = rand.Next(0, 8);
                            int RandRowB = rand.Next(0, 8);
                            int RandColB = rand.Next(0, 8);
                            //make sure we dont' swap an answered spot
                            while (AnsweredPosition.Exists(pos => pos.Item1 == RandRowA && pos.Item2 == RandColA) || AnsweredPosition.Exists(pos => pos.Item1 == RandRowB && pos.Item2 == RandColB))
                            {
                                //Keep trying to get a row aand col that doesn't match an existing spot
                                RandRowA = rand.Next(0, 8);
                                RandColA = rand.Next(0, 8);

                                RandRowB = rand.Next(0, 8);
                                RandColB = rand.Next(0, 8);
                            }
                            if (RandRowA == 0 && RandColA == 0)
                            {
                                Console.WriteLine("Failed");
                            }
                            //Check if new value already exists in row.
                            if (!DoesIntAlreadyExistInRow(MutatedGrid, RandRowA, Grid.Grid[RandRowB, RandColB]) && !DoesIntAlreadyExistInRow(MutatedGrid, RandRowB, Grid.Grid[RandRowA, RandColA]))
                            {
                                //Safe to swap so swap Values at random spots. 
                                int AValue = MutatedGrid[RandRowA, RandColA];
                                MutatedGrid[RandRowA, RandColA] = MutatedGrid[RandRowB, RandColB];
                                MutatedGrid[RandRowB, RandColB] = AValue;
                                HasMutated = true;
                                NumberOfMutations++;
                            }                         
                        }
                        if (DoesGridRowsHaveDoubles(new ASudokuGrid(MutatedGrid))) ;
                        {
                            Console.WriteLine("Why?");
                        }
                    }
                }
            }

            if (GridsFitness(MutatedGrid) > OriginalFitness && HasMutated)
            {
                Phi = Phi + 1;
            }

            return MutatedGrid;
        }

        static void CalculateDynamicMutationRate()
        {
            if (NumberOfMutations == 0)
            {
                Phi = 0;
            }
            else
            {
                Phi = Phi / NumberOfMutations;
            }

            if (Phi > 2.0f)
            {
                Sigma = Sigma / 0.998f;
            }
            else if (Phi < 2.0f)
            {
                Sigma = Sigma * 0.998f;
            }

            Normal normalDist = new Normal(0.0f, Sigma);

            MutationRate = Math.Abs((float)normalDist.Sample());
        }

        static bool DoesIntAlreadyExistInRow(int[,] Grid, int Row, int ValueToCheck)
        {
            for (int y = 0; y < 9; y++)
            {
                if (Grid[Row, y] == ValueToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        static void SortPopulationByFitness()
        {
            //Population.OrderByDescending(SudokuGrid => GridsFitness(SudokuGrid.Grid));
            Population.Sort((SudokuGridA, SudokuGridB) => SudokuGridA.fitness.CompareTo(SudokuGridB.fitness));
            //Population.OrderBy(Grid => Guid.NewGuid());
        }

        static void RunEpoch()
        {
            for (int g = 0; g < MaxGenerations; g++)
            {
                List<ASudokuGrid> NewPopulation = new List<ASudokuGrid>();

                //Mate to New Populataion
                for (int i = 0; i < PopulationSize; i++)
                {
                    //Apply Selection Pressures, by choosing the best random mommy and daddy 
                    List<ASudokuGrid> NewChilldren = MateViaCrossoverCycle(new ASudokuGrid((int[,])TournamentSelection().Grid.Clone()), new ASudokuGrid((int[,])TournamentSelection().Grid.Clone()));
                    NewChilldren.ForEach(child => 
                    {
                        Population.Add(new ASudokuGrid((int[,])Mutate(child).Clone()));
                    });
                }

                //CalculateDynamicMutationRate();

                //Elitism
                //Try Adding to population, sorting then, pruning out the worst instead of just swapping out new generation with old, although both seem to get stuck in the same place at different rates. 
                SortPopulationByFitness();
                Population.RemoveRange(PopulationSize - 1, Population.Count - PopulationSize);

                //Old Original Population is now the new Population, rinse and repeat. 
                //Population = NewPopulation;

                if (ReseedEnabled)
                {
                    if (Population[0].fitness != Population[1].fitness)
                    {
                        Stale = 0;
                    }
                    else
                    {
                        Stale += 1;
                    }

                    if (Stale >= StaleLimit)
                    {
                        //This may not be a very good solution, seems useless probably gets pruned since none of the will be good? 
                        RegenerateFromIndex((int)(PopulationSize * PopulationReSeedPercent));
                        Console.WriteLine("Went Stale, Reseeding");
                        Stale = 0;
                        NumberOfMutations = 0;
                        Sigma = 1;
                        MutationRate = 0.07f;
                    }
                }

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
