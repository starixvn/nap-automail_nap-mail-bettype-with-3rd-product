using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface ExcelService
{
    string Execute();
}

public class ExcelServiceImp : ExcelService
{
    private readonly ExcelHeaderService excelHeaderService;
    private readonly DataService dataService;
    private readonly DateProviderService dateProviderService;

    public ExcelServiceImp(
        DataService dataService,
        ExcelHeaderService excelHeaderService,
        DateProviderService dateProviderService)
    {
        this.dataService = dataService;
        this.excelHeaderService = excelHeaderService;
        this.dateProviderService = dateProviderService;
    }

    public string Execute()
    {
        var excelPackage = new ExcelPackage(new FileInfo(@"Templates/Template.xlsx"));
        var dataTemplate = excelPackage.Workbook.Worksheets["Data"];

        var dataSource = dataService.Execute();
        foreach (var (sheetName, sports) in dataSource)
        {
            var excelWorksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            ExportTitle(excelPackage, excelWorksheet, sheetName);
            FreezePanes(excelWorksheet);
            ExportData(excelWorksheet, dataTemplate, sports);
        }

        return Save(excelPackage, dataTemplate);
    }

    private void ExportData(ExcelWorksheet excelWorksheet, ExcelWorksheet dataTemplate, Dictionary<string, List<DataTable>> sports)
    {
        foreach (var sport in sports)
        {
            foreach (var section in sport.Value)
            {
                var startRowIndex = excelWorksheet.Dimension.End.Row;
                var sportNameAddress = $"B{startRowIndex + 2}";
                var sportHeaderStartRowAddress = $"B{startRowIndex + 5}";
                excelWorksheet.Cells[sportNameAddress].Value = sport.Key;
                excelWorksheet.Cells[sportNameAddress].StyleID = dataTemplate.Cells["B5"].StyleID;
                excelWorksheet.Cells[sportHeaderStartRowAddress].LoadFromDataTable(section, false);
                FormatHeaderSportSection(excelWorksheet, dataTemplate, section, startRowIndex);
                FormatDateColumn(excelWorksheet, dataTemplate, startRowIndex);
                FormatDataSection(excelWorksheet, dataTemplate, section, startRowIndex);
                FormatTotal(excelWorksheet, dataTemplate, section, startRowIndex);
            }
        }

        excelWorksheet.View.ShowGridLines = false;
    }

    private void FormatTotal(ExcelWorksheet excelWorksheet, ExcelWorksheet dataTemplate, DataTable section, int startRowIndex)
    {
        var startRow = startRowIndex + section.Rows.Count + 5;
        var startColumn = 2;
        excelWorksheet.InsertRow(startRow, 1);
        dataTemplate.Cells["B8"].Copy(excelWorksheet.Cells[startRow, startColumn]);
        startColumn++;

        for (var columnIndex = 1; columnIndex < section.Columns.Count; columnIndex++)
        {
            excelWorksheet.Cells[startRow, startColumn].Value = section.AsEnumerable().Sum(row => row[columnIndex] is DBNull ? 0 : Convert.ToDouble(row[columnIndex]));
            excelWorksheet.Cells[startRow, startColumn].StyleID = dataTemplate.Cells["C8"].StyleID;
            startColumn++;
        }
    }

    private void FormatDataSection(ExcelWorksheet excelWorksheet, ExcelWorksheet dataTemplate, DataTable section, int startRowIndex)
    {
        const int StartColumn = 3;
        var startRow = startRowIndex + 5;
        var endRow = startRowIndex + section.Rows.Count + 4;
        var endColumn = section.Columns.Count + 1;
        excelWorksheet.Cells[startRow, StartColumn, endRow, endColumn].StyleID = dataTemplate.Cells["C7"].StyleID;
        for (var columnIndex = 3; columnIndex <= endColumn; columnIndex++)
        {
            for (var rowIndex = startRow; rowIndex <= endRow; rowIndex++)
            {
                var value = excelWorksheet.Cells[rowIndex, columnIndex].Value;
                excelWorksheet.Cells[rowIndex, columnIndex].Value = Convert.ToDouble(value);
            }
        }
    }

    private void FormatDateColumn(ExcelWorksheet excelWorksheet, ExcelWorksheet dataTemplate, int startRowIndex)
    {
        var start = $"B{startRowIndex + 4}";
        var end = $"B{excelWorksheet.Dimension.End.Row}";
        excelWorksheet.Cells[$"{start}:{end}"].StyleID = dataTemplate.Cells["B7"].StyleID;
    }

    private void FormatHeaderSportSection(ExcelWorksheet excelWorksheet, ExcelWorksheet dataTemplate, DataTable section, int startRowIndex)
    {
        const int StartColumn = 2;
        var startRow = startRowIndex + 3;
        var endColumn = section.Columns.Count + 1;
        excelWorksheet.Cells[startRow, StartColumn, startRow + 1, endColumn].StyleID = dataTemplate.Cells["B6"].StyleID;
        excelHeaderService.SplitHeader(excelWorksheet, section, startRow);
    }

    private void ExportTitle(ExcelPackage excelPackage, ExcelWorksheet excelWorksheet, string sheetName)
    {
        var date = dateProviderService.Date();
        excelPackage.Workbook.Worksheets[$"{sheetName} Temp"].Cells["B2:B5"].Copy(excelWorksheet.Cells["B2"]);
        excelWorksheet.Cells["B2"].Value = excelWorksheet.Cells["B2"].Value.ToString()
            .Replace("{0}", $"{date.DateStart:MM/dd/yyyy}")
            .Replace("{1}", $"{date.DateEnd:MM/dd/yyyy}");
        excelWorksheet.Cells["B3"].Value = excelWorksheet.Cells["B3"].Value.ToString()
            .Replace("{0}", $"{DateTime.Now}");
        excelPackage.Workbook.Worksheets.Delete($"{sheetName} Temp");
    }

    private static void FreezePanes(ExcelWorksheet excelWorksheet)
    {
        const int ColumnIndex = 3;
        var rowIndex = excelWorksheet.Dimension.End.Row + 1;
        excelWorksheet.View.FreezePanes(rowIndex, ColumnIndex);
    }

    public string Save(ExcelPackage excelPackage, ExcelWorksheet dataTemplate)
    {
        excelPackage.Workbook.Worksheets.Delete(dataTemplate);
        var folder = $@"{DateTime.Now:yyyy}/{DateTime.Now:MM}";
        var root = $@"{AppConfigService.Config.Path}/{folder}";
        Directory.CreateDirectory(root);

        var fileName = $@"{root}/Bettype_With_3rd_Products_Monthly_Winloss_For_{DateTime.Now:yyyy_MM_dd}.xlsx";
        excelPackage.SaveAs(new FileInfo(fileName), AppConfigService.Config.Password);

        return fileName;
    }
}