using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tool
{
    public static class NPOIWordUtils
    {
        #region 读取模板 / 输出流（原有代码不变）
        public static XWPFDocument LoadTemplate(string templatePath)
        {
            var fs = File.OpenRead(templatePath);
            return new XWPFDocument(fs);
        }
        public static XWPFDocument LoadTemplate(Stream stream)
        {
            return new XWPFDocument(stream);
        }

        public static MemoryStream SaveToStream(this XWPFDocument doc)
        {
            MemoryStream ms = new MemoryStream();
            doc.Write(ms);
            ms.Position = 0;
            return ms;
        }
        #endregion

        #region 占位符替换【修复版本，保留字体/下划线样式】
        public static XWPFDocument ReplacePlaceholders(this XWPFDocument doc, Dictionary<string, string> dataDict)
        {
            foreach (var para in doc.Paragraphs)
            {
                ReplaceParaPlaceholder(para, dataDict);
            }
            foreach (var table in doc.Tables)
            {
                ScanTableForPlaceholder(table, dataDict);
            }
            return doc;
        }

        private static void ScanTableForPlaceholder(XWPFTable table, Dictionary<string, string> dict)
        {
            foreach (var row in table.Rows)
            {
                foreach (var cell in row.GetTableCells())
                {
                    foreach (var para in cell.Paragraphs)
                    {
                        ReplaceParaPlaceholder(para, dict);
                    }
                    //递归扫描单元格内部嵌套子表
                    foreach (var subTable in cell.Tables)
                    {
                        ScanTableForPlaceholder(subTable, dict);
                    }
                }
            }
        }

        /// <summary>
        /// 修复版：占位替换，保留原有字体、下划线、字号样式
        /// </summary>
        private static void ReplaceParaPlaceholder(XWPFParagraph para, Dictionary<string, string> dataDict)
        {
            if (para.Runs.Count == 0) return;
            // 逐个Run独立替换，只修改Run内字符串，不删除任何Run
            foreach (var run in para.Runs)
            {
                string txt = run.Text;
                bool changed = false;
                foreach (var kv in dataDict)
                {
                    string token = $"${{{kv.Key}}}";
                    if (txt.Contains(token))
                    {
                        txt = txt.Replace(token, kv.Value ?? string.Empty);
                        changed = true;
                    }
                }
                if (changed)
                {
                    run.SetText(txt);
                }
            }
        }


        #endregion

        #region 底层核心：给定XWPFTable直接填充（唯一一份填充逻辑）
        /// <summary>单元格字体快照，用于克隆行后恢复字体样式（适配NPOI2.7.6）</summary>
        public class CellFontSnapshot
        {
            public string FontFamily { get; set; } = string.Empty;
            public double FontSize { get; set; }
            public bool IsBold { get; set; }
        }

        /// <summary>读取模板单元格字体快照</summary>
        public static CellFontSnapshot GetCellFontSnapshot(XWPFTableCell templateCell)
        {
            var snap = new CellFontSnapshot();
            if (templateCell.Paragraphs.Count == 0) return snap;
            var para = templateCell.Paragraphs[0];
            if (para.Runs.Count == 0) return snap;

            var run = para.Runs[0];
            // 读取东亚字体（中文）
            snap.FontFamily = run.GetFontFamily(FontCharRange.EastAsia);
            snap.FontSize = run.FontSize;
            snap.IsBold = run.IsBold;
            return snap;
        }

        /// <summary>
        /// 安全设置单元格文本，传入字体快照，新建Run强制还原模板字体
        /// </summary>
        public static void SetCellTextPreserveStyle(this XWPFTableCell cell, string text, CellFontSnapshot? fontSnap = null)
        {
            XWPFParagraph para;
            if (cell.Paragraphs.Count > 0)
            {
                para = cell.Paragraphs[0];
            }
            else
            {
                para = cell.AddParagraph();
            }

            XWPFRun run;
            if (para.Runs.Count > 0)
            {
                run = para.Runs[0];
                run.SetText(text ?? string.Empty);
                // 删除其余多余run
                for (int i = para.Runs.Count - 1; i >= 1; i--)
                {
                    para.RemoveRun(i);
                }
            }
            else
            {
                run = para.CreateRun();
                run.SetText(text ?? string.Empty);
                // 有快照，还原字体
                if (fontSnap != null && !string.IsNullOrEmpty(fontSnap.FontFamily))
                {
                    // ✅ 补齐第二个参数，中文用 FontCharRange.None
                    run.SetFontFamily(fontSnap.FontFamily, FontCharRange.None);
                    run.FontSize = fontSnap.FontSize;
                    run.IsBold = fontSnap.IsBold;
                }
            }
        }

        public static void FillTableByXWPFTable<T>(XWPFTable table, int templateRowIndex, List<T> dataList, Action<XWPFTableRow, T, List<CellFontSnapshot>> fillRowAction)
        {
            if (table.Rows.Count <= templateRowIndex)
                throw new Exception("模板样例行索引超出表格行数");
            XWPFTableRow templateRow = table.GetRow(templateRowIndex);

            // 预先读取模板行所有单元格字体快照
            var templateCells = templateRow.GetTableCells();
            var fontSnapshots = new List<CellFontSnapshot>();
            foreach (var c in templateCells)
            {
                fontSnapshots.Add(GetCellFontSnapshot(c));
            }

            int insertPos = templateRowIndex + 1;
            foreach (var item in dataList)
            {
                XWPFTableRow newRow = templateRow.CloneRow();
                // 把字体快照数组传给回调
                fillRowAction.Invoke(newRow, item, fontSnapshots);

                table.AddRow(newRow, insertPos);
                table.RemoveRow(table.Rows.Count - 1);
                insertPos++;
            }
            table.RemoveRow(templateRowIndex);
        }
        #endregion

        #region 方式1：按tableIndex查找顶层表格，调用底层FillTableByXWPFTable
        /// <summary>
        /// 按文档顶层表格索引填充（顶层独立表格）
        /// </summary>
        public static XWPFDocument FillTableByTemplateRow<T>(this XWPFDocument doc,
            int tableIndex,
            int templateRowIndex,
            List<T> dataList,
            Action<XWPFTableRow, T, List<CellFontSnapshot>> fillRowAction)
        {
            if (doc.Tables.Count <= tableIndex)
                throw new ArgumentOutOfRangeException(nameof(tableIndex), "未找到该索引的顶层表格");
            var table = doc.Tables[tableIndex];
            FillTableByXWPFTable(table, templateRowIndex, dataList, fillRowAction);
            return doc;
        }
        #endregion

        #region 方式2：按占位Tag标签递归查找表格（支持任意层级嵌套表格）
        /// <summary>
        /// 根据模板内标签 ${{tagName}} 定位目标表格，标签写在模板样例行单元格内
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <param name="tagName">标签名，不带${}</param>
        /// <param name="dataList"></param>
        /// <param name="fillRowAction"></param>
        /// <returns></returns>
        public static XWPFDocument FillTableByPlaceholderTag<T>(this XWPFDocument doc,
            string tagName,
            List<T> dataList,
            Action<XWPFTableRow, T, List<CellFontSnapshot>> fillRowAction)
        {
            string targetText = $"${{{tagName}}}";
            //递归查找包含tag的段落，并拿到所属表格 + 当前行索引
            var result = FindParagraphWithTag(doc, targetText);
            if (result == null)
                throw new Exception($"文档中未找到标记：{targetText}");

            XWPFTable targetTable = result.Table;
            int templateRowIndex = result.RowIndex;
            FillTableByXWPFTable(targetTable, templateRowIndex, dataList, fillRowAction);
            return doc;
        }

        /// <summary>
        /// 将XWPFDocument转成byte数组
        /// </summary>
        /// <param name="doc">XWPF文档对象</param>
        /// <returns>docx字节数组</returns>
        public static byte[] ToByteArray(this XWPFDocument doc)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            using var ms = new MemoryStream();
            doc.Write(ms);
            ms.Position = 0;
            return ms.ToArray();
        }

        // 查找结果实体，内部用
        private class TableFindResult
        {
            public XWPFTable Table { get; set; } = null!;
            public int RowIndex { get; set; }
        }

        /// <summary>
        /// 递归扫描文档所有段落（正文、表格、嵌套子表）寻找标记
        /// </summary>
        private static TableFindResult? FindParagraphWithTag(XWPFDocument doc, string targetText)
        {
            // 扫描顶层段落
            foreach (var para in doc.Paragraphs)
            {
                if (para.Text.Contains(targetText))
                    throw new Exception("标签不能放在正文段落，必须放在表格单元格内");
            }
            //扫描所有顶层表格 + 递归嵌套子表
            foreach (var table in doc.Tables)
            {
                var findResult = ScanTableRecursive(table, targetText);
                if (findResult != null) return findResult;
            }
            return null;
        }

        private static TableFindResult? ScanTableRecursive(XWPFTable table, string targetText)
        {
            for (int r = 0; r < table.Rows.Count; r++)
            {
                var row = table.GetRow(r);
                foreach (var cell in row.GetTableCells())
                {
                    //当前单元格段落查找tag
                    foreach (var para in cell.Paragraphs)
                    {
                        if (para.Text.Contains(targetText))
                        {
                            return new TableFindResult
                            {
                                Table = table,
                                RowIndex = r
                            };
                        }
                    }
                    //递归扫描单元格内嵌套子表格
                    foreach (var subTable in cell.Tables)
                    {
                        var subRes = ScanTableRecursive(subTable, targetText);
                        if (subRes != null) return subRes;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
