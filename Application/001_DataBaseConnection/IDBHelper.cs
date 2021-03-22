using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace UrbanX.Application
{
    interface IDBHelper
    {
        /// <summary>
        /// 执行 Transact-SQL 并返回受影响的行数
        /// </summary>
        int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        /// <summary>
        /// 在事务中执行 Transact-SQL 语句并返回受影响的行数.
        /// </summary>
        int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，返回DataSet
        /// </summary>
        DataSet ExecuteQuery(string connectionString, CommandType cmdType,string cmdText, params DbParameter[] cmdParms);
        /// <summary>
        /// 在事务中执行查询，返回DataSet
        /// </summary>
        DataSet ExecuteQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，返回DataReader
        /// </summary>
        DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        /// <summary>
        /// 在事务中执行查询，返回DataReader
        /// </summary>
        DbDataReader ExecuteReader(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
        /// <summary>
        /// 在事务中执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] cmdParms);
    }
}
