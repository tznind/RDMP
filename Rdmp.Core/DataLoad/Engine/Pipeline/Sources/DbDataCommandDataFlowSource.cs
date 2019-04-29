// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Sources
{
    /// <inheritdoc/>
    public class DbDataCommandDataFlowSource :  IDbDataCommandDataFlowSource
    {
        private readonly string _sql;
        private DbDataReader _reader;
        private readonly DbConnectionStringBuilder _builder;
        private readonly int _timeout;
        private DbConnection _con;

        private readonly string _taskBeingPerformed;
        private Stopwatch timer = new Stopwatch();

        public int BatchSize { get; set; }

        public DbCommand cmd { get; private set; }

        public bool AllowEmptyResultSets { get; set; }
        public int TotalRowsRead { get; set; }

        public DbDataCommandDataFlowSource(string sql,string taskBeingPerformed, DbConnectionStringBuilder builder, int timeout)
        {
            _sql = sql;
            _taskBeingPerformed = taskBeingPerformed;
            _builder = builder;
            _timeout = timeout;

            BatchSize = 10000;
        }

        private int _numberOfColumns;

        private bool firstChunk = true;

        public DataTable GetChunk(IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            if (_reader == null)
            {
                _con = DatabaseCommandHelper.GetConnection(_builder);
                _con.Open();

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Running SQL:" +Environment.NewLine + _sql));

                cmd = DatabaseCommandHelper.GetCommand(_sql, _con);
                cmd.CommandTimeout = _timeout;
                
                _reader = cmd.ExecuteReaderAsync(cancellationToken.AbortToken).Result;
                _numberOfColumns = _reader.FieldCount;
            }

            int readThisBatch = 0;
            timer.Start();
            try
            {
                DataTable chunk = GetChunkSchema(_reader);
                
                while (_reader.Read())
                {
                    AddRowToDataTable(chunk, _reader);
                    readThisBatch ++;

                    //we reached batch limit
                    if (readThisBatch == BatchSize)
                        return chunk;
                }

                //if data was read
                if (readThisBatch > 0)
                    return chunk;

                //data is exhausted

                //if data was exhausted on first read and we are allowing empty result sets
                if (firstChunk && AllowEmptyResultSets)
                    return chunk;//return the empty chunk

                //data exhausted
                return null;
            }
            catch (Exception e)
            {
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Source read failed",e));
                throw;
            }
            finally
            {
                firstChunk = false;
                timer.Stop();
                job.OnProgress(this, new ProgressEventArgs(_taskBeingPerformed, new ProgressMeasurement(TotalRowsRead, ProgressType.Records), timer.Elapsed));

            }
        }

        private DataRow AddRowToDataTable(DataTable chunk, DbDataReader reader)
        {
            object[] values = new object[_numberOfColumns];

            reader.GetValues(values);
            TotalRowsRead++;
            return chunk.LoadDataRow(values, LoadOption.Upsert);
        }

        /// <inheritdoc/>
        public DataRow ReadOneRow()
        {
            //return null if there are no more records to read
            return _reader.Read() ? AddRowToDataTable(GetChunkSchema(_reader), _reader) : null;
        }

        private DataTable GetChunkSchema(DbDataReader reader)
        {
            DataTable toReturn = new DataTable("dt");
            
            //Retrieve column schema into a DataTable.
            var schemaTable = reader.GetSchemaTable();
            if (schemaTable == null)
                throw new InvalidOperationException("Could not retrieve schema information from the DbDataReader");
            
            Debug.Assert(schemaTable.Columns[0].ColumnName.ToLower().Contains("name"));

            //For each field in the table...
            foreach (DataRow myField in schemaTable.Rows)
            {

                var t = Type.GetType(myField["DataType"].ToString());

                if(t == null)
                    throw new NotSupportedException("Type.GetType failed on SQL DataType:" + myField["DataType"]);

                //lets not mess around with floats, make everything a double please
                if (t == typeof (float))
                    t = typeof (double);


                toReturn.Columns.Add(myField[0].ToString(), t);//0 should always be the column name
            }

            return toReturn;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            CloseReader(listener);
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            CloseReader(listener);
        }

        private void CloseReader(IDataLoadEventListener listener)
        {
            try
            {
                if (_con == null)
                    return;

                if (_con.State != ConnectionState.Closed)
                    _con.Close();

                _reader.Dispose();

                //do not do this more than once! which could happen if they abort then it disposes
                _con = null;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Could not close Reader / Connection", e));
            }
        }

        public DataTable TryGetPreview()
        {
            DataTable chunk = new DataTable();
            using (var con = DatabaseCommandHelper.GetConnection(_builder))
            {
                con.Open();
                var da = DatabaseCommandHelper.GetDataAdapter(DatabaseCommandHelper.GetCommand(_sql, con));

                int read = da.Fill(0, 100, chunk);

                if (read == 0)
                    return null;

                return chunk;
            }
        }
    }
}
