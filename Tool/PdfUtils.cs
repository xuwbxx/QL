using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace Tool
{
    public class PdfUtils
    {
        /// <summary>
        /// 从PDF文件路径提取完整文本内容（包含分页信息）
        /// </summary>
        /// <param name="pdfFilePath">PDF文件的绝对路径/相对路径</param>
        /// <returns>整理后的PDF文本内容（按页码分隔）</returns>
        /// <exception cref="ArgumentNullException">文件路径为空</exception>
        /// <exception cref="System.IO.FileNotFoundException">PDF文件不存在</exception>
        /// <exception cref="Exception">PDF解析过程中的异常（如加密、格式损坏等）</exception>
        public static string ExtractPdfTextByPath(string pdfFilePath)
        {
            // 1. 参数校验
            if (string.IsNullOrEmpty(pdfFilePath))
            {
                throw new ArgumentNullException(nameof(pdfFilePath), "PDF文件路径不能为空");
            }

            // 2. 验证文件是否存在
            if (!System.IO.File.Exists(pdfFilePath))
            {
                throw new System.IO.FileNotFoundException("指定的PDF文件不存在", pdfFilePath);
            }

            // 3. 构建文本容器
            var textBuilder = new StringBuilder();

            // 4. 声明PDF相关资源（使用using自动释放资源，避免内存泄漏）
            PdfReader pdfReader = null;
            PdfDocument pdfDocument = null;

            try
            {
                // 初始化PDF阅读器（若PDF有密码，可传入第二个参数：new ReaderProperties().SetPassword("密码".GetBytes())）
                pdfReader = new PdfReader(pdfFilePath);
                pdfDocument = new PdfDocument(pdfReader);

                // 获取总页数
                int totalPages = pdfDocument.GetNumberOfPages();
                //textBuilder.AppendLine($"【PDF文件信息】路径：{pdfFilePath}，总页数：{totalPages}");
                //textBuilder.AppendLine("----------------------------------------------------");

                // 5. 遍历所有页面提取文本
                for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                {
                    // 获取当前页
                    var pdfPage = pdfDocument.GetPage(pageNum);
                    if (pdfPage == null)
                    {
                        //textBuilder.AppendLine($"第{pageNum}页：（页面无效或已损坏）");
                        //textBuilder.AppendLine("----------------------------------------------------");
                        continue;
                    }

                    // 文本提取策略（LocationTextExtractionStrategy：按文本位置排序提取）
                    var textExtractionStrategy = new LocationTextExtractionStrategy();
                    // 提取当前页文本
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfPage, textExtractionStrategy);

                    // 拼接分页文本
                    //textBuilder.AppendLine($"第{pageNum}页内容：");
                    textBuilder.AppendLine(string.IsNullOrWhiteSpace(pageText) ? "（该页无有效文本）" : pageText);
                    //textBuilder.AppendLine("----------------------------------------------------");
                }

                // 返回完整文本
                return textBuilder.ToString();
            }
            catch (System.IO.FileNotFoundException)
            {
                throw; // 直接抛出文件不存在异常
            }
            catch (ArgumentNullException)
            {
                throw; // 直接抛出参数异常
            }
            catch (Exception ex)
            {
                throw new Exception($"解析PDF文件失败：{ex.Message}", ex); // 包装其他异常
            }
            finally
            {
                // 确保资源释放（即使发生异常）
                pdfDocument?.Close();
                pdfReader?.Close();
            }
        }
    }
}
