using System.Windows.Controls;

namespace Sudoku
{
    public class Cell
    {
        public TextBlock Block { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public bool Editable { get; set; }
    }
}