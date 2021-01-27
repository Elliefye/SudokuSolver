using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverWPF
{
    public class Square
    {
        public static int sudokuSize;
        public List<int> potentialValues;
        public int row, column, block, currentValue = 0;
        public bool inputSquare = false;
        //BLOCKS: 0 though 3/8/15 (left to right, top to bottom)
        //COLUMNS AND ROWS: 0 through 3/8/15 (top to bottom and left to right)

        public int getBlock(int rowId, int columnId)
        {
            int i = 0, j = 0, increment = (int)Math.Sqrt(sudokuSize), blockId = 0;

            while (i <= sudokuSize)
            {
                if (rowId < (increment + i))
                {
                    while (j <= sudokuSize)
                    {
                        if (columnId < (increment + j))
                        {
                            return blockId;
                        }
                        else
                        {
                            j += increment;
                            blockId++;
                        }
                    }
                }
                else
                {
                    i += increment;
                    blockId += increment;
                }
            }           
            return -1;
        }

        public Square(int r, int c) //initiation
        {
            row = r;
            column = c;
            block = getBlock(r, c);

            potentialValues = new List<int>(sudokuSize);
            resetPotentialValues(this);
        }

        public static void resetPotentialValues(Square square)
        {
            square.potentialValues.Clear();
            for (int i = 1; i <= sudokuSize; i++)
                square.potentialValues.Add(i);
        }

        public Square cloneSquare()
        {
            Square copy = new Square(this.row, this.column);
            copy.currentValue = this.currentValue;
            copy.potentialValues.Clear();

            foreach (int number in this.potentialValues)
                copy.potentialValues.Add(number);

            return copy;
        }
    }
}
