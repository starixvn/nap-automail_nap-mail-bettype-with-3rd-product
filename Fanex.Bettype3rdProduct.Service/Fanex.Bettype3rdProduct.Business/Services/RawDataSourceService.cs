using BettypeWith3rdProducts.Business.Criteria;
using Fanex.Bettype3rdProduct.Business.Models;
using Fanex.Data;
using System.Collections.Generic;
using System.Data;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface RawDataSourceService
{
    Dictionary<string, DataTable> QueryDataSource(SourceMapping sourceMapping);
}

public class RawDataSourceServiceImp : RawDataSourceService
{
    private readonly DateProviderService dateService;

    public RawDataSourceServiceImp(DateProviderService dateService)
    {
        this.dateService = dateService;
    }

    public Dictionary<string, DataTable> QueryDataSource(SourceMapping sourceMapping)
    {
        var date = dateService.Date();
        var rawDataSource = new Dictionary<string, DataTable>();
        var criteria = new BettypeProductCriteria
        {
            DateStart = date.DateStart,
            DateEnd = date.DateEnd,
            StatisticId = (int)sourceMapping.DataType,
            StoreProcedure = sourceMapping.StoreProcedure
        };

        var dataSet = QueryData(criteria);
        var index = 0;
        var key = string.Empty;
        foreach (DataTable dataTable in dataSet.Tables)
        {
            switch (index)
            {
                case 0:
                    key = dataTable.Rows[0]["Sport"].ToString();
                    break;

                case 1:
                    rawDataSource.Add(key, dataTable);
                    index = -1;
                    break;
            }

            index++;
        }

        return rawDataSource;
    }

    private DataSet QueryData(BettypeProductCriteria criteria)
    {
        using var db = new ObjectDb(criteria.GetSettingKey(), 0);
        db.SetParameterValue("datestart", criteria.DateStart);
        db.SetParameterValue("dateend", criteria.DateEnd);
        db.SetParameterValue("statisticid", criteria.StatisticId);

        return db.ExecuteDataSet();
    }
}