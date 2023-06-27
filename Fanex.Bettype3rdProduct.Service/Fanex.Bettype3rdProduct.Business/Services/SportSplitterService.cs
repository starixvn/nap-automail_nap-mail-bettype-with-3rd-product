using Fanex.Bettype3rdProduct.Business.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface SportSplitterService
{
    Dictionary<string, List<DataTable>> Execute(SourceMapping sourceMapping);
}

public class SportSplitterServiceImp : SportSplitterService
{
    private const int MaxNumberOfColumnsOnEachSection = 70;
    private readonly RepositoryBettypeProductService repositoryService;
    private readonly RawDataSourceService rawDataSourceService;

    private readonly int firstIndexSportSection;

    public SportSplitterServiceImp(RepositoryBettypeProductService repositoryService, RawDataSourceService rawDataSourceService)
    {
        this.repositoryService = repositoryService;
        this.rawDataSourceService = rawDataSourceService;

        firstIndexSportSection = 0;
    }

    public virtual int NumberOfColumnsOnEachSection => MaxNumberOfColumnsOnEachSection;

    public Dictionary<string, List<DataTable>> Execute(SourceMapping sourceMapping)
    {
        var rawDataSource = rawDataSourceService.QueryDataSource(sourceMapping);
        var result = new Dictionary<string, List<DataTable>>();
        SplitGeneralData(rawDataSource, result);
        SplitSportData(rawDataSource, result);

        return result;
    }

    private void SplitGeneralData(Dictionary<string, DataTable> rawDataSource, Dictionary<string, List<DataTable>> result)
    {
        var generalData = repositoryService.QueryGeneralData(rawDataSource);
        foreach (var sport in generalData)
        {
            result.Add(sport.Key, new List<DataTable> { sport.Value });
        }
    }

    private void SplitSportData(Dictionary<string, DataTable> rawDataSource, Dictionary<string, List<DataTable>> result)
    {
        var detailData = repositoryService.QueryDetailData(rawDataSource);
        foreach (var sport in detailData)
        {
            var sportSections = SplitSportIntoManySections(sport.Value);
            result.Add(sport.Key, sportSections);
        }
    }

    private List<DataTable> SplitSportIntoManySections(DataTable value)
    {
        var numberOfSections = GetNumberOfSections(value);
        var endColumnIndex = 0;
        var lastRemovedIndex = 0;
        var sections = new List<DataTable>();

        for (var indexOfSportSection = 0; indexOfSportSection < numberOfSections; indexOfSportSection++)
        {
            int beginColumnIndex;
            if (indexOfSportSection == firstIndexSportSection)
            {
                beginColumnIndex = 1;
                endColumnIndex = NumberOfColumnsOnEachSection + 1;
            }
            else
            {
                beginColumnIndex = endColumnIndex - lastRemovedIndex;
                endColumnIndex = beginColumnIndex + NumberOfColumnsOnEachSection;
            }

            endColumnIndex = endColumnIndex > value.Columns.Count ? value.Columns.Count : endColumnIndex;
            lastRemovedIndex = AddSportIntoSection(value, beginColumnIndex, endColumnIndex, sections);
        }

        return sections;
    }

    private static int AddSportIntoSection(DataTable value, int beginColumnIndex, int endColumnIndex, List<DataTable> sections)
    {
        var columnNames = new List<string>();
        var lastRemovedIndex = 0;
        columnNames.Add(value.Columns[0].ColumnName);
        for (var columnIndex = beginColumnIndex; columnIndex < endColumnIndex; columnIndex++)
        {
            var columnName = value.Columns[columnIndex].ColumnName;
            var currentBettype = columnName.Split(new[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries)[0];
            columnNames.Add(columnName);
            lastRemovedIndex = columnIndex == endColumnIndex - 1
                ? RemoveRedundantBettypes(columnNames, currentBettype)
                : lastRemovedIndex;
        }

        var section = value.AsDataView().ToTable(false, columnNames.ToArray());
        if (endColumnIndex > beginColumnIndex)
        {
            sections.Add(section);
        }

        return lastRemovedIndex;
    }

    private static int RemoveRedundantBettypes(List<string> columnNames, string currentBettype)
    {
        var lastRemovedIndex = 0;
        var numberOfBettypeOnEachGroup = GetNumberOfBettypeOnEachGroup(columnNames);
        var countBettypeGroup = CountBettypeGroup(columnNames, currentBettype);
        var isEnoughBettypeGroup = countBettypeGroup == numberOfBettypeOnEachGroup;
        if (isEnoughBettypeGroup) return lastRemovedIndex;
        columnNames.RemoveRange(columnNames.Count - countBettypeGroup, countBettypeGroup);
        lastRemovedIndex = countBettypeGroup;

        return lastRemovedIndex;
    }

    private static int CountBettypeGroup(IEnumerable<string> columnNames, string currentBettype)
    {
        return columnNames
            .Where(s => s.Contains("@#@"))
            .Count(s => s.Split(new[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries)[0] == currentBettype);
    }

    private static int GetNumberOfBettypeOnEachGroup(List<string> columnNames)
    {
        var count = columnNames.Count(s => s.Contains("@#@"));
        if (count == 0)
        {
            return count;
        }

        return columnNames
            .Where(s => s.Contains("@#@"))
            .Select(s => s.Split(new[] { "@#@" }, StringSplitOptions.RemoveEmptyEntries)[0])
            .GroupBy(group => group, (key, value) => new { count = value.Count() })
            .Distinct()
            .Max(bettype => bettype.count);
    }

    private int GetNumberOfSections(DataTable value)
    {
        var columnCount = value.Columns.Count;
        var numberOfSections = columnCount / NumberOfColumnsOnEachSection;
        numberOfSections = columnCount % NumberOfColumnsOnEachSection > 0
            ? numberOfSections + 1
            : numberOfSections;

        return numberOfSections;
    }
}