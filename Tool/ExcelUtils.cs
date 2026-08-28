using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Globalization;
using System.Text;

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
        public static DataSet ConvertExcelFileToDataSet(IFormFile file)
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

                // 5. 遍历所有Sheet转换为DataTable
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    if (sheet == null) continue;

                    // 创建DataTable（表名=Sheet名）
                    DataTable dataTable = new DataTable(sheet.SheetName);

                    // 读取表头（第一行作为列名）
                    IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                    if (headerRow == null) continue;

                    // 构建DataTable列
                    for (int col = 0; col < headerRow.LastCellNum; col++)
                    {
                        ICell cell = headerRow.GetCell(col);
                        string columnName = cell?.ToString() ?? $"列{col + 1}"; // 空表头默认命名
                        dataTable.Columns.Add(columnName);
                    }

                    // 读取数据行（跳过表头，从第二行开始）
                    for (int row = sheet.FirstRowNum + 1; row <= sheet.LastRowNum; row++)
                    {
                        IRow dataRow = sheet.GetRow(row);
                        if (dataRow == null) continue;

                        // 跳过空行（所有单元格都为空则忽略）
                        bool isEmptyRow = true;
                        for (int col = 0; col < headerRow.LastCellNum; col++)
                        {
                            ICell cell = dataRow.GetCell(col);
                            if (cell != null && !string.IsNullOrEmpty(cell.ToString()))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }
                        if (isEmptyRow) continue;

                        // 填充数据到DataRow
                        DataRow dtRow = dataTable.NewRow();
                        for (int col = 0; col < headerRow.LastCellNum; col++)
                        {
                            ICell cell = dataRow.GetCell(col);
                            dtRow[col] = cell?.ToString() ?? string.Empty; // 空单元格赋值为空字符串
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





        public static DataSet ConvertExcelFileToDataSetForAll(IFormFile file)
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

                    // 遍历所有Sheet
                    for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                    {
                        ISheet sheet = workbook.GetSheetAt(sheetIndex);
                        if (sheet == null) continue;

                        DataTable dataTable = new DataTable(sheet.SheetName);
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        if (headerRow == null) continue;

                        // 构建列
                        for (int col = 0; col < headerRow.LastCellNum; col++)
                        {
                            ICell cell = headerRow.GetCell(col);
                            string columnName = cell?.ToString() ?? $"列{col + 1}";
                            dataTable.Columns.Add(columnName);
                        }

                        // 读取数据行
                        for (int row = sheet.FirstRowNum + 1; row <= sheet.LastRowNum; row++)
                        {
                            IRow dataRow = sheet.GetRow(row);
                            if (dataRow == null) continue;

                            // 跳过空行
                            bool isEmptyRow = true;
                            for (int col = 0; col < headerRow.LastCellNum; col++)
                            {
                                ICell cell = dataRow.GetCell(col);
                                if (cell != null && !string.IsNullOrEmpty(cell.ToString()))
                                {
                                    isEmptyRow = false;
                                    break;
                                }
                            }
                            if (isEmptyRow) continue;

                            // 填充行数据
                            DataRow dtRow = dataTable.NewRow();
                            for (int col = 0; col < headerRow.LastCellNum; col++)
                            {
                                ICell cell = dataRow.GetCell(col);
                                dtRow[col] = cell?.ToString() ?? string.Empty;
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




}
