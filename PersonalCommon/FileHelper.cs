using Aspose.Words;
using Aspose.Words.Saving;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
/// <summary>
/// 文件操作类
/// </summary>
namespace PersonalCommon
{
    public static class FileHelper
    {
        /// <summary>
        /// 判断远程文件是否存在 
        /// </summary>
        /// <param name="fileUrl">远程文件路径，包括IP地址以及详细的路径</param>
        /// <returns></returns>
        public static bool RemoteFileExists(string fileUrl)
        {
            bool result = false;//下载结果
            WebResponse response = null;
            try
            {
                WebRequest req = WebRequest.Create(fileUrl);
                response = req.GetResponse();
                result = response == null ? false : true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return result;
        }

        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FileExists(string path, string name)
        {
            if (Directory.Exists(path) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(path);
            }
            //判断文件的存在
            if (!File.Exists(path + "/" + name))
            {
                HSSFWorkbook hssfworkbook = new HSSFWorkbook();
                //创建Excel工作表  
                var sheet1 = hssfworkbook.CreateSheet("Sheet0");
                //保存  
                FileStream file = new FileStream(path + "/" + name, FileMode.Create);
                hssfworkbook.Write(file);
                file.Close();
            }
            return path + "/" + name;
        }
        #endregion

        #region Excel文件导出 ,不需要模板
        /// <summary>
        /// 文件导出,不需要模板
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="title">文件标题</param>
        /// <param name="head">表头</param>
        /// <param name="list">内容</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static MemoryStream StatisticalReport_Exl<T>(string title, List<string> head, List<T> list, string fileName) {
            IWorkbook workbook = null;
            string fileExt = Path.GetExtension(fileName).ToLower();
            if (fileExt == ".xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (fileExt == ".xls")
            {
                workbook = new HSSFWorkbook();
            }
            else { workbook = null; }
            if (workbook == null)
            {
                return new MemoryStream(); ;
            }
            ISheet sheet = workbook.CreateSheet("Sheet0");

            var titlestyle = GetDefaultStyle(workbook);
            var titlefont = workbook.CreateFont();
            titlefont.Boldweight = short.MaxValue;
            titlefont.FontHeightInPoints = 14;
            titlestyle.SetFont(titlefont);
            titlestyle.Alignment = HorizontalAlignment.Center;//水平对齐

            var indexcellstyle = GetDefaultStyle(workbook);
            var cellfont = workbook.CreateFont();
            cellfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfont.FontHeightInPoints = 10;
            indexcellstyle.SetFont(cellfont);

            var valuecellstyle = GetDefaultStyle(workbook);
            var cellfontv = workbook.CreateFont();
            cellfontv.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfontv.FontHeightInPoints = 10;
            valuecellstyle.Alignment = HorizontalAlignment.Center;
            valuecellstyle.SetFont(cellfontv);

            int valueIndex = 0;
            //标题
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            CreatAddCell(row, title, titlestyle, 0);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, head.Count-1));

            //表头
            int rowHead = 1;
            IRow rowhead = sheet.CreateRow(rowHead);
            foreach (string cellhead in head)
            {
                CreatAddCell(rowhead, cellhead, indexcellstyle, valueIndex);
                valueIndex++;
            }

            DataTable dt = TypeHelper.ListToDataTable<T>(list);
            //内容绑定
            int rowIndex = 2;
            foreach (DataRow dataRow in dt.Rows)
            {
                IRow rowContent = sheet.CreateRow(rowIndex);
                valueIndex = 0;
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    var value = dataRow[i];
                    if (value == null)
                    {
                        value = "";
                    }
                    else
                    {
                        value = value.GetType() == typeof(DateTime) ? Convert.ToDateTime(value).ToString("yyyy-MM-dd") : value.ToString();
                    }
                    CreatAddCell(rowContent, value.ToString(), valuecellstyle, valueIndex);
                    valueIndex++;
                }
                rowIndex++;
            }

