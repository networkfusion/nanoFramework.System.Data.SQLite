using System;
using System.Collections;

namespace System.Data.Sqlite
{
    public class ResultSet
    {
        public int RowCount { get; private set; }
        public int ColumnCount { get; }
        public string[] ColumnNames { get; }
        public ArrayList Data { get; }

        public ArrayList this[int row]
        {
            get
            {
                if (row < 0 || row >= RowCount) throw new ArgumentOutOfRangeException(nameof(row));

                return (ArrayList)Data[row];
            }
        }

        public object this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= RowCount) throw new ArgumentOutOfRangeException(nameof(row));
                if (column < 0 || column >= ColumnCount) throw new ArgumentOutOfRangeException(nameof(column));

                return ((ArrayList)Data[row])[column];
            }
        }

        internal ResultSet(string[] columnNames)
        {
            if (columnNames == null) throw new ArgumentNullException(nameof(columnNames));
            if (columnNames.Length == 0) throw new ArgumentException("At least one column must be provided.", nameof(columnNames));

            Data = new ArrayList();
            ColumnNames = new string[columnNames.Length];
            ColumnCount = columnNames.Length;
            RowCount = 0;

            Array.Copy(columnNames, ColumnNames, columnNames.Length);
        }

        internal void AddRow(ArrayList row)
        {
            if (row.Count != ColumnCount) throw new ArgumentException("Row must contain exactly as many members as the number of columns in this result set.", nameof(row));

            RowCount++;
            Data.Add(row);
        }
    }
}