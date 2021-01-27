using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverWPF
{
    public class Board
    {
        public List<Square> currentBoard;
        public Stack<List<Square>> boards;
        public Stack<Square> backtrackSquares;
        public MainWindow main;
        int undoCount = 0;

        public Board() //initiation
        {
            currentBoard = new List<Square>();
            boards = new Stack<List<Square>>();
            backtrackSquares = new Stack<Square>();
            int size = Square.sudokuSize;

            for (int row = 0; row < size; row++)
                for (int column = 0; column < size; column++)
                    currentBoard.Add(new Square(row, column)); //initiates all the squares in the board
        }

        public Square returnCurrent(int currentRow, int currentColumn)
        {
            return currentBoard.Find(x => (x.row == currentRow) && (x.column == currentColumn));
        }

        bool validate(Square square, int value)
        {
            Square[] currentRow = currentBoard.FindAll(x => x.row == square.row).ToArray();
            Square[] currentColumn = currentBoard.FindAll(x => x.column == square.column).ToArray();
            Square[] currentBlock = currentBoard.FindAll(x => x.block == x.getBlock(square.row, square.column)).ToArray();

            if (currentRow.FirstOrDefault(x => x.currentValue == value) != null)
                return false;
            else if (currentColumn.FirstOrDefault(x => x.currentValue == value) != null)
                return false;
            else if (currentBlock.FirstOrDefault(x => x.currentValue == value) != null)
                return false;
            return true;
        }

        public List<Square> cloneBoard()
        {
            List<Square> copy = new List<Square>();
            foreach (Square square in currentBoard)
            {
                if (square.inputSquare)
                    copy.Add(square);
                else if(backtrackSquares.Contains(square))
                    copy.Add(square.cloneSquare());
                else
                {
                    Square temp = square.cloneSquare();
                    if(temp.potentialValues.Count == 0)
                        Square.resetPotentialValues(temp);
                    copy.Add(temp);
                }
            }
                
            return copy;
        }

        public void addSquareValue(int row, int column, int value)
        {
            Square current = returnCurrent(row, column);
            if (!validate(current, value))
            {
                if (current.inputSquare)
                {
                    main.showError();
                    main.stop = true;
                    return;
                }
                current.potentialValues.Remove(value);
                trySmartSolving();
                return;
            }

            current.currentValue = value;
            current.potentialValues.Remove(value);

            Square[] currentRow = currentBoard.FindAll(x => x.row == row).ToArray();
            foreach (Square currentRowSquare in currentRow)
                if ((currentRowSquare != current) && (currentRowSquare.potentialValues.Contains(value))
                     && (currentRowSquare.currentValue == 0))
                {
                    currentRowSquare.potentialValues.Remove(value);
                }

            Square[] currentColumn = currentBoard.FindAll(x => x.column == column).ToArray();
            foreach (Square currentColumnSquare in currentColumn)
                if ((currentColumnSquare != current) && currentColumnSquare.potentialValues.Contains(value)
                      && (currentColumnSquare.currentValue == 0))
                {
                    currentColumnSquare.potentialValues.Remove(value);
                }

            Square[] currentBlock = currentBoard.FindAll(x => x.block == x.getBlock(row, column)).ToArray();
            foreach (Square currentBlockSquare in currentBlock)
                if ((currentBlockSquare != current) && currentBlockSquare.potentialValues.Contains(value)
                     && (currentBlockSquare.currentValue == 0))
                {
                    currentBlockSquare.potentialValues.Remove(value);
                }

            if (currentBoard.FindAll(x => x.currentValue == 0).Count == 0) //if all squares have values
            {
                main.showSolved();
                return;
            }
            trySmartSolving();
        }

        public void trySmartSolving()
        {
            if (!main.allValuesAdded) //does nothing until input is read
                return;
            else
            {
                Square newSquare = currentBoard.FirstOrDefault(x => x.potentialValues.Count == 0 && x.currentValue == 0);

                if (newSquare != null) //unsolvable
                {
                    undo();
                    return;
                }
                    
                newSquare = currentBoard.FirstOrDefault(x => x.potentialValues.Count == 1 && x.currentValue == 0);

                 if (newSquare != null) //found a square with 1 possible value
                {
                    addSquareValue(newSquare.row, newSquare.column, newSquare.potentialValues.ElementAt(0));
                    return;
                }

                for (int i = 2; i <= Square.sudokuSize; i++) //finds a square to guess the value of
                {
                    newSquare = currentBoard.FirstOrDefault(x => x.potentialValues.Count == i && x.currentValue == 0);
                    if (newSquare != null)
                    {
                        boards.Push(cloneBoard()); //board with backtrack square still empty
                        backtrackSquares.Push(newSquare.cloneSquare()); //pushes an empty backtrack square
                        addSquareValue(newSquare.row, newSquare.column, newSquare.potentialValues.ElementAt(0));
                        return;
                    }
                }

                undo(); //backtrack if no more squares with potential values are found
            }
        }

        private void undo()
        {
            undoCount++;

            currentBoard.Clear();
            currentBoard = boards.Pop(); //goes back to the board before the last square was guessed
            Square backtrackSquare = backtrackSquares.Pop(); //this is the square before the value was guessed
            Square backtrackSquareCurrent = returnCurrent(backtrackSquare.row, backtrackSquare.column);
            backtrackSquareCurrent.potentialValues.RemoveAt(0); //first value was wrong

             if (backtrackSquareCurrent.potentialValues.Count != 0) //try another value
            {
                if (backtrackSquareCurrent.potentialValues.Count != 1) //backtrack square needs to be preserved
                {
                    backtrackSquares.Push(backtrackSquareCurrent.cloneSquare());
                    boards.Push(cloneBoard());
                }
                    
                addSquareValue(backtrackSquareCurrent.row, backtrackSquareCurrent.column, backtrackSquareCurrent.potentialValues.ElementAt(0));
            }
            else if (backtrackSquares.Count == 0) //unsolvable sudoku
                main.showUnsolvable();
            else undo();
        }
    }
}
