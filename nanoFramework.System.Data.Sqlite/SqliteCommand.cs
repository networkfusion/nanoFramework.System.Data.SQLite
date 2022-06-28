using System;
using System.Collections;
using System.Runtime.CompilerServices;

//https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/compare
//based upon GHI (to get working) but trying to change to be close to https://github.com/Faithlife/System.Data.SQLite
namespace System.Data.Sqlite
{
    //Most of this should be internal and called by SQLiteConnection
    /// <summary>
    /// Sqlite implementation of DbCommand.
    /// </summary>
    public class SqliteCommand : IDisposable
    {
        //actually, we should be using Enum SQLiteColumnType
        private const int SQLITE_INTEGER = 1;
        private const int SQLITE_FLOAT = 2;
        private const int SQLITE_TEXT = 3;
        private const int SQLITE_BLOB = 4;
        private const int SQLITE_NULL = 5;
        //


        //actually, we should be using Enum SQLiteErrorCode
        private const int SQLITE_OK = 0;
        private const int SQLITE_ROW = 100;
        private const int SQLITE_DONE = 101;
        //


        private bool disposed;

        #pragma warning disable 0414
        private readonly int nativePointer;
        #pragma warning restore 0414

        //should use SQLiteConnection.cs
        //protected override DbConnection DbConnection { get; set; }

        public SqliteCommand()
        {
            nativePointer = 0;
            disposed = false;

            if (NativeOpen(":memory:") != SQLITE_OK) //SQLiteErrorCode.Ok
            {
                throw new OpenException();
            }
        }

        //should use SQLiteConnection.cs
        public SqliteCommand(string file)
        {
            nativePointer = 0;
            disposed = false;

            //file = StorageFile.GetFileFromPath(file).ToString();  //Path.GetFullPath(file); //TODO: is this actually equivilent? anyway, should we just require a full path to the file, so that we dont need the windows.storage dependency!
            if (System.IO.File.Exists(file))
            {

                //if (file == null)
                //{
                //    throw new ArgumentException("You must provide a valid file.", nameof(file));
                //}

                if (NativeOpen(file) != SQLITE_OK) //SQLiteErrorCode.Ok
                {
                    throw new OpenException();
                }
            }
        }

        ~SqliteCommand()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public void ExecuteNonQuery(string query)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Object disposed.");
            }
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var handle = Prepare(query);
            if (NativeStep(handle) != SQLITE_DONE)
            {
                throw new QueryExecutionException(NativeErrorMessage());
            }

            FinalizeSqlStatment(handle);
        }

        //where should this be? SQLiteDataReader and public override object GetValue(int ordinal)  with the function called ExecuteReader() sounds correct?!
        public ResultSet ExecuteQuery(string query)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Object disposed.");
            }
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var handle = Prepare(query);
            var columnCount = NativeColumnCount(handle);
            var columnNames = new string[columnCount];

            for (var i = 0; i < columnCount; i++)
            {
                columnNames[i] = NativeColumnName(handle, i);
            }

            var results = new ResultSet(columnNames);

            while (NativeStep(handle) == SQLITE_ROW) //SQLiteErrorCode.Row
            {
                var row = new ArrayList();

                for (var i = 0; i < columnCount; i++)
                {
                    switch (NativeColumnType(handle, i))
                    {
                        case SQLITE_INTEGER: //SQLiteColumnType.Integer:
                            row.Add(NativeColumnLong(handle, i)); 
                            break;
                        case SQLITE_TEXT: //SQLiteColumnType.Text:
                            row.Add(NativeColumnText(handle, i)); 
                            break;
                        case SQLITE_FLOAT: //SQLiteColumnType.Float:
                            row.Add(NativeColumnDouble(handle, i)); 
                            break;
                        case SQLITE_NULL: //SQLiteColumnType.Null
                            row.Add(null); 
                            break;
                        case SQLITE_BLOB: //SQLiteColumnType.Blob:
                            var length = NativeColumnBlobLength(handle, i);

                            if (length == 0)
                            {
                                row.Add(null);
                                break;
                            }

                            var buffer = new byte[length];
                            NativeColumnBlobData(handle, i, buffer);
                            row.Add(buffer);

                            break;
                    }
                }

                results.AddRow(row);
            }

            FinalizeSqlStatment(handle);

            return results;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            NativeClose();

            disposed = true;
        }

        public int Prepare(string query)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Object disposed.");
            }

            if (NativePrepare(query, query.Length, out var handle) != SQLITE_OK) //SQLiteErrorCode.Ok
            {
                throw new QueryPrepareException(NativeErrorMessage());
            }

            return handle;
        }

        private void FinalizeSqlStatment(int handle)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Object disposed.");
            }

            if (NativeFinalize(handle) != SQLITE_OK) //SQLiteErrorCode.Ok
            {
                throw new QueryFinalizationException(NativeErrorMessage());
            }
        }


        //use NativeMethods class?
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string filename);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativePrepare(string query, int queryLength, out int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeErrorMessage();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeStep(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeFinalize(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnCount(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeColumnName(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnType(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern long NativeColumnLong(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeColumnText(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern double NativeColumnDouble(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnBlobLength(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeColumnBlobData(int handle, int column, byte[] buffer);
    }
}