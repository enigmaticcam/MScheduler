using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Abstract {
    public interface IConnectionControl {
        string DatabaseName { get; }
        string ConnectionString { get; }
        DataSet ExecuteDataSet(string sql);
        object ExecuteScalar(string sql);
        void ExecuteNonQuery(string sql);
        string SqlSafe(string sql);
    }
    
    public class ConnectionControl : IConnectionControl {

        private string _databaseName;
        public string DatabaseName {
            get { return _databaseName; }
        }

        private string _connectionString;
        public string ConnectionString {
            get { return _connectionString; }
        }

        public DataSet ExecuteDataSet(string sql) {
            return DAC2.ExecuteDataset(_connectionString, CommandType.Text, sql);
        }

        public object ExecuteScalar(string sql) {
            return DAC2.ExecuteScalar(_connectionString, CommandType.Text, sql);
        }

        public void ExecuteNonQuery(string sql) {
            DAC2.ExecuteNonQuery(_connectionString, CommandType.Text, sql);
        }

        public string SqlSafe(string sql) {
            return sql.Replace("'", "''");
        }

        public ConnectionControl(string databaseName, string connectionString) {
            _databaseName = databaseName;
            _connectionString = connectionString;
        }
    }

    
}
