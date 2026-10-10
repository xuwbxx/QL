using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Tool
{
    public class ExcelUtils
    {

        /// <summary>
        /// 读取Excel文件并返回DataSet，每个sheet对应一个DataTable
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <param name="firstRowIsHeader">第一行是否为列名</param>
        /// <returns>包含所有sheet数据的DataSet</returns>
        public static DataSet ReadExcel(string filePath, bool firstRowIsHeader = true)
        {
            var dataSet = new DataSet();

            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    var dataTable = new DataTable(worksheet.Name);
                    var firstRow = 1;

                    // 如果第一行是列名，则读取列名
                    if (firstRowIsHeader)
                    {
                        var firstRowRange = worksheet.Range(1, 1, 1, worksheet.LastColumnUsed().ColumnNumber());
                        foreach (var cell in firstRowRange.FirstRow().Cells())
                        {
                            // 处理重复列名的情况
                            var columnName = cell.Value.ToString();
                            if (string.IsNullOrEmpty(columnName))
                            {
                                columnName = $"Column{dataTable.Columns.Count + 1}";
                            }

                            // 确保列名唯一
                            var uniqueColumnName = columnName;
                            var count = 1;
                            while (dataTable.Columns.Contains(uniqueColumnName))
                            {
                                uniqueColumnName = $"{columnName}{count++}";
                            }

                            dataTable.Columns.Add(uniqueColumnName);
                        }
                        firstRow = 2;
                    }
                    else
                    {
                        // 如果第一行不是列名，则使用默认列名
                        var lastColumn = worksheet.LastColumnUsed().ColumnNumber();
                        for (int i = 1; i <= lastColumn; i++)
                        {
                            dataTable.Columns.Add($"Column{i}");
                        }
                    }

                    // 读取数据行
                    for (int rowNum = firstRow; rowNum <= worksheet.LastRowUsed().RowNumber(); rowNum++)
                    {
                        var currentRow = worksheet.Row(rowNum);
                        var dataRow = dataTable.NewRow();

                        for (int colNum = 1; colNum <= dataTable.Columns.Count; colNum++)
                        {
                            try
                            {
                                dataRow[colNum - 1] = currentRow.Cell(colNum).Value.ToString();
                            }
                            catch (Exception ex)
                            {
                                // 处理异常，可以记录日志或设置默认值
                                dataRow[colNum - 1] = DBNull.Value;
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }

                    dataSet.Tables.Add(dataTable);
                }
            }

            return dataSet;
        }

        /// <summary>
        /// 将DataSet写入Excel文件
        /// </summary>
        /// <param name="dataSet">包含数据的DataSet</param>
        /// <param name="filePath">输出文件路径</param>
        /// <param name="firstRowIsHeader">是否将列名写入第一行</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool WriteExcel(DataSet dataSet, string filePath, bool firstRowIsHeader = true)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    foreach (DataTable table in dataSet.Tables)
                    {
                        var worksheet = workbook.Worksheets.Add(table.TableName);
                        var startRow = 1;

                        // 如果需要写入列名
                        if (firstRowIsHeader)
                        {
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                worksheet.Cell(1, i + 1).Value = table.Columns[i].ColumnName;
                            }
                            startRow = 2;
                        }

                        // 写入数据
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            for (int j = 0; j < table.Columns.Count; j++)
                            {
                                worksheet.Cell(i + startRow, j + 1).Value = table.Rows[i][j]?.ToString() ?? string.Empty;
                            }
                        }

                        // 自动调整列宽
                        worksheet.Columns().AdjustToContents();
                    }

                    // 保存文件
                    workbook.SaveAs(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                // 可以记录异常日志
                Console.WriteLine($"写入Excel文件时出错: {ex.Message}");
                return false;
            }
        }



        public static byte[] TableToBytes(DataTable dt, string sheetname)
        {
            XSSFWorkbook xssfworkbook = new XSSFWorkbook();
            ISheet sheet = xssfworkbook.CreateSheet(sheetname);

            //表头
            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(dt.Columns[i].ColumnName);
            }

            //数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row1 = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row1.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            //转为字节数组
            MemoryStream stream = new MemoryStream();
            xssfworkbook.Write(stream);
            var buf = stream.ToArray();

            //保存为Excel文件
            //using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            //{
            //    fs.Write(buf, 0, buf.Length);
            //    fs.Flush();
            //}

            return buf;
        }
        public static byte[] SetToBytes(DataSet dataSet, bool AddFirstColumn)
        {
            try
            {
                if (dataSet == null || dataSet.Tables == null || dataSet.Tables.Count == 0)
                    throw new Exception("输入的DataSet或路径异常");
                int sheetIndex = 0;
                //根据输出路径的扩展名判断workbook的实例类型
                IWorkbook workbook = new XSSFWorkbook();

                //将DataSet导出为Excel
                foreach (DataTable dt in dataSet.Tables)
                {
                    sheetIndex++;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ISheet sheet = workbook.CreateSheet(string.IsNullOrEmpty(dt.TableName) ? ("sheet" + sheetIndex) : dt.TableName);//创建一个名称为Sheet0的表
                        int rowCount = dt.Rows.Count;//行数
                        int columnCount = dt.Columns.Count;//列数

                        if (AddFirstColumn)
                        {
                            //设置列头
                            IRow row = sheet.CreateRow(0);//excel第一行设为列头
                            for (int c = 0; c < columnCount; c++)
                            {
                                ICell cell = row.CreateCell(c);
                                cell.SetCellValue(dt.Columns[c].ColumnName);
                            }

                            //设置每行每列的单元格,
                            for (int i = 0; i < rowCount; i++)
                            {
                                row = sheet.CreateRow(i + 1);
                                for (int j = 0; j < columnCount; j++)
                                {
                                    ICell cell = row.CreateCell(j);//excel第二行开始写入数据
                                    cell.SetCellValue(dt.Rows[i][j].ToString());
                                }
                            }
                        }
                        else
                        {
                            IRow row = null;
                            //设置每行每列的单元格,
                            for (int i = 0; i < rowCount; i++)
                            {
                                row = sheet.CreateRow(i);
                                for (int j = 0; j < columnCount; j++)
                                {
                                    ICell cell = row.CreateCell(j);//excel第二行开始写入数据
                                    cell.SetCellValue(dt.Rows[i][j].ToString());
                                }
                            }
                        }
                    }
                }

                //转为字节数组
                MemoryStream stream = new MemoryStream();
                workbook.Write(stream);
                var buf = stream.ToArray();
                return buf;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ExcelUtils));
                return null;
            }
        }


        /// <summary>
        /// 同步方法：将IFormFile（Excel）转换为DataSet
        /// </summary>
        /// <param name="file">上传的Excel文件</param>
        /// <returns>包含Excel所有sheet的DataSet</returns>
        /// <exception cref="ArgumentException">文件校验异常</exception>
        /// <exception cref="NotSupportedException">不支持的文件格式</exception>
        public static DataSet ConvertExcelFileToDataSet(IFormFile file, bool isFirstLineColumnName = false)
        {
            // 1. 基础校验
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("上传的文件为空或大小为0，请选择有效的Excel文件");
            }

            // 2. 校验文件格式
            var fileExt = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExt) || (fileExt != ".xlsx" && fileExt != ".xls"))
            {
                throw new NotSupportedException($"不支持的文件格式：{fileExt}，仅支持 .xlsx 或 .xls 格式");
            }

            DataSet dataSet = new DataSet();

            // 3. 同步读取文件流到内存流
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream); // 同步Copy，替代异步CopyToAsync
                memoryStream.Position = 0; // 重置流指针到起始位置

                // 4. 根据文件格式创建Workbook
                IWorkbook workbook = fileExt == ".xlsx" 
                    ? new XSSFWorkbook(memoryStream)  // .xlsx (2007+)
                    : (IWorkbook)new HSSFWorkbook(memoryStream); // .xls (2003-)
                
                // 4.1 计算公式，将公式单元格替换为计算结果
                var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                evaluator.EvaluateAll();

                // 5. 遍历所有Sheet转换为DataTable
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    if (sheet == null) continue;

                    // 创建DataTable（表名=Sheet名）
                    DataTable dataTable = new DataTable(sheet.SheetName);

                    IRow firstRow = sheet.GetRow(sheet.FirstRowNum);
                    if (firstRow == null) continue;

                    int colCount = firstRow.LastCellNum;
                    int dataStartRow = sheet.FirstRowNum;

                    // 构建DataTable列
                    if (isFirstLineColumnName)
                    {
                        // 第一行作为列名
                        for (int col = 0; col < colCount; col++)
                        {
                            ICell cell = firstRow.GetCell(col);
                            string columnName = GetCellValue(cell) ?? $"列{col + 1}";
                            dataTable.Columns.Add(columnName);
                        }
                        dataStartRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        // 默认列名
                        for (int col = 0; col < colCount; col++)
                        {
                            dataTable.Columns.Add($"列{col + 1}");
                        }
                    }

                    // 读取数据行
                    for (int row = dataStartRow; row <= sheet.LastRowNum; row++)
                    {
                        IRow dataRow = sheet.GetRow(row);
                        if (dataRow == null) continue;

                        // 跳过空行（所有单元格都为空则忽略）
                        bool isEmptyRow = true;
                        for (int col = 0; col < colCount; col++)
                        {
                            ICell cell = dataRow.GetCell(col);
                            if (cell != null && !string.IsNullOrEmpty(GetCellValue(cell)))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }
                        if (isEmptyRow) continue;

                        // 填充数据到DataRow
                        DataRow dtRow = dataTable.NewRow();
                        for (int col = 0; col < colCount; col++)
                        {
                            ICell cell = dataRow.GetCell(col);
                            dtRow[col] = GetCellValue(cell) ?? string.Empty; // 空单元格赋值为空字符串
                        }
                        dataTable.Rows.Add(dtRow);
                    }

                    // 将当前Sheet的DataTable加入DataSet
                    if (dataTable.Rows.Count > 0)
                    {
                        dataSet.Tables.Add(dataTable);
                    }
                }
            }

            return dataSet;
        }





        public static DataSet ConvertExcelFileToDataSetForAll(IFormFile file, bool isFirstLineColumnName = false)
        {
            // 1. 基础校验
            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("上传的文件为空或大小为0，请选择有效的Excel文件");
            }

            // 2. 校验文件格式（新增支持 .csv）
            var fileExt = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExt) ||
                (fileExt != ".xlsx" && fileExt != ".xls" && fileExt != ".csv"))
            {
                throw new NotSupportedException($"不支持的文件格式：{fileExt}，仅支持 .xlsx / .xls / .csv 格式");
            }

            DataSet dataSet = new DataSet();

            // 3. 读取文件流到内存流
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0; // 重置流指针

                // 4. 分格式处理：Excel / CSV
                if (fileExt == ".csv")
                {
                    // 处理 CSV 文件
                    DataTable csvTable = ConvertCsvToDataTable(memoryStream);
                    dataSet.Tables.Add(csvTable);
                }
                else
                {
                    // 处理 Excel 文件（.xlsx / .xls）
                    IWorkbook workbook = fileExt == ".xlsx"
                        ? new XSSFWorkbook(memoryStream)
                        : (IWorkbook)new HSSFWorkbook(memoryStream);

                    // 计算公式，将公式单元格替换为计算结果
                    var evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                    evaluator.EvaluateAll();

                    // 遍历所有Sheet
                    for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                    {
                        ISheet sheet = workbook.GetSheetAt(sheetIndex);
                        if (sheet == null) continue;

                        DataTable dataTable = new DataTable(sheet.SheetName);
                        IRow firstRow = sheet.GetRow(sheet.FirstRowNum);
                        if (firstRow == null) continue;

                        int colCount = firstRow.LastCellNum;
                        int dataStartRow = sheet.FirstRowNum;

                        // 构建列
                        if (isFirstLineColumnName)
                        {
                            for (int col = 0; col < colCount; col++)
                            {
                                ICell cell = firstRow.GetCell(col);
                                string columnName = GetCellValue(cell) ?? $"列{col + 1}";
                                dataTable.Columns.Add(columnName);
                            }
                            dataStartRow = sheet.FirstRowNum + 1;
                        }
                        else
                        {
                            for (int col = 0; col < colCount; col++)
                            {
                                dataTable.Columns.Add($"列{col + 1}");
                            }
                        }

                        // 读取数据行
                        for (int row = dataStartRow; row <= sheet.LastRowNum; row++)
                        {
                            IRow dataRow = sheet.GetRow(row);
                            if (dataRow == null) continue;

                            // 跳过空行
                            bool isEmptyRow = true;
                            for (int col = 0; col < colCount; col++)
                            {
                                ICell cell = dataRow.GetCell(col);
                                if (cell != null && !string.IsNullOrEmpty(GetCellValue(cell)))
                                {
                                    isEmptyRow = false;
                                    break;
                                }
                            }
                            if (isEmptyRow) continue;

                            // 填充行数据
                            DataRow dtRow = dataTable.NewRow();
                            for (int col = 0; col < colCount; col++)
                            {
                                ICell cell = dataRow.GetCell(col);
                                dtRow[col] = GetCellValue(cell) ?? string.Empty;
                            }
                            dataTable.Rows.Add(dtRow);
                        }

                        if (dataTable.Rows.Count > 0)
                        {
                            dataSet.Tables.Add(dataTable);
                        }
                    }
                }
            }

            return dataSet;
        }

        /// <summary>
        /// CSV 专用转换方法（自动处理逗号分隔、引号包裹、空行、中文编码）
        /// </summary>
        private static DataTable ConvertCsvToDataTable(Stream csvStream)
        {
            DataTable dt = new DataTable("CSV数据");
            // 优先尝试 UTF8 编码，乱码可替换为 Encoding.GetEncoding("GB2312")
            using (var reader = new StreamReader(csvStream, Encoding.UTF8))
            {
                string line;
                bool isFirstRow = true;

                while ((line = reader.ReadLine()) != null)
                {
                    // 跳过空行
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 解析CSV行（处理带逗号、带引号的字段）
                    string[] fields = ParseCsvLine(line);

                    // 第一行作为表头
                    if (isFirstRow)
                    {
                        foreach (string field in fields)
                        {
                            dt.Columns.Add(string.IsNullOrEmpty(field) ? $"列{dt.Columns.Count + 1}" : field);
                        }
                        isFirstRow = false;
                        continue;
                    }

                    // 数据行
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < fields.Length && i < dt.Columns.Count; i++)
                    {
                        row[i] = fields[i] ?? string.Empty;
                    }
                    dt.Rows.Add(row);
                }
            }

            return dt;
        }

        /// <summary>
        /// 安全解析CSV单行数据（支持引号内逗号、引号转义）
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // 添加最后一个字段
            fields.Add(currentField.ToString().Trim());
            return fields.ToArray();
        }



        public static bool DataSetToExcel(DataSet dataSet, string Outpath, bool AddFirstColumn)
        {
            bool result = false;
            try
            {
                if (dataSet == null || dataSet.Tables == null || dataSet.Tables.Count == 0 || string.IsNullOrEmpty(Outpath))
                    throw new Exception("输入的DataSet或路径异常");
                int sheetIndex = 0;
                //根据输出路径的扩展名判断workbook的实例类型
                IWorkbook workbook = null;
                string pathExtensionName = Outpath.Trim().Substring(Outpath.Length - 5);
                if (pathExtensionName.Contains(".xlsx"))
                {
                    workbook = new XSSFWorkbook();
                }
                else if (pathExtensionName.Contains(".xls"))
                {
                    workbook = new HSSFWorkbook();
                }
                else
                {
                    Outpath = Outpath.Trim() + ".xls";
                    workbook = new HSSFWorkbook();
                }
                //将DataSet导出为Excel
                foreach (DataTable dt in dataSet.Tables)
                {
                    sheetIndex++;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ISheet sheet = workbook.CreateSheet(string.IsNullOrEmpty(dt.TableName) ? ("sheet" + sheetIndex) : dt.TableName);//创建一个名称为Sheet0的表
                        int rowCount = dt.Rows.Count;//行数
                        int columnCount = dt.Columns.Count;//列数

                        if (AddFirstColumn)
                        {
                            //设置列头
                            IRow row = sheet.CreateRow(0);//excel第一行设为列头
                            for (int c = 0; c < columnCount; c++)
                            {
                                ICell cell = row.CreateCell(c);
                                cell.SetCellValue(dt.Columns[c].ColumnName);
                            }

                            //设置每行每列的单元格,
                            for (int i = 0; i < rowCount; i++)
                            {
                                row = sheet.CreateRow(i + 1);
                                for (int j = 0; j < columnCount; j++)
                                {
                                    ICell cell = row.CreateCell(j);//excel第二行开始写入数据
                                    cell.SetCellValue(dt.Rows[i][j].ToString());
                                }
                            }
                        }
                        else
                        {
                            IRow row = null;
                            //设置每行每列的单元格,
                            for (int i = 0; i < rowCount; i++)
                            {
                                row = sheet.CreateRow(i);
                                for (int j = 0; j < columnCount; j++)
                                {
                                    ICell cell = row.CreateCell(j);//excel第二行开始写入数据
                                    cell.SetCellValue(dt.Rows[i][j].ToString());
                                }
                            }
                        }
                    }
                }
                //向outPath输出数据
                using (FileStream fs = File.OpenWrite(Outpath))
                {
                    workbook.Write(fs);//向打开的这个xls文件中写入数据
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ExcelUtils));
                return false;
            }
        }


        public static void SaveCSV(DataTable dt, string filePath)//table数据写入csv
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);

            //写入列名
            string data = "";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            //写入各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                    if (str.Contains(',') || str.Contains('"')
                      || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                    {
                        str = string.Format("\"{0}\"", str);
                    }

                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 将Excel文件中的数据读出到DataTable中(xlsx)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DataSet ExcelToTableForXLSX(string file)
        {
            DataSet ds = new DataSet();
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                XSSFWorkbook xssfworkbook = new XSSFWorkbook(fs);

                int sheetcount = xssfworkbook.Count;

                for (int k = 0; k < sheetcount; k++)
                {
                    DataTable dt = new DataTable();

                    ISheet sheet = xssfworkbook.GetSheetAt(k);

                    dt.TableName = sheet.SheetName;



                    //表头
                    IRow header = sheet.GetRow(sheet.FirstRowNum);
                    if (header == null)
                        continue;
                    List<int> columns = new List<int>();
                    for (int i = 0; i < header.LastCellNum; i++)
                    {
                        object obj = GetValueTypeForXLSX(header.GetCell(i) as XSSFCell);
                        if (obj == null || obj.ToString() == string.Empty)
                        {
                            dt.Columns.Add(new DataColumn("Columns" + i.ToString()));
                            //continue;
                        }
                        else
                            dt.Columns.Add(new DataColumn(obj.ToString()));
                        columns.Add(i);
                    }
                    //数据
                    for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
                    {
                        DataRow dr = dt.NewRow();
                        bool hasValue = false;
                        foreach (int j in columns)
                        {
                            if (sheet.GetRow(i) != null)
                            {
                                dr[j] = GetValueTypeForXLSX(sheet.GetRow(i).GetCell(j) as XSSFCell);
                                if (dr[j] != null && dr[j].ToString() != string.Empty)
                                {
                                    hasValue = true;
                                }
                            }

                        }
                        if (hasValue)
                        {
                            dt.Rows.Add(dr);
                        }
                    }

                    ds.Tables.Add(dt);

                }


            }
            return ds;
        }

        /// <summary>
        /// 获取单元格类型(xlsx)
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static object GetValueTypeForXLSX(XSSFCell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:
                    return null;
                case CellType.Boolean: //BOOLEAN:
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:
                    return cell.NumericCellValue;
                case CellType.String: //STRING:
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:
                default:
                    return "=" + cell.CellFormula;
            }
        }


        /// <summary>
        /// 获取单元格值，支持公式单元格（读取缓存计算结果）
        /// </summary>
        private static string GetCellValue(ICell cell)
        {
            if (cell == null) return null;

            switch (cell.CellType)
            {
                case CellType.Blank:
                    return "";
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    return cell.NumericCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                                return cell.DateCellValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            return cell.NumericCellValue.ToString();
                        case CellType.String:
                            return cell.StringCellValue;
                        case CellType.Boolean:
                            return cell.BooleanCellValue.ToString();
                        default:
                            return "";
                    }
                default:
                    return "";
            }
        }


        #region 导入/导出通用能力（业务无关，供各业务模块复用）

        /// <summary>单个工作表允许的最大数据行数</summary>
        public const int MaxDataRows = 50_000;

        /// <summary>上传文件允许的最大字节数（20 MB）</summary>
        public const long MaxFileBytes = 20L * 1024 * 1024;

        /// <summary>
        /// 导入过程中的错误信息收集回调。通用方法只负责"发现问题"，
        /// 由调用方决定如何落地（业务侧通常转成自己的错误类型）。
        /// row=Excel行号；rowKey=行标识（业务侧多为测点编号）；field=字段名；originalValue=原始值；reason=失败原因
        /// </summary>
        public delegate void ExcelErrorSink(int row, string rowKey, string field, string originalValue, string reason);

        /// <summary>打开工作簿（ClosedXML）。</summary>
        public static XLWorkbook OpenWorkbook(Stream stream) => new XLWorkbook(stream);

        /// <summary>取第一个工作表：导入统一只认第一个工作表。</summary>
        public static IXLWorksheet FirstWorksheet(XLWorkbook workbook) => workbook.Worksheet(1);

        /// <summary>
        /// 读取首行表头，按需校验必需列是否齐全；返回「列名 -> 列号」映射，缺列时通过 addError 上报。
        /// lang 用于把通用校验文案（缺少必需列等）输出成对应语言。
        /// </summary>
        public static Dictionary<string, int> ReadHeaderMap(IXLWorksheet sheet, string[] required, string? lang, ExcelErrorSink addError)
        {
            var allColumns = new Dictionary<string, int>(StringComparer.Ordinal);
            var lastColumn = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (var column = 1; column <= lastColumn; column++)
            {
                var name = sheet.Cell(1, column).GetString();
                if (!string.IsNullOrEmpty(name) && !allColumns.ContainsKey(name)) allColumns[name] = column;
            }
            var map = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var name in required)
            {
                if (!allColumns.TryGetValue(name, out var column)) addError(1, string.Empty, name, string.Empty, MsgMissingColumn(lang, name));
                else map[name] = column;
            }
            return map;
        }

        /// <summary>整行是否为空（只看给定列）。</summary>
        public static bool IsEmptyRow(IXLWorksheet sheet, int row, IEnumerable<int> columns) => columns.All(c => sheet.Cell(row, c).IsEmpty());

        /// <summary>统计区间内非空数据行数。</summary>
        public static int CountNonEmptyRows(IXLWorksheet sheet, int first, int last, IEnumerable<int> columns)
        {
            var count = 0;
            for (var row = first; row <= last; row++) if (!IsEmptyRow(sheet, row, columns)) count++;
            return count;
        }

        /// <summary>读取单元格文本；公式单元格按空处理（导入不允许公式）。</summary>
        public static string ReadText(IXLCell cell) => cell.HasFormula ? string.Empty : cell.GetString();

        /// <summary>校验该行没有使用公式。</summary>
        public static void ValidateNoFormula(IXLWorksheet sheet, int row, IDictionary<string, int> columns, string rowKey, string? lang, ExcelErrorSink addError)
        {
            foreach (var pair in columns)
            {
                var cell = sheet.Cell(row, pair.Value);
                if (cell.HasFormula) addError(row, rowKey, pair.Key, cell.FormulaA1, MsgFormula(lang));
            }
        }

        /// <summary>
        /// 读取数值单元格：空值/非数字/越界都会上报，随后按 scale 四舍五入（不再因小数位数报错）。
        /// </summary>
        public static decimal? ReadDecimal(IXLCell cell, int row, string rowKey, string field, int scale, decimal? min, decimal? max, string? lang, ExcelErrorSink addError)
        {
            var raw = cell.GetFormattedString();
            if (cell.IsEmpty())
            {
                addError(row, rowKey, field, raw, MsgRequired(lang, field));
                return null;
            }
            decimal value;
            if (cell.DataType == XLDataType.Number) value = Convert.ToDecimal(cell.GetDouble(), CultureInfo.InvariantCulture);
            else if (!decimal.TryParse(cell.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                addError(row, rowKey, field, raw, MsgNumber(lang, field));
                return null;
            }
            // [Shen, Lin] 不再验证小数位数，而是自动四舍五入为指定精度
            // if (!HasScale(value, scale)) addError(row, rowKey, field, raw, $"{field}最多保留{scale}位小数");
            value = Math.Round(value, scale, MidpointRounding.AwayFromZero);
            if (min.HasValue && value < min.Value || max.HasValue && value > max.Value)
                addError(row, rowKey, field, raw, MsgRange(lang, field, min, max));
            return value;
        }

        /// <summary>校验必填文本：非空且不超过最大长度。</summary>
        public static void ValidateRequiredText(int row, string rowKey, string field, string value, int maxLength, string? lang, ExcelErrorSink addError)
        {
            if (string.IsNullOrWhiteSpace(value)) addError(row, rowKey, field, value, MsgRequired(lang, field));
            else if (value.Length > maxLength) addError(row, rowKey, field, value, MsgMaxLength(lang, field, maxLength));
        }

        // 通用校验文案：措辞与业务无关（只含由调用方传入的字段名/列名等），故留在通用工具类里；
        // 业务专属文案在 Service 侧的 BizMessages。
        private static bool Zh(string? lang) =>
            string.IsNullOrWhiteSpace(lang) || lang!.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        private static string MsgMissingColumn(string? lang, string column) => Zh(lang)
            ? $"缺少必需列：{column}"
            : $"Missing required column: {column}";
        private static string MsgFormula(string? lang) => Zh(lang)
            ? "导入字段不允许使用公式"
            : "Formulas are not allowed in imported fields";
        private static string MsgRequired(string? lang, string field) => Zh(lang) ? $"{field}不能为空" : $"{field} is required";
        private static string MsgNumber(string? lang, string field) => Zh(lang) ? $"{field}必须是有效数字" : $"{field} must be a valid number";
        private static string MsgRange(string? lang, string field, decimal? min, decimal? max) => Zh(lang)
            ? $"{field}必须在{min}到{max}之间"
            : $"{field} must be between {min} and {max}";
        private static string MsgMaxLength(string? lang, string field, int maxLength) => Zh(lang)
            ? $"{field}不能超过{maxLength}个字符"
            : $"{field} must not exceed {maxLength} characters";

        /// <summary>
        /// 生成导入模板：第一个工作表为「数据」（仅表头），第二个为「填写说明」（字段/必填/规则/示例）。
        /// </summary>
        public static byte[] CreateTemplate(string[] headers, string[][] instructions)
        {
            using var workbook = new XLWorkbook();
            var data = workbook.AddWorksheet("数据");
            for (var i = 0; i < headers.Length; i++) data.Cell(1, i + 1).Value = headers[i];
            StyleSheet(data, headers.Length);
            data.SheetView.FreezeRows(1);
            var help = workbook.AddWorksheet("填写说明");
            var helpHeaders = new[] { "字段", "必填", "规则", "示例" };
            for (var i = 0; i < helpHeaders.Length; i++) help.Cell(1, i + 1).Value = helpHeaders[i];
            for (var r = 0; r < instructions.Length; r++)
                for (var c = 0; c < instructions[r].Length; c++) help.Cell(r + 2, c + 1).Value = instructions[r][c];
            StyleSheet(help, helpHeaders.Length);
            return SaveToBytes(workbook);
        }

        /// <summary>生成单表工作簿（表头 + 若干行数据），用于错误明细之类的导出。</summary>
        public static byte[] CreateTableWorkbook(string sheetName, string[] headers, IEnumerable<string[]> rows)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.AddWorksheet(sheetName);
            for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
            var row = 2;
            foreach (var values in rows)
            {
                for (var i = 0; i < headers.Length && i < values.Length; i++) sheet.Cell(row, i + 1).Value = values[i];
                row++;
            }
            StyleSheet(sheet, headers.Length);
            return SaveToBytes(workbook);
        }

        /// <summary>表头加粗 + 底色居中，自动列宽，整表加边框。</summary>
        public static void StyleSheet(IXLWorksheet sheet, int columns)
        {
            var header = sheet.Range(1, 1, 1, columns);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FF");
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Columns(1, columns).AdjustToContents(12, 50);
            sheet.RangeUsed()?.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            sheet.RangeUsed()?.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        /// <summary>工作簿导出为字节数组。</summary>
        public static byte[] SaveToBytes(XLWorkbook workbook)
        {
            using var output = new MemoryStream();
            workbook.SaveAs(output);
            return output.ToArray();
        }

        /// <summary>把任意文本清洗成可用于文件名/路径片段的安全串。</summary>
        public static string SanitizePathPart(string value)
        {
            var sanitized = Regex.Replace(value ?? string.Empty, @"[\x00-\x1F<>:""/\\|?*]", "_").Trim().TrimEnd('.');
            return string.IsNullOrWhiteSpace(sanitized) ? "未命名" : sanitized;
        }

        /// <summary>判断 decimal 的小数位数是否不超过 maximumScale。</summary>
        public static bool HasScale(decimal value, int maximumScale)
        {
            var bits = decimal.GetBits(value);
            var scale = (bits[3] >> 16) & 0x7F;
            return scale <= maximumScale;
        }

        #endregion


    }

    public class CsvService<T> where T : class
    {
        public static List<T> ReadCsvFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            return csv.GetRecords<T>().ToList();
        }

        public static void WriteCsvFile(string filePath, List<T> data)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(data);
        }
    }

    public class CSVhelper
    {

        public static List<string[]> ReadCsvFile(string filePath)
        {
            List<string[]> data = new List<string[]>();

            // 使用TextFieldParser处理CSV文件，能正确处理带引号的字段
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
                parser.SetDelimiters(","); // 设置分隔符为逗号
                parser.HasFieldsEnclosedInQuotes = true; // 处理带引号的字段

                // 读取所有行
                while (!parser.EndOfData)
                {
                    try
                    {
                        // 读取一行并分割为字段
                        string[] fields = parser.ReadFields();
                        if (fields != null)
                        {
                            data.Add(fields);
                        }
                    }
                    catch (MalformedLineException ex)
                    {
                        return new List<string[]>();
                        //Console.WriteLine($"警告：行 {parser.LineNumber} 格式不正确，已跳过。错误：{ex.Message}");
                    }
                }
            }

            return data;
        }

    }




}
