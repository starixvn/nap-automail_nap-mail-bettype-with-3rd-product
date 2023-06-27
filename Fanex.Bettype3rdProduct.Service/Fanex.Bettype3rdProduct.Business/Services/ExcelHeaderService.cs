using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface ExcelHeaderService
{
    void SplitHeader(ExcelWorksheet excelWorksheet, DataTable section, int startRow);
}

public class ExcelHeaderServiceImp : ExcelHeaderService
{
    private const int WidthHeader = 18;

    public void SplitHeader(ExcelWorksheet excelWorksheet, DataTable section, int startRow)
    {
        var columns = new List<string>();
        RenderColumns(excelWorksheet, section, startRow, columns);
        MergeColumns(excelWorksheet, startRow, columns);
    }

    private static void RenderColumns(ExcelWorksheet excelWorksheet, DataTable section, int startRow, List<string> columns)
    {
        var startColumn = 2;
        for (var columnIndex = 0; columnIndex < section.Columns.Count; columnIndex++)
        {
            var columnName = section.Columns[columnIndex].ColumnName;
            var arrayBettypes = columnName.Split(new[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries);
            var firstHeader = arrayBettypes[0];
            var secondHeader = arrayBettypes.Length == 1
                ? arrayBettypes[0]
                : arrayBettypes[1];

            excelWorksheet.Cells[startRow, startColumn].Value = firstHeader;
            excelWorksheet.Cells[startRow + 1, startColumn].Value = secondHeader;
            excelWorksheet.Column(startColumn).Width = WidthHeader;
            columns.Add(firstHeader);
            startColumn++;
        }
    }

    private static void MergeColumns(ExcelWorksheet excelWorksheet, int startRow, List<string> columns)
    {
        var startColumn = 2;
        columns
            .Distinct()
            .ToList()
            .ForEach(firstHeader =>
            {
                var numberOfBettypes = columns.Count(columnName => columnName == firstHeader);
                if (numberOfBettypes == 1)
                {
                    excelWorksheet.Cells[startRow, startColumn, startRow + 1, startColumn].Merge = true;
                }
                else
                {
                    excelWorksheet.Cells[startRow, startColumn, startRow, startColumn + numberOfBettypes - 1].Merge = true;
                }

                startColumn += numberOfBettypes;
            });
    }
}