            NpoiMemoryStream ms = new NpoiMemoryStream();
            ms.AllowClose = false;
            workbook.Write(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            ms.AllowClose = true;
            return ms;
        }
        /// <summary>
        /// 文件导出,不需要模板
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="title">文件标题</param>
        /// <param name="head">表头</param>
        /// <param name="dt">内容</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static MemoryStream StatisticalReport_Exl(string title, List<string> head, DataTable dt, string fileName)
        {
            IWorkbook workbook = null;
            string fileExt = Path.GetExtension(fileName).ToLower();
            if (fileExt == ".xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (fileExt == ".xls")
            {
                workbook = new HSSFWorkbook();
            }
            else { workbook = null; }
            if (workbook == null)
            {
                return new MemoryStream(); ;
            }
            ISheet sheet = workbook.CreateSheet("Sheet0");

            var titlestyle = GetDefaultStyle(workbook);
            var titlefont = workbook.CreateFont();
            titlefont.Boldweight = short.MaxValue;
            titlefont.FontHeightInPoints = 14;
            titlestyle.SetFont(titlefont);
            titlestyle.Alignment = HorizontalAlignment.Center;//水平对齐

            var indexcellstyle = GetDefaultStyle(workbook);
            var cellfont = workbook.CreateFont();
            cellfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfont.FontHeightInPoints = 10;
            indexcellstyle.SetFont(cellfont);

            var valuecellstyle = GetDefaultStyle(workbook);
            var cellfontv = workbook.CreateFont();
            cellfontv.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfontv.FontHeightInPoints = 10;
            valuecellstyle.Alignment = HorizontalAlignment.Center;
            valuecellstyle.SetFont(cellfontv);

            int valueIndex = 0;
            //标题
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            CreatAddCell(row, title, titlestyle, 0);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, head.Count-1));

            //表头
            int rowHead = 1;
            IRow rowhead = sheet.CreateRow(rowHead);
            foreach (string cellhead in head)
            {
                CreatAddCell(rowhead, cellhead, indexcellstyle, valueIndex);
                valueIndex++;
            }

