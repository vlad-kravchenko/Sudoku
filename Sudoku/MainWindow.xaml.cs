using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sudoku
{
    public partial class MainWindow : Window
    {
        int[,] map;
        List<Cell> cells = new List<Cell>();
        int prevLine = 0;
        string prevLevel = "Normal";
        int cRow, cCol;

        private bool HasConflicts
        {
            get { return MainGrid.Children.Cast<Border>().Any(b => b.Background == Brushes.Red); }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitMap("Normal", false);
        }

        private void UpdateGrid()
        {
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            cells = new List<Cell>();

            for(int i = 0; i < 9; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    Border border = new Border();
                    border.BorderThickness = GetBorder(row, col);
                    border.BorderBrush = Brushes.Black;

                    TextBlock block = new TextBlock();
                    block.VerticalAlignment = VerticalAlignment.Center;
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    block.Text = map[row, col].ToString() == "0" ? "" : map[row, col].ToString();
                    cells.Add(new Cell { Block = block, Row = row, Col = col, Editable = map[row, col].ToString() == "0" });
                    border.Child = block;

                    MainGrid.Children.Add(border);
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                }
            }
        }

        public void InitMap(string level, bool reset)
        {
            map = new int[9, 9];
            List<string> resource = (Properties.Resources.ResourceManager.GetObject(level) as string).Split('\n').ToList();
            if (!reset)
                prevLine = new Random().Next(0, resource.Count);
            int k = 0;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    map[row, col] = Convert.ToInt32(resource[prevLine][k].ToString());
                    k++;
                }
            }
            UpdateGrid();
            Window_SizeChanged(null, null);
        }

        private Thickness GetBorder(int row, int col)
        {
            if ((col == 2 || col == 5) && (row == 2 || row == 5))
                return new Thickness(1, 1, 5, 5);
            if (col == 2 || col == 5)
                return new Thickness(1, 1, 5, 1);
            if (row == 2 || row == 5)
                return new Thickness(1, 1, 1, 5);
            return new Thickness(1, 1, 1, 1);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int row = 0;
            int col = 0;
            GetClickCoordinates(out row, out col);

            if (HasConflicts && (row != cRow || col != cCol)) return;
            if (cells.FirstOrDefault(c => c.Row == row && c.Col == col).Editable == false) return;

            var block = cells.FirstOrDefault(c => c.Col == col && c.Row == row).Block;
            int number = block.Text == "" ? 0 : Convert.ToInt32(block.Text);
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    number++;
                    if (number == 10) number = 0;
                    block.Text = number == 0 ? "" : number.ToString();
                    CheckEndGame(row, col);
                    break;
                case MouseButton.Right:
                    number--;
                    if (number == -1) number = 9;
                    block.Text = number == 0 ? "" : number.ToString();
                    CheckEndGame(row, col);
                    break;
            }
        }

        private void CheckEndGame(int row, int col)
        {
            CheckConflicts(row, col);
            if (!MainGrid.Children.Cast<Border>().Any(b=>b.Background == Brushes.Red) && !cells.Any(c=>c.Block.Text == ""))
            {
                MessageBox.Show("End of game!");
                InitMap("Normal", false);
            }
        }

        private void CheckConflicts(int row, int col)
        {
            foreach (var border in MainGrid.Children.Cast<Border>()) border.Background = Brushes.White;
            CheckRow(row);
            CheckCol(col);
            CheckRegion(row, col);
            if (HasConflicts)
            {
                cRow = row;
                cCol = col;
            }
        }

        private void CheckRow(int row)
        {
            List<string> rowValues = cells.Where(c => c.Row == row && c.Block.Text != "").Select(c => c.Block.Text).ToList();
            if (rowValues.Distinct().Count() != rowValues.Count)
            {
                foreach (var tb in cells.Where(c => c.Row == row).Select(c => c.Block))
                {
                    (tb.Parent as Border).Background = Brushes.Red;
                }
            }
        }

        private void CheckCol(int col)
        {
            List<string> colValues = cells.Where(c => c.Col == col && c.Block.Text != "").Select(c => c.Block.Text).ToList();
            if (colValues.Distinct().Count() != colValues.Count)
            {
                foreach (var tb in cells.Where(c => c.Col == col).Select(c => c.Block))
                {
                    (tb.Parent as Border).Background = Brushes.Red;
                }
            }
        }

        private void CheckRegion(int row, int col)
        {
            var region = cells;
            if (row < 3)
            {
                region = region.Where(c => c.Row < 3).ToList();
            }
            else if (row > 2 && row < 6)
            {
                region = region.Where(c => c.Row > 2 && c.Row < 6).ToList();
            }
            else if(row > 5)
            {
                region = region.Where(c => c.Row > 5).ToList();
            }
            if (col < 3)
            {
                region = region.Where(c => c.Col < 3).ToList();
            }
            else if(col > 2 && col < 6)
            {
                region = region.Where(c => c.Col > 2 && c.Col < 6).ToList();
            }
            else if(col > 5)
            {
                region = region.Where(c => c.Col > 5).ToList();
            }
            List<string> colValues = region.Where(c => c.Block.Text != "").Select(c => c.Block.Text).ToList();
            if (colValues.Distinct().Count() != colValues.Count)
            {
                foreach (var tb in region.Select(c => c.Block))
                {
                    (tb.Parent as Border).Background = Brushes.Red;
                }
            }
        }

        private void GetClickCoordinates(out int row, out int col)
        {
            col = row = 0;
            var point = Mouse.GetPosition(MainGrid);
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;
            foreach (var rowDefinition in MainGrid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                    break;
                row++;
            }
            foreach (var columnDefinition in MainGrid.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                    break;
                col++;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var block in cells.Select(c => c.Block))
            {
                block.FontSize = this.Height / 15;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            InitMap(prevLevel, true);
        }

        private void NewGameClick(object sender, RoutedEventArgs e)
        {
            InitMap((sender as MenuItem).Name, false);
            prevLevel = (sender as MenuItem).Name;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (HasConflicts) return;

            if (e.Key > Key.D0 || e.Key < Key.D9)
            {
                if (e.Key > Key.NumPad0 || e.Key < Key.NumPad9)
                {
                    foreach (var border in MainGrid.Children.Cast<Border>()) border.Background = Brushes.White;

                    string text = e.Key.ToString().Last().ToString();
                    if (text == "0") text = "";
                    foreach (var tb in cells.Where(c => c.Block.Text == text).Select(c => c.Block))
                    {
                        (tb.Parent as Border).Background = Brushes.LightGreen;
                    }
                }
            }
        }
    }
}