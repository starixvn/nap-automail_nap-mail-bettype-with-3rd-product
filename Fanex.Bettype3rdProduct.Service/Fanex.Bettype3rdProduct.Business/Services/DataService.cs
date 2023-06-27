using BettypeWith3rdProducts.Business.Criteria;
using Fanex.Bettype3rdProduct.Business.Models;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface DataService
{
    Dictionary<string, Dictionary<string, List<DataTable>>> Execute();

    bool IsDataAvailable();
}

public class DataServiceImp : DataService
{
    private readonly SportSplitterService sportSplitterService;
    private readonly IDynamicRepository repository;

    public DataServiceImp(
        SportSplitterService sportSplitterService,
        IDynamicRepository repository)
    {
        this.sportSplitterService = sportSplitterService;
        this.repository = repository;
    }

    public Dictionary<string, Dictionary<string, List<DataTable>>> Execute()
    {
        var sourceMappings = AppConfigService.Config.SourceMappings;

        return sourceMappings.ToDictionary(sourceMapping => sourceMapping.SheetName, sportSplitterService.Execute);
    }

    public bool IsDataAvailable()
    {
        if (AppConfigService.Config.IsManual || !AppConfigService.Config.IsCheckEnoughData)
        {
            return true;
        }

        return QueryDWStatus().DataIsReady;
    }

    private TaskStatusModel QueryDWStatus()
    {
        return repository.Get<TaskStatusModel>(new GetDWStatusCriteria());
    }
}