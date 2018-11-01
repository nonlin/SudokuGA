using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuGA
{
    public class ASudokuGrid
    {
        public int[,] Grid;
        public float fitness;

        public ASudokuGrid()
        {

        }

        public ASudokuGrid(int[,] newGrid)
        {
            Grid = newGrid;
            fitness = Program.GridsFitness(newGrid);
        }
        public void UpdateFitness()
        {
            fitness = Program.GridsFitness(Grid);
        }
    }

    public class Program
    {


        static Random rand = new Random();
        static int[,] ProblemSudokuGrid;// = new int[9,9];
        static ASudokuGrid ProblemGrid;
        static int PopulationSize = 500;
        static int ElitesSize = (int)(0.05f * PopulationSize);
        static int MaxGenerations = 1000;
        static int Generation = 0;
        static float Phi = 0;
        static int NumberOfMutations = 0;
        static float Sigma = 1.0f;
        static float MutationRate = 0.07f;
        static float SelectionRate = .85f;
        static float CrossoverRate = 1.0f;
        static int k = 2;
        static bool ReseedEnabled = true;
        static int Stale = 0;
        static int StaleLimit = 3;
        static float PopulationReSeedPercent = 0.95f;
        static List<ASudokuGrid> Population = new List<ASudokuGrid>();
        static List<Tuple<int, int>> AnsweredPosition;
        static List<int> ValidUsableNumber = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
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

            //Test Check Methods
            //Console.WriteLine(WillWeInsertduplicateForSubgid(new ASudokuGrid(AnswerGrid), 1, 1, 8));
            //Console.WriteLine(DoesColContainDuplicates(new ASudokuGrid(AnswerGrid), 2));
            //Console.WriteLine(GridsFitness(AnswerGrid));
            
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
            ProblemGrid = new ASudokuGrid(ProblemSudokuGrid);
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
                            while (WillWeInsertDuplicateForRow(Population[i].Grid, x, newRandNum))
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
                Population[i].UpdateFitness();
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
                            while (WillWeInsertDuplicateForRow(Population[i].Grid, x, newRandNum))
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

        static public float GridsFitness(int[,] Grid)
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

        #region Helpers

        static bool WillWeInsertduplicateForSubgid(ASudokuGrid Grid, int row, int col, int value)
        {
            int i = 3 * (int)(row / 3);
            int j = 3 * (int)(col / 3);

            if (Grid.Grid[i, j] == value
           || (Grid.Grid[i, j + 1] == value)
           || (Grid.Grid[i, j + 2] == value)
           || (Grid.Grid[i + 1, j] == value)
           || (Grid.Grid[i + 1, j + 1] == value)
           || (Grid.Grid[i + 1, j + 2] == value)
           || (Grid.Grid[i + 2, j] == value)
           || (Grid.Grid[i + 2, j + 1] == value)
           || (Grid.Grid[i + 2, j + 2] == value))
            {
                return true;
            }

            return false;
        }

        static bool WillWeInsertDuplicateForCol(ASudokuGrid Grid, int ColToCheck, int value)
        {
            List<int> ColInts = new List<int>();
            for (int x = 0; x < (Grid.Grid.GetLength(0)); ++x)
            {
                if (Grid.Grid[x, ColToCheck] == value)
                {
                    return true;
                }
            }

            return false;
        }

        static bool WillWeInsertDuplicateForRow(int[,] Grid, int Row, int ValueToCheck)
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

        static bool DoesSubgridsHaveDuplicates(ASudokuGrid Grid)
        {
            List<List<int>> Subgrids = new List<List<int>>();
            int YCounter = 1;
            int XCounter = 1;
            bool ContainsDuplicate = false;
            //May want/need to wrap into a method, this converts a 2D Grid into sub grids for a 9x9 grid.
            for (int SubgridIndex = 1; SubgridIndex <= 9; SubgridIndex++)
            {
                Subgrids.Add(new List<int>());
                for (int x = ((Grid.Grid.GetLength(0) / 3) * XCounter) - (Grid.Grid.GetLength(0) / 3); x < (Grid.Grid.GetLength(0) / 3) * XCounter; ++x)
                {
                    //Y Works well, do something for X now. 
                    for (int y = (Grid.Grid.GetLength(1) - (3 * (3 - YCounter))) - 3; y < (Grid.Grid.GetLength(1) - (3 * (3 - YCounter))); ++y)
                    {
                        Subgrids[Subgrids.Count - 1].Add(Grid.Grid[x, y]);
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
                    ContainsDuplicate = true;
                }
            });

            return ContainsDuplicate;
        }

        static bool DoesGridColHaveDuplicates(ASudokuGrid Grid)
        {
            List<int> ColInts = new List<int>();
            //Count for rows and cols
            for (int x = 0; x < (Grid.Grid.GetLength(0)); ++x)
            {
                for (int y = 0; y < (Grid.Grid.GetLength(1)); ++y)
                {
                    ColInts.Add(Grid.Grid[y, x]);
                }
                if (ColInts.Count != ColInts.Distinct().Count())
                {
                    return true;
                }
                ColInts.Clear();
            }

            return false;
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
        #endregion Helper

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

            MommyGenes = GetGridAsGenes((int[,])Mommy.Grid.Clone());
            DaddyGenes = GetGridAsGenes((int[,])Daddy.Grid.Clone());

            if (rand.NextDouble() <= CrossoverRate)
            {
                int RandomCrossOverPoint = rand.Next(0, 80);

                NewChildGenes1.AddRange(MommyGenes.ToList().GetRange(0, RandomCrossOverPoint));
                NewChildGenes1.AddRange(DaddyGenes.ToList().GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));

                NewChildGenes2.AddRange(DaddyGenes.ToList().GetRange(0, RandomCrossOverPoint));
                NewChildGenes2.AddRange(MommyGenes.ToList().GetRange(RandomCrossOverPoint, ((DaddyGenes.Count) - RandomCrossOverPoint)));


                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(NewChildGenes1).Clone()));
                Children.Add(new ASudokuGrid((int[,])ConvertRawGenesToGrid(NewChildGenes2).Clone()));
            }
            else
            {
                //Asexual reproduction 
                Children.Add(new ASudokuGrid((int[,])Mommy.Grid.Clone()));
                Children.Add(new ASudokuGrid((int[,])Daddy.Grid.Clone()));
            }

            if (DoesGridRowsHaveDoubles(Children[0]) || DoesGridRowsHaveDoubles(Children[1]))
            {
                Children.Clear();
                //Asexual reproduction 
                Children.Add(new ASudokuGrid((int[,])Mommy.Grid.Clone()));
                Children.Add(new ASudokuGrid((int[,])Daddy.Grid.Clone()));
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
                    //Reset for next row/attempt
                    Child1Row.Clear();
                    Child2Row.Clear();
                    //Pluck rows out in C# fashion
                    for (int y = 0; y < 9; y++)
                    {
                        Child1Row.Add(Child1.Grid[i, y]);
                        Child2Row.Add(Child2.Grid[i, y]);
                    }
                    if (Child1Row.Count != Child1Row.Distinct().Count() || Child2Row.Count != Child2Row.Distinct().Count())
                    {
                        //Console.Write("Why");
                    }
                    //Crossover Rows in Cycle Fashion
                    List<List<int>> RowsCrossed = CrossoverRows(Child1Row, Child2Row);
                    //Child1Row = RowsCrossed[0];
                    //Child2Row = RowsCrossed[1];
                    //Put crossed rows back in to their slots, make check here to not replace known values from original grid
                    for (int y = 0; y < 9; y++)
                    {
                        if (!AnsweredPosition.Exists(pos => pos.Item1 == i && pos.Item2 == y))
                        {
                            Child1.Grid[i, y] = RowsCrossed[0][y];
                            Child2.Grid[i, y] = RowsCrossed[1][y];
                        }
                    }
                }
            }
            //Children[0].UpdateFitness();
            //Children[1].UpdateFitness();
            Children.Add(Child1);
            Children.Add(Child2);
            //Return Crossed Children 
            return Children;
        }

        //Thils will not work if rows have doubles, each row should have a unique number for this to work. Otherwise it will get stuck trying to swap values. 
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
            ASudokuGrid MutatedGrid = new ASudokuGrid((int[,])Grid.Grid.Clone());

            List<int> GridGenes = new List<int>();
            GridGenes = GetGridAsGenes(Grid.Grid);
            float OriginalFitness = Grid.fitness;//GridsFitness(Grid);
            bool HasMutated = false;

            //Swapping when we know it is safe, instead of just randomly switching or always swapping has helped the GA make progress significantly, as this helps prevent back tracking. 
            if (rand.NextDouble() <= MutationRate)
            {
                while (!HasMutated)
                {
                    //Mutate randomly
                    int RandRowA = rand.Next(0, 9);
                    int RandColA = rand.Next(0, 9);
                    int RandRowB = rand.Next(0, 9);
                    int RandColB = rand.Next(0, 9);

                    while (RandColA == RandColB)
                    {
                        RandColA = rand.Next(0, 9);
                        RandColB = rand.Next(0, 9);
                    }

                    //This is very important, since we ensure all made grids to start have no doubles, it won't be possible to swap items between two randomr rows, so instead do it in the same row at two random cols
                    RandRowB = RandRowA;
                    //Check if new value already exists in row.     
                    if (ProblemGrid.Grid[RandRowA, RandColA] == 0 && ProblemGrid.Grid[RandRowB, RandColB] == 0)
                    {
                        if ((!WillWeInsertDuplicateForCol(ProblemGrid, RandColA, MutatedGrid.Grid[RandRowB, RandColB]) && !WillWeInsertduplicateForSubgid(ProblemGrid, RandRowA, RandColA, MutatedGrid.Grid[RandRowB, RandColB])) &&
                            (!WillWeInsertDuplicateForCol(ProblemGrid, RandColB, MutatedGrid.Grid[RandRowA, RandColA]) && !WillWeInsertduplicateForSubgid(ProblemGrid, RandRowB, RandColB, MutatedGrid.Grid[RandRowA, RandColA])))// &&
                                                                                                                                                                                                                                  //(!WillWeInsertDuplicateForRow(MutatedGrid.Grid, RandRowA, Grid.Grid[RandRowB, RandColB]) && !WillWeInsertDuplicateForRow(MutatedGrid.Grid, RandRowB, Grid.Grid[RandRowA, RandColA])))
                        {
                            //Safe to swap so swap Values at random spots. 
                            int AValue = MutatedGrid.Grid[RandRowA, RandColA];
                            MutatedGrid.Grid[RandRowA, RandColA] = MutatedGrid.Grid[RandRowB, RandColB];
                            MutatedGrid.Grid[RandRowB, RandColB] = AValue;
                            HasMutated = true;
                            NumberOfMutations++;
                        }
                    }
                }
            }

            if (DoesGridRowsHaveDoubles(MutatedGrid))
            {
                Console.WriteLine("Why?");
            }

            if (MutatedGrid.fitness > OriginalFitness && HasMutated)
            {
                Phi = Phi + 1;
            }

            return MutatedGrid.Grid;
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

                List<ASudokuGrid> NextPopulation = new List<ASudokuGrid>();
                List<ASudokuGrid> Elites = new List<ASudokuGrid>();
                SortPopulationByFitness();
                //Get best in population before Mating and Mutating
                for (int e = 0; e < ElitesSize; e++)
                {
                    Elites.Add(new ASudokuGrid((int[,])Population[e].Grid.Clone()));
                }

                //Mate to New Populataion considering the best for 
                for (int i = ElitesSize; i < PopulationSize; i = i + 2)
                {
                    //Apply Selection Pressures, by choosing the best random mommy and daddy 
                    List<ASudokuGrid> NewChilldren = MateViaCrossoverCycle(new ASudokuGrid((int[,])TournamentSelection().Grid.Clone()), new ASudokuGrid((int[,])TournamentSelection().Grid.Clone()));
                    NewChilldren.ForEach(child => 
                    {
                        NextPopulation.Add(new ASudokuGrid((int[,])Mutate(child).Clone()));
                    });
                }

                CalculateDynamicMutationRate();

                NextPopulation.AddRange(Elites);

                Population = NextPopulation;

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
                float bestFitness = BestFitnessInPopulation(true);

                if (bestFitness == 0)
                {
                    Console.WriteLine("Current Generation " + Generation + " Solution Found! ");
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
