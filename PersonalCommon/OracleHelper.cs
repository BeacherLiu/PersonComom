using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace PersonalCommon
{
    /// <summary>
    /// Oracle官网
    /// </summary>
    public class OracleHelper : IDisposable
    {

        //<add name="FdOracleConn" providerName="Oracle.ManagedDataAccess.Client" connectionString="DATA SOURCE=172.16.17.42:1521/orcl;PASSWORD=FAMILYDOCTOR;PERSIST SECURITY INFO=True;USER ID=FAMILYDOCTOR"/>
        public static string conn;
        public OracleHelper(string connect)
        {
            conn = connect;
            if (mQracleConnecting == null)
                mQracleConnecting = new OracleConnection(connect);
            if (mQracleConnecting.State != ConnectionState.Open)
                mQracleConnecting.Open();
        }

        #region DB
        private static OracleConnection mQracleConnecting = null;
        private static OracleConnection QracleConnecting
        {
            get
            {
                return mQracleConnecting;
            }
        }

        public static DataTable DBGetDataTable(string sql)
        {
            try
            {
                DataTable dataSet = new DataTable();
                OracleDataAdapter OraDA = new OracleDataAdapter(sql, mQracleConnecting);
                OraDA.Fill(dataSet);
                return dataSet;
            }
            catch (Exception)
            {
                //FileSupport.Instance.Write("数据库连接异常" + conn);
                return null;
            }

        }

        // 执行SQL语句,返回所影响的行数   
        public static int ExecuteSQL(string sql)
        {
            int Cmd = 0;
            OracleCommand command = new OracleCommand(sql, QracleConnecting);
            try
            {
                Cmd = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // FileSupport.Instance.Write(ex.ToString());
            }
            return Cmd;
        }

        public static int ExecuteSQL_Insert(object item)
        {
            var type = item.GetType();
            var tablename = type.Name;
            var atts = type.GetProperties();
            var keys = "";
            var values = "";
            foreach (var a in atts)
            {
                var aname = a.Name;
                var value = a.GetValue(item);
                keys += aname + ",";
                values += GetValue2String(value) + ",";
            }

            keys = keys.Trim(',');
            values = values.Trim(',');

            var sql = "INSERT INTO " + tablename + " ( " + keys + " ) VALUES ( " + values + " )";
            return ExecuteSQL(sql);
        }

        #endregion

        /// <summary>
        /// 获取对象列表
        /// </summary>
        /// <typeparam name="T">表所对应的对象名称</typeparam>
        /// <param name="sql">查询语句</param>
        /// <returns>返回获取到的对象实例列表</returns>
        public static List<T> QueryObjectList<T>(string sql) where T : new()
        {
            var dataset = ReturnDataSet(sql, "table");
            if (dataset != null)
            {
                var table = dataset.Tables[0];
                return ConvertTableToObject<T>(table);
            }
            return null;
        }

        public static DataSet ReturnDataSet(string sql, string DataSetName)
        {
            DataSet dataSet = new DataSet();
            OracleDataAdapter OraDA = new OracleDataAdapter(sql, QracleConnecting);
            OraDA.Fill(dataSet, DataSetName);
            return dataSet;
        }

        private static List<T> ConvertTableToObject<T>(DataTable t) where T : new()
        {
            if (t == null)
                return null;
            List<T> list = new List<T>();
            foreach (DataRow row in t.Rows)
            {
                T obj = new T();
                GetObject(t.Columns, row, obj);
                if (obj != null && obj is T)
                    list.Add(obj);
            }
            return list;
        }

        public T ConvertToObject<T>(DataRow row) where T : new()
        {
            object obj = new T();
            if (row != null)
            {
                DataTable t = row.Table;
                GetObject(t.Columns, row, obj);
            }
            if (obj != null && obj is T)
                return (T)obj;
            else
                return default(T);
        }

        private static void GetObject(DataColumnCollection cols, DataRow dr, Object obj)
        {
            Type t = obj.GetType();
            var props = t.GetProperties();
            foreach (var pro in props)
            {
                if (cols.Contains(pro.Name))
                {
                    if (dr[pro.Name] != DBNull.Value)
                    {
                        try
                        {
                            switch (pro.PropertyType.Name)
                            {
                                case "Int32":
                                    {
                                        Int32 value = Convert.ToInt32(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                case "Double":
                                    {
                                        double value = Convert.ToDouble(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                case "Single":
                                    {
                                        float value = Convert.ToSingle(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                case "Int64":
                                    {
                                        Int64 value = Convert.ToInt64(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                case "Int16":
                                    {
                                        Int16 value = Convert.ToInt16(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                case "Decimal":
                                    {
                                        Decimal value = Convert.ToDecimal(dr[pro.Name]);
                                        pro.SetValue(obj, value, null);
                                    }
                                    break;
                                default:
                                    {
                                        pro.SetValue(obj, dr[pro.Name], null);
                                    }
                                    break;
                            }
                        }
                        catch
                        {
                            pro.SetValue(obj, null, null);
                        }
                        finally
                        {
                        }
                    }
                    else
                    {
                        pro.SetValue(obj, null, null);
                    }
                }
            }
        }

        public static string GetValue2String(Object obj)
        {
            if (obj == null)
                return "null";
            Type t = obj.GetType();
            try
            {
                switch (t.Name)
                {
                    case "String":
                        {
                            return "'" + obj.ToString() + "'";
                        }
                    case "DateTime":
                        {
                            return "to_date('" + obj.ToString() + "','YYYY-MM-DD hh24:mi:ss')";
                        }
                    default:
                        {
                            return obj.ToString();
                        }
                }
            }
            catch
            {
                return "";
            }
        }

        public void Dispose()
        {
            if (mQracleConnecting.State == ConnectionState.Open)
                mQracleConnecting.Close();
        }

        /// <summary>
        /// 不带参数的存储过程
        /// </summary>
        /// <param name="strconn">连接字符串</param>
        /// <param name="procname">存储过程名</param>
        public static void ExecProc(string strconn,string procname)
        {
            OracleConnection conn = new OracleConnection(strconn);
            conn.Open();
            OracleCommand orm = conn.CreateCommand();
            orm.CommandType = CommandType.StoredProcedure;
            orm.ExecuteNonQuery();
            conn.Close();
        }
        /// <summary>
        /// 没有返回值的带参数的存储过程
        /// </summary>
        /// <param name="strconn"></param>
        /// <param name="procname"></param>
        /// <param name="parameters">OracleParameter[] parameters ={new OracleParameter("paramin",OracleType.VarChar,20)};parameters[0].Value = "bjd";parameters[2].Value = "cs";parameters[0].Direction = ParameterDirection.Input;</param>
        public static void ExecProc(string strconn, string procname,params OracleParameter[] parameters)
        {
            OracleConnection oc = new OracleConnection(strconn);
            oc.Open();
            OracleCommand om = oc.CreateCommand();
            om.CommandType = CommandType.StoredProcedure;
            om.CommandText = procname;
            foreach (OracleParameter parameter in parameters)
            {
                om.Parameters.Add(parameter);
            }
            om.ExecuteNonQuery();
            oc.Close();
        }
        /// <summary>
        /// 带参数有返回值的存储过程
        /// </summary>
        /// <param name="strconn"></param>
        /// <param name="procname"></param>
        /// <param name="parameters"></param>
        public static DataSet ExecProcDataSet(string strconn, string procname, params OracleParameter[] parameters)
        {
            OracleConnection oc = new OracleConnection(strconn);
            oc.Open();
            OracleCommand om = oc.CreateCommand();
            om.CommandType = CommandType.StoredProcedure;
            om.CommandText = procname;
            foreach (OracleParameter parameter in parameters)
            {
                om.Parameters.Add(parameter);
            }
            om.ExecuteNonQuery();
            oc.Close();
            OracleDataAdapter da = new OracleDataAdapter(om);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }
    }
}
