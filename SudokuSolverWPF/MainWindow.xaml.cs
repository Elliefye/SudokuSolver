using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SudokuSolverWPF
{
    public partial class MainWindow : Window
    {
        private IEnumerable<TextBox> boxes;
        public bool allValuesAdded = false, stop = false;
        public Board currentGame;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonSolve_Click(object sender, RoutedEventArgs e)
        {
            allValuesAdded = false;
            stop = false;

            boxes = customSizeSudokuGrid.Children.OfType<TextBox>();

            foreach (TextBox box in boxes) //check for invalid input and color squares with existing values
            {
                if (!String.IsNullOrWhiteSpace(box.Text))
                    box.Background = Brushes.LightGray;
                else box.Background = Brushes.White;

                int boxValue = 1;

                if ((!String.IsNullOrWhiteSpace(box.Text)) && (!int.TryParse(box.Text, out boxValue)))
                {
                    MessageBox.Show("Incorrect input."); //not a number
                    return;
                }
                else if ((boxValue > Square.sudokuSize) || (boxValue < 1))
                {
                    MessageBox.Show("Incorrect input."); //incorrect number
                    return;
                }
            }


            currentGame = new Board();
            currentGame.main = this;

            foreach (TextBox t in boxes)
            {
                if (!String.IsNullOrWhiteSpace(t.Text))
                {
                    if (stop)
                        break;
                    //TextBox names are of this format: _rcs, where r = row, c = column
                    int rowId = int.Parse(t.Name.Substring(1, 2));
                    int columnId = int.Parse(t.Name.Substring(3, 2));
                    int squareValue = int.Parse(t.Text);
                    currentGame.returnCurrent(rowId, columnId).inputSquare = true;
                    currentGame.returnCurrent(rowId, columnId).potentialValues.Clear();
                    currentGame.addSquareValue(rowId, columnId, squareValue);
                }
            }

            if (stop)
                return;
            else
            {
                allValuesAdded = true;
                currentGame.trySmartSolving();
            }

        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            boxes = customSizeSudokuGrid.Children.OfType<TextBox>();

            foreach (TextBox t in boxes)
            {
                t.Text = "";
                t.Background = Brushes.White;
            }
        }

        public void showSolved()
        {
            MessageBox.Show("Sudoku has been solved.");

            foreach (Square square in currentGame.currentBoard)
                displaySolution(square);
        }

        public void showError()
        {
            MessageBox.Show("Wrong input.");
        }

        public void showUnsolvable()
        {
            MessageBox.Show("Sudoku can't be solved.");
        }

        private void newSize_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                int newSudokuSize = int.Parse(newSize.Text);
                double possibleSize = Math.Sqrt(newSudokuSize);
                if (possibleSize % 1 != 0)
                    showError();
                else
                {
                    newSize.Visibility = Visibility.Hidden;
                    customBorder.Visibility = Visibility.Visible;
                    customSizeSudokuGrid.ShowGridLines = true;

                    for (int i = 0; i <= newSudokuSize; i++)
                    {
                        customSizeSudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        customSizeSudokuGrid.RowDefinitions.Add(new RowDefinition());
                    }

                    foreach (ColumnDefinition col in customSizeSudokuGrid.ColumnDefinitions)
                        col.Width = new GridLength(customBorder.Width / newSudokuSize);

                    foreach (RowDefinition row in customSizeSudokuGrid.RowDefinitions)
                        row.Height = new GridLength(customBorder.Height / newSudokuSize);

                    for (int j = 0; j < newSudokuSize; j++) //rows
                        for (int k = 0; k < newSudokuSize; k++) //columns
                        {
                            TextBox square = new TextBox();

                            string name = "_";
                            if (j < 10)
                                name += "0";
                            name += j;
                            if (k < 10)
                                name += "0";
                            name += k;

                            square.Name = name;
                            Grid.SetRow(square, j);
                            Grid.SetColumn(square, k);
                            customSizeSudokuGrid.Children.Add(square);
                        }

                    Square.sudokuSize = newSudokuSize;

                    for (int l = 0; l <= newSudokuSize; l += (int)possibleSize)
                        for (int m = 0; m <= newSudokuSize; m += (int)possibleSize)
                        {
                            Border b = new Border();
                            b.BorderThickness = new Thickness(3);
                            b.BorderBrush = Brushes.Black;
                            Grid.SetRow(b, l);
                            Grid.SetColumn(b, m);
                            Grid.SetRowSpan(b, (int)possibleSize);
                            Grid.SetColumnSpan(b, (int)possibleSize);
                            customSizeSudokuGrid.Children.Add(b);
                        }

                    SolveBtn.IsEnabled = true;
                    ClearBtn.IsEnabled = true;
                }
            }
        }

        private void displaySolution(Square s)
        {
            if (boxes != null)
            {
                foreach (TextBox box in boxes)
                    if ((int.Parse(box.Name.Substring(1, 2)) == s.row) && (int.Parse(box.Name.Substring(3, 2)) == s.column))
                        box.Text = s.currentValue.ToString();
            }
            else MessageBox.Show("Error.");
        }
    }
}
