using System.Data;
using System.Text.RegularExpressions;
using Tool;

namespace Service.TechCenter
{
    public class FDYF_Service
    {

        public DataTable WeatherPdfToExcel(string FileFolderPath)
        {
            try
            {
                if (!Directory.Exists(FileFolderPath))
                {
                    return null;
                }

                string[] pdfFilePaths = Directory.GetFiles(
                    path: FileFolderPath,
                    searchPattern: "*.pdf",
                    searchOption: SearchOption.AllDirectories
                );

                DataTable dt = new DataTable("MarineMeteorologyData");
                dt.Columns.Add("DateTime", typeof(DateTime));       // 完整时间（如2025-10-02 05:00:00）
                dt.Columns.Add("SwellDirection", typeof(double));   // 涌浪-方向(度)
                dt.Columns.Add("SwellHeight", typeof(double));      // 涌浪-浪高(米)
                dt.Columns.Add("SwellPeriod", typeof(double));      // 涌浪-周期(秒)
                dt.Columns.Add("WindDirection", typeof(double));    // 风-风向(度)
                dt.Columns.Add("WindSpeed", typeof(double));        // 风-风速(米/秒)
                dt.Columns.Add("WindGust", typeof(double));         // 风-阵风(米/秒)
                dt.Columns.Add("WindSpeed50m", typeof(double));     // 风-50米风速(米/秒)
                dt.Columns.Add("WindGust50m", typeof(double));      // 风-50米阵风(米/秒)
                dt.Columns.Add("WindSpeed100m", typeof(double));    // 风-100米风速(米/秒)
                dt.Columns.Add("WindGust100m", typeof(double));     // 风-100米阵风(米/秒)
                dt.Columns.Add("TotalWaveDirection", typeof(double));// 总海浪-方向(度)
                dt.Columns.Add("TotalWaveHeight", typeof(double));  // 总海浪-浪高(米)
                dt.Columns.Add("TotalWaveMaxHeight", typeof(double));// 总海浪-最大浪高(米)
                dt.Columns.Add("WindWaveHeight", typeof(double));   // 风浪-浪高(米)
                dt.Columns.Add("WindWavePeriod", typeof(double));   // 风浪-周期(秒)
                dt.Columns.Add("TideLevel", typeof(double));        // 潮-潮位(米)
                dt.Columns.Add("CurrentDirection", typeof(double)); // 流-流向(度)
                dt.Columns.Add("CurrentSpeed", typeof(double));     // 流-流速(节)
                dt.Columns.Add("Temperature", typeof(double));      // 气温(°C)
                dt.Columns.Add("Rainfall", typeof(double));         // 降雨(毫米)
                dt.Columns.Add("Visibility", typeof(double));       // 能见度(公里)

                foreach (var file in pdfFilePaths)
                {
                    string PdfContent = PdfUtils.ExtractPdfTextByPath(file);

                    // 2. 校验输入内容
                    if (string.IsNullOrEmpty(PdfContent)) return dt;

                    // 将文本内容按行拆分（兼容所有换行符）+ 提前修剪+过滤空行
                    string[] lines = Regex.Split(PdfContent, @"\r?\n|\r")
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToArray();

                    // 3. 提取发布时间
                    DateTime publishTime = ExtractPublishTime(lines);
                    if (publishTime == DateTime.MinValue) return dt;

                    // 核心规则：仅保留发布时间后12小时内的数据
                    DateTime endTime = publishTime.AddHours(12);
                    bool isExceedTimeRange = false; // 超过12小时范围标记

                    // 核心状态变量（修复首个日期无表头问题）
                    DateTime currentDate = DateTime.MinValue;          // 当前解析的日期
                    bool isGlobalHeaderFound = false;                   // 是否找到文件开头的全局表头
                    bool isCurrentDateHeaderFound = false;              // 当前日期是否找到专属表头

                    // 5. 逐行解析（一旦超过截止时间则终止循环）
                    foreach (string line in lines)
                    {

                        // 提前终止：后续数据无意义
                        if (isExceedTimeRange) break;

                        // -------------------- 第一步：匹配全局表头（文件开头） --------------------
                        if (!isGlobalHeaderFound)
                        {
                            bool isMainHeader = line.StartsWith("时间 方向 浪高 周期 风向 风速 阵风");
                            bool isUnitHeader = line.StartsWith("(时) (度) (米) (秒) (度) (米/秒)");
                            if (isMainHeader || isUnitHeader)
                            {
                                isGlobalHeaderFound = true;
                                continue;
                            }
                        }

                        // -------------------- 第二步：匹配日期行（移除跨天终止的错误逻辑） --------------------
                        Match dateMatch = Regex.Match(line, @"^(\d{4}) 年 (\d{1,2}) 月 (\d{1,2}) 日$");
                        if (dateMatch.Success)
                        {
                            // 解析日期
                            int year = int.Parse(dateMatch.Groups[1].Value);
                            int month = int.Parse(dateMatch.Groups[2].Value);
                            int day = int.Parse(dateMatch.Groups[3].Value);
                            currentDate = new DateTime(year, month, day);

                            // 状态重置：
                            // 发布时间当天的日期 → 复用全局表头
                            // 跨天的日期（如发布时间02日20:36，03日）→ 需要匹配专属表头
                            isCurrentDateHeaderFound = (currentDate == publishTime.Date) && isGlobalHeaderFound;
                            continue;
                        }

                        // -------------------- 第三步：匹配跨天日期的专属表头 --------------------
                        if (currentDate != DateTime.MinValue && !isCurrentDateHeaderFound)
                        {
                            bool isDateHeader = line.StartsWith("时间 方向 浪高 周期 风向 风速 阵风");
                            if (isDateHeader)
                            {
                                isCurrentDateHeaderFound = true;
                                continue;
                            }
                        }

                        // -------------------- 第四步：解析数据行（支持跨天） --------------------
                        if (currentDate != DateTime.MinValue && isCurrentDateHeaderFound)
                        {
                            // 过滤非数据行
                            if (IsNonDataLine(line)) continue;

                            // 拆分数据（多空格分隔）
                            string[] dataParts = Regex.Split(line, @"\s+")
                                .Where(p => !string.IsNullOrEmpty(p))
                                .ToArray();

                            // 验证列数（至少21列才是有效数据）
                            if (dataParts.Length < 21) continue;

                            // 解析小时
                            if (!int.TryParse(dataParts[0], out int hour)) continue;
                            DateTime dataTime = currentDate.AddHours(hour);

                            // 核心过滤规则（支持跨天）：
                            if (dataTime <= publishTime) continue;       // 发布时间前的数据跳过
                            if (dataTime > endTime)
                            {
                                isExceedTimeRange = true; // 超过12小时，终止后续遍历
                                continue;
                            }

                            // 解析数据到DataRow
                            DataRow dr = dt.NewRow();
                            dr["DateTime"] = dataTime;
                            // 涌浪
                            dr["SwellDirection"] = ParseDouble(dataParts[1]);
                            dr["SwellHeight"] = ParseDouble(dataParts[2]);
                            dr["SwellPeriod"] = ParseDouble(dataParts[3]);
                            // 风（近地面）
                            dr["WindDirection"] = ParseDouble(dataParts[4]);
                            dr["WindSpeed"] = ParseDouble(dataParts[5]);
                            dr["WindGust"] = ParseDouble(dataParts[6]);
                            // 风（50米）
                            dr["WindSpeed50m"] = ParseDouble(dataParts[7]);
                            dr["WindGust50m"] = ParseDouble(dataParts[8]);
                            // 风（100米）
                            dr["WindSpeed100m"] = ParseDouble(dataParts[9]);
                            dr["WindGust100m"] = ParseDouble(dataParts[10]);
                            // 总海浪
                            dr["TotalWaveDirection"] = ParseDouble(dataParts[11]);
                            dr["TotalWaveHeight"] = ParseDouble(dataParts[12]);
                            dr["TotalWaveMaxHeight"] = ParseDouble(dataParts[13]);
                            // 风浪
                            dr["WindWaveHeight"] = ParseDouble(dataParts[14]);
                            dr["WindWavePeriod"] = ParseDouble(dataParts[15]);
                            // 潮
                            dr["TideLevel"] = ParseDouble(dataParts[16]);
                            // 流
                            dr["CurrentDirection"] = ParseDouble(dataParts[17]);
                            dr["CurrentSpeed"] = ParseDouble(dataParts[18]);
                            // 气温/降雨/能见度（处理28° - 10.1格式）
                            string tempPart = dataParts[19].Replace("°", "");
                            dr["Temperature"] = ParseDouble(tempPart);
                            dr["Rainfall"] = ParseDouble(dataParts[20]);
                            dr["Visibility"] = dataParts.Length > 21 ? ParseDouble(dataParts[21]) : DBNull.Value;

                            dt.Rows.Add(dr);
                        }

                    }

                }

                return dt;

            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }


        /// <summary>
        /// 判断是否为非数据行
        /// </summary>
        private static bool IsNonDataLine(string line)
        {
            return line.Contains("注：")
                || line.Contains("copyright")
                || line.Contains("象辑科技股份有限公司")
                || line.Contains("十至二十天预报")
                || line.Contains("雷闪潜势概率预报")
                || line.StartsWith("Beijing 涌浪")
                || line.StartsWith("°C 毫米 公里");
        }


        /// <summary>
        /// 提取发布时间
        /// </summary>
        private static DateTime ExtractPublishTime(string[] lines)
        {
            foreach (string line in lines)
            {
                Match publishMatch = Regex.Match(line, @"发布时间：(\d{4}-\d{1,2}-\d{1,2}),\s*(\d{1,2}:\d{1,2})");
                if (publishMatch.Success)
                {
                    string datePart = publishMatch.Groups[1].Value;
                    string timePart = publishMatch.Groups[2].Value;
                    if (DateTime.TryParse($"{datePart} {timePart}", out DateTime publishTime))
                    {
                        return publishTime;
                    }
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// 安全解析double
        /// </summary>
        private static object ParseDouble(string value)
        {
            value = value?.Trim() ?? "";
            if (value == "-" || string.IsNullOrEmpty(value))
            {
                return DBNull.Value;
            }
            return double.TryParse(value, out double result) ? result : DBNull.Value;
        }

    }
}