            //内容绑定
            int rowIndex = 2;
            foreach (DataRow dataRow in dt.Rows)
            {
                IRow rowContent = sheet.CreateRow(rowIndex);
                valueIndex = 0;
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    var value = dataRow[i];
                    if (value == null)
                    {
                        value = "";
                    }
                    else
                    {
                        value = value.GetType() == typeof(DateTime) ? Convert.ToDateTime(value).ToString("yyyy-MM-dd") : value.ToString();
                    }
                    CreatAddCell(rowContent, value.ToString(), valuecellstyle, valueIndex);
                    valueIndex++;
                }
                rowIndex++;
            }

            NpoiMemoryStream ms = new NpoiMemoryStream();
            ms.AllowClose = false;
            workbook.Write(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            ms.AllowClose = true;
            return ms;
        }
        /// <summary>
        /// 文件导出,不需要模板
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="title">文件标题</param>
        /// <param name="head">表头</param>
        /// <param name="datalist">内容</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static MemoryStream StatisticalReport_Exl<T>(string title, List<string> head, IQueryable<T> datalist, string fileName)
        {
            IWorkbook workbook = null;
            string fileExt = Path.GetExtension(fileName).ToLower();
            if (fileExt == ".xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (fileExt == ".xls")
            {
                workbook = new HSSFWorkbook();
            }
            else { workbook = null; }
            if (workbook == null)
            {
                return new MemoryStream(); ;
            }
            ISheet sheet = workbook.CreateSheet("Sheet0");

            var titlestyle = GetDefaultStyle(workbook);
            var titlefont = workbook.CreateFont();
            titlefont.Boldweight = short.MaxValue;
            titlefont.FontHeightInPoints = 14;
            titlestyle.SetFont(titlefont);
            titlestyle.Alignment = HorizontalAlignment.Center;//水平对齐

            var indexcellstyle = GetDefaultStyle(workbook);
            var cellfont = workbook.CreateFont();
            cellfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfont.FontHeightInPoints = 10;
            indexcellstyle.SetFont(cellfont);

            var valuecellstyle = GetDefaultStyle(workbook);
            var cellfontv = workbook.CreateFont();
            cellfontv.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            cellfontv.FontHeightInPoints = 10;
            valuecellstyle.Alignment = HorizontalAlignment.Center;
            valuecellstyle.SetFont(cellfontv);

            DataTable dt = datalist.ToDataTable(rec => new object[] { datalist });
            int valueIndex = 0;
            //标题
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            CreatAddCell(row, title, titlestyle, 0);
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, head.Count-1));

            //表头
            int rowHead = 1;
            IRow rowhead = sheet.CreateRow(rowHead);
            foreach (string cellhead in head)
            {
                CreatAddCell(rowhead, cellhead, indexcellstyle, valueIndex);
                valueIndex++;
            }

            //内容绑定
            int rowIndex = 2;
            foreach (DataRow dataRow in dt.Rows)
            {
                IRow rowContent = sheet.CreateRow(rowIndex);
                valueIndex = 0;
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    var value = dataRow[i];
                    if (value == null)
                    {
                        value = "";
                    }
                    else
                    {
                        value = value.GetType() == typeof(DateTime) ? Convert.ToDateTime(value).ToString("yyyy-MM-dd") : value.ToString();
                    }
                    CreatAddCell(rowContent, value.ToString(), valuecellstyle, valueIndex);
                    valueIndex++;
                }
                rowIndex++;
            }

            NpoiMemoryStream ms = new NpoiMemoryStream();
            ms.AllowClose = false;
            workbook.Write(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            ms.AllowClose = true;
            return ms;
        }

        private static void CreatAddCell(NPOI.SS.UserModel.IRow dataRow, string value, NPOI.SS.UserModel.ICellStyle style, int col)
        {
            var cell = dataRow.CreateCell(col);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static ICellStyle GetDefaultStyle(IWorkbook workbook)
        {
            var cs = workbook.CreateCellStyle();
            cs.BorderBottom = BorderStyle.Thin;
            cs.BorderLeft = BorderStyle.Thin;
            cs.BorderRight = BorderStyle.Thin;
            cs.BorderTop = BorderStyle.Thin;
            cs.Alignment = HorizontalAlignment.Center;
            cs.VerticalAlignment = VerticalAlignment.Center;
            var cellfont = workbook.CreateFont();
            cellfont.Boldweight = (short)FontBoldWeight.Normal;
            cs.SetFont(cellfont);
            return cs;
        }
        #endregion Excel文件导出 

        #region Excel文件导出，引入模板
        /// <summary>
        /// Excel文件导出，引入模板
        /// </summary>
        /// <param name="dt">数据集</param>
        /// <param name="TempletFileName">模板文件</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBody">内容开始行索引</param>
        /// <returns></returns>
        public static MemoryStream DataTableToExcel(DataTable dt, string TempletFileName, string sheetName,int rowBody)
        {
            try
            {
                FileStream file = new FileStream(TempletFileName, FileMode.Open, FileAccess.Read);
                HSSFWorkbook hssfworkbook = new HSSFWorkbook(file);
                HSSFSheet ws = (HSSFSheet)hssfworkbook.GetSheet(sheetName);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int row = dt.Rows.Count;
                    int cell = dt.Columns.Count;
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < cell; j++)
                        {
                            /*
                            CELL_TYPE_NUMERIC 数值型 0
                            CELL_TYPE_STRING 字符串型    1
                            CELL_TYPE_FORMULA 公式型 2
                            CELL_TYPE_BLANK 空值  3
                            CELL_TYPE_BOOLEAN 布尔型 4
                            CELL_TYPE_ERROR 错误
                            */
                            if (dt.Columns[j].DataType == typeof(decimal))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.Numeric);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToDouble(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.String);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToDateTime(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(bool))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.Boolean);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToBoolean(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(byte))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.String);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToByte(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(Int16))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.Numeric);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToInt16(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(Int32))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.Numeric);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToInt32(dt.Rows[i][j]));
                            }
                            else if (dt.Columns[j].DataType == typeof(Int64))
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.Numeric);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(Convert.ToInt64(dt.Rows[i][j]));
                            }
                            else
                            {
                                ws.GetRow(rowBody + i).GetCell(j).SetCellType(CellType.String);
                                ws.GetRow(rowBody + i).GetCell(j).SetCellValue(dt.Rows[i][j].ToString());
                            }
                        }
                    }
                }
                ws.ForceFormulaRecalculation = true;
                NpoiMemoryStream ms = new NpoiMemoryStream();
                ms.AllowClose = false;
                hssfworkbook.Write(ms);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                ms.AllowClose = true;
                return ms;
            }
            catch (Exception ex)
            {
                return new MemoryStream();
            }
        }
        /// <summary>
        /// Excel文件导出，引入模板
        /// </summary>
        /// <param name="list">数据集</param>
        /// <param name="TempletFileName">模板文件</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBody">内容开始行索引</param>
        /// <returns></returns>
        public static MemoryStream DataTableToExcel<T>(List<T> list, string TempletFileName, string sheetName, int rowBody)
        {
            return DataTableToExcel(TypeHelper.ListToDataTable(list),TempletFileName,sheetName,rowBody);
        }
        #endregion

        /// <summary>  
        /// 将excel导入到datatable  
        /// </summary>  
        /// <param name="filePath">excel路径</param>  
        /// <param name="isColumnName">第一行是否是列名</param>  
        /// <returns>返回datatable</returns>  
        public static DataTable ExcelToDataTable(string filePath, bool isColumnName)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = File.OpenRead(filePath))
                {
                    // 2007版本  
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本  
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0);//读取第一个sheet，当然也可以循环读取每个sheet  
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数  
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行  
                                int cellCount = firstRow.LastCellNum;//列数  

                                //构建datatable的列  
                                if (isColumnName)
                                {
                                    startRow = 1;//如果第一行是列名，则从第二行开始读取  
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        dataTable.Columns.Add(column);
                                    }
                                }

                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                return null;
            }
        }

        /// <summary>
        /// 将DataTable导入到excel
        /// </summary>
        /// <param name="dt">数据集</param>
        /// <param name="saveFile">文件保存路径 例如：D:/myxls.xls</param>
        /// <returns></returns>
        public static bool DataTableToExcel(DataTable dt,string saveFile)
        {
            bool result = false;
            IWorkbook workbook = null;
            FileStream fs = null;
            IRow row = null;
            ISheet sheet = null;
            ICell cell = null;
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    workbook = new HSSFWorkbook();
                    sheet = workbook.CreateSheet("Sheet0");//创建一个名称为Sheet0的表  
                    int rowCount = dt.Rows.Count;//行数  
                    int columnCount = dt.Columns.Count;//列数  

                    //设置列头  
                    row = sheet.CreateRow(0);//excel第一行设为列头  
                    for (int c = 0; c < columnCount; c++)
                    {
                        cell = row.CreateCell(c);
                        cell.SetCellValue(dt.Columns[c].ColumnName);
                    }

                    //设置每行每列的单元格,  
                    for (int i = 0; i < rowCount; i++)
                    {
                        row = sheet.CreateRow(i + 1);
                        for (int j = 0; j < columnCount; j++)
                        {
                            cell = row.CreateCell(j);//excel第二行开始写入数据  
                            cell.SetCellValue(dt.Rows[i][j].ToString());
                        }
                    }
                    using (fs = File.OpenWrite(saveFile))
                    {
                        workbook.Write(fs);//向打开的这个xls文件中写入数据  
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                return false;
            }
        }

        

        #region Word文件导出
        /// <summary>
        /// Word文件填充数据(书签)后导出
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="dictBookMark">书签数据</param>
        /// <returns></returns>
        public static byte[] Report_Word(string path, Dictionary<string, string> dictBookMark)
        {
            //载入模板
            var doc = new Document(path);
            //书签绑定值，用于单个字段
            //Dictionary<string, string> dictBookMark = new Dictionary<string, string>();
            dictBookMark.Add("MedicinalName", "品名-山药");
            foreach (string name in dictBookMark.Keys)
            {
                var bookmark = doc.Range.Bookmarks[name];
                if (bookmark != null)
                {
                    bookmark.Text = dictBookMark[name];
                }
            }
            //在MVC中采用,保存文档到流中，使用base.File输出该文件
            var docStream = new MemoryStream();
            doc.Save(docStream, SaveOptions.CreateSaveOptions(SaveFormat.Doc));
            //return File(docStream.ToArray(), "application/msword", "Template.doc");
            return docStream.ToArray();
        }
        #endregion
    }

    #region Model
    /// <summary>
    /// Excel导出用
    /// </summary>
    public class NpoiMemoryStream : MemoryStream
    {
        public NpoiMemoryStream()
        {
            AllowClose = true;
        }

        public bool AllowClose { get; set; }

        public override void Close()
        {
            if (AllowClose)
                base.Close();
        }
    }
    #endregion Model
}