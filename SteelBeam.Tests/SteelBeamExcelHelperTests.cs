using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tool;

namespace SteelBeam.Tests;

[TestClass]
public class SteelBeamExcelHelperTests
{
    [DataTestMethod]
    [DataRow("GS-Z-1-4", true)]
    [DataRow("a_b.c/1", true)]
    [DataRow("gs a", false)]
    [DataRow("中文", false)]
    [DataRow("=1+1", false)]
    [DataRow("-A1", false)]
    public void PointCodeValidationMatchesRequirement(string value, bool expected) =>
        Assert.AreEqual(expected, SteelBeamExcelHelper.HasValidPointCode(value));

    [TestMethod]
    public void SanitizePathPartReplacesInvalidCharacters() =>
        Assert.AreEqual("项目_一_", SteelBeamExcelHelper.SanitizePathPart("项目/一?"));

    [TestMethod]
    public void ParseTheoreticalReadsFirstWorksheetAndIgnoresExtraWorksheet()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("数据");
        for (var i = 0; i < SteelBeamExcelHelper.TheoreticalHeaders.Length; i++)
            sheet.Cell(1, i + 1).Value = SteelBeamExcelHelper.TheoreticalHeaders[i];
        sheet.Cell(2, 1).Value = "GS-Z-1-4";
        sheet.Cell(2, 2).Value = 1.123456;
        sheet.Cell(2, 3).Value = -2.5;
        sheet.Cell(2, 4).Value = 0;
        sheet.Cell(2, 5).Value = 0.01;
        sheet.Cell(2, 6).Value = 1;
        sheet.Cell(2, 7).Value = "A";
        sheet.Cell(2, 8).Value = "测位1";
        workbook.AddWorksheet("填写说明").Cell(1, 1).Value = "不读取";
        using var stream = Save(workbook);

        var result = SteelBeamExcelHelper.ParseTheoretical(stream);

        Assert.IsTrue(result.Success, string.Join(";", result.Errors.Select(a => a.Reason)));
        Assert.AreEqual(1, result.Rows.Count);
        Assert.AreEqual("GS-Z-1-4", result.Rows[0].PointCode);
    }

    [TestMethod]
    public void ParseMeasuredRejectsFormulaAndDuplicateCodeIgnoringCase()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("数据");
        for (var i = 0; i < SteelBeamExcelHelper.MeasuredHeaders.Length; i++)
            sheet.Cell(1, i + 1).Value = SteelBeamExcelHelper.MeasuredHeaders[i];
        sheet.Cell(2, 1).Value = "GS-A1";
        sheet.Cell(2, 2).FormulaA1 = "1+1";
        sheet.Cell(2, 3).Value = 2;
        sheet.Cell(2, 4).Value = 3;
        sheet.Cell(3, 1).Value = "gs-a1";
        sheet.Cell(3, 2).Value = 1;
        sheet.Cell(3, 3).Value = 2;
        sheet.Cell(3, 4).Value = 3;
        using var stream = Save(workbook);

        var result = SteelBeamExcelHelper.ParseMeasured(stream);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Errors.Any(a => a.Reason.Contains("公式")));
        Assert.IsTrue(result.Errors.Any(a => a.Reason.Contains("重复")));
    }

    [TestMethod]
    public void DecimalScaleValidationUsesConfiguredScale()
    {
        Assert.IsTrue(SteelBeamExcelHelper.HasScale(1.123456m, 6));
        Assert.IsFalse(SteelBeamExcelHelper.HasScale(1.1234567m, 6));
        Assert.IsFalse(SteelBeamExcelHelper.HasScale(0.12345m, 4));
    }

    [TestMethod]
    public void TemplatesContainDataAndInstructionWorksheets()
    {
        using var stream = new MemoryStream(SteelBeamExcelHelper.CreateTheoreticalTemplate());
        using var workbook = new XLWorkbook(stream);
        Assert.AreEqual("数据", workbook.Worksheet(1).Name);
        Assert.AreEqual("填写说明", workbook.Worksheet(2).Name);
    }

    private static MemoryStream Save(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
