using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCommon
{
    /// <summary>
    /// 类型转换
    /// </summary>
    public static class TypeHelper
    {
        #region IQueryable<T> 转DataTable
        //调用 DataTable dt = query.ToDataTable(rec => new object[] { query });
        public static DataTable ToDataTable<T>(this IEnumerable<T> varlist, CreateRowDelegate<T> fn)
        {
            DataTable dtReturn = new DataTable();
            // column names
            PropertyInfo[] oProps = null;
            // Could add a check to verify that there is an element 0
            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType; if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }
                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow(); foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);
                }
                dtReturn.Rows.Add(dr);
            }
            return (dtReturn);
        }

        public delegate object[] CreateRowDelegate<T>(T t);
        #endregion

        #region List<T>/IEnumerable转换到DataTable/DataView  方法1
        /// <summary>
        /// Convert a List{T} to a DataTable.
        /// </summary>
        public static DataTable ListToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in items)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        private static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }
        #endregion

        #region List<T>/IEnumerable转换到DataTable/DataView 方法2
        public  static DataTable IEnumerableToDataTable<T>(IEnumerable<T> collection)
        {
            var props = typeof(T).GetProperties();
            var dt = new DataTable();
            dt.Columns.AddRange(props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray());
            if (collection.Count() > 0)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in props)
                    {
                        object obj = pi.GetValue(collection.ElementAt(i), null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    dt.LoadDataRow(array, true);
                }
            }
            return dt;
        }
        #endregion

        #region List集合转DataTable
        /// <summary>  
        /// List集合转DataTable  
        /// </summary>  
        /// <typeparam name="T">实体类型</typeparam>  
        /// <param name="list">传入集合</param>  
        /// <param name="isStoreDB">是否存入数据库DateTime字段，date时间范围没事，取出展示不用设置TRUE</param>  
        /// <returns>返回datatable结果</returns>  
        public static DataTable ListToTable<T>(List<T> list, bool isStoreDB = true)
        {
            Type tp = typeof(T);
            PropertyInfo[] proInfos = tp.GetProperties();
            DataTable dt = new DataTable();
            foreach (var item in proInfos)
            {
                dt.Columns.Add(item.Name, item.PropertyType); //添加列明及对应类型  
            }
            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                foreach (var proInfo in proInfos)
                {
                    object obj = proInfo.GetValue(item);
                    if (obj == null)
                    {
                        continue;
                    }
                    //if (obj != null)  
                    // {  
                    if (isStoreDB && proInfo.PropertyType == typeof(DateTime) && Convert.ToDateTime(obj) < Convert.ToDateTime("1753-01-01"))
                    {
                        continue;
                    }
                    // dr[proInfo.Name] = proInfo.GetValue(item);  
                    dr[proInfo.Name] = obj;
                    // }  
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion

        #region 提取DataTable某一行转为指定对象
        /// <summary>  
        /// table指定行转对象  
        /// </summary>  
        /// <typeparam name="T">实体</typeparam>  
        /// <param name="dt">传入的表格</param>  
        /// <param name="rowindex">table行索引，默认为第一行</param>  
        /// <param name="isStoreDB">是否存入数据库DateTime字段，date时间范围没事，取出展示不用设置TRUE</param> 
        /// <returns>返回实体对象</returns>  
        public static T TableToEntity<T>(DataTable dt, int rowindex = 0, bool isStoreDB = true)
        {
            Type type = typeof(T);
            T entity = Activator.CreateInstance<T>(); //创建对象实例  
            if (dt == null)
            {
                return entity;
            }
            //if (dt != null)  
            //{  
            DataRow row = dt.Rows[rowindex]; //要查询的行索引  
            PropertyInfo[] pArray = type.GetProperties();
            foreach (PropertyInfo p in pArray)
            {
                if (!dt.Columns.Contains(p.Name) || row[p.Name] == null || row[p.Name] == DBNull.Value)
                {
                    continue;
                }

                if (isStoreDB && p.PropertyType == typeof(DateTime) && Convert.ToDateTime(row[p.Name]) < Convert.ToDateTime("1753-01-02"))
                {
                    continue;
                }
                try
                {
                    var obj = Convert.ChangeType(row[p.Name], p.PropertyType);//类型强转，将table字段类型转为对象字段类型  
                    p.SetValue(entity, obj, null);
                }
                catch (Exception)
                {
                    // throw;  
                }
                // p.SetValue(entity, row[p.Name], null);                     
            }
            //  }  
            return entity;
        }
        #endregion

    }

    /// <summary>
    /// 实体转换辅助类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ModelConvertHelper<T> where T : new()
    {
        /// <summary>
        /// DataTable转化IList
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IList<T> ConvertToModel(DataTable dt)
        {
            // 定义集合    
            IList<T> ts = new List<T>();

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }
        /// <summary>  
        /// DataTable转化为List集合  
        /// </summary>  
        /// <typeparam name="T">实体对象</typeparam>  
        /// <param name="dt">datatable表</param>  
        /// <param name="isStoreDB">是否存入数据库datetime字段，date字段没事，取出不用判断</param>  
        /// <returns>返回list集合</returns>  
        public static List<T> TableToList<T>(DataTable dt, bool isStoreDB = true)
        {
            List<T> list = new List<T>();
            Type type = typeof(T);
            //List<string> listColums = new List<string>();  
            PropertyInfo[] pArray = type.GetProperties(); //集合属性数组  
            foreach (DataRow row in dt.Rows)
            {
                T entity = Activator.CreateInstance<T>(); //新建对象实例   
                foreach (PropertyInfo p in pArray)
                {
                    if (!dt.Columns.Contains(p.Name) || row[p.Name] == null || row[p.Name] == DBNull.Value)
                    {
                        continue;  //DataTable列中不存在集合属性或者字段内容为空则，跳出循环，进行下个循环     
                    }
                    if (isStoreDB && p.PropertyType == typeof(DateTime) && Convert.ToDateTime(row[p.Name]) < Convert.ToDateTime("1753-01-01"))
                    {
                        continue;
                    }
                    try
                    {
                        var obj = Convert.ChangeType(row[p.Name], p.PropertyType);//类型强转，将table字段类型转为集合字段类型    
                        p.SetValue(entity, obj, null);
                    }
                    catch (Exception)
                    {
                        // throw;  
                    }
                    //if (row[p.Name].GetType() == p.PropertyType)  
                    //{  
                    //    p.SetValue(entity, row[p.Name], null); //如果不考虑类型异常，foreach下面只要这一句就行  
                    //}                      
                    //object obj = null;  
                    //if (ConvertType(row[p.Name], p.PropertyType,isStoreDB, out obj))  
                    //{                                          
                    //    p.SetValue(entity, obj, null);  
                    //}                  
                }
                list.Add(entity);
            }
            return list;
        }

    }
}
