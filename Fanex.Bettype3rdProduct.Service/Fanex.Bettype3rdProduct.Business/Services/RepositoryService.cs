using System.Collections.Generic;
using System.Data;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface RepositoryBettypeProductService
{
    Dictionary<string, DataTable> QueryGeneralData(Dictionary<string, DataTable> rawDataSource);

    Dictionary<string, DataTable> QueryDetailData(Dictionary<string, DataTable> rawDataSource);
}

public class RepositoryServiceImp : RepositoryBettypeProductService
{
    public Dictionary<string, DataTable> QueryGeneralData(Dictionary<string, DataTable> rawDataSource)
    {
        var dataSource = new Dictionary<string, DataTable>();
        var index = 0;
        const int MaxIndex = 2;
        foreach (var item in rawDataSource)
        {
            if (index < MaxIndex)
            {
                dataSource.Add(item.Key, item.Value);
            }

            index++;
        }

        return dataSource;
    }

    public Dictionary<string, DataTable> QueryDetailData(Dictionary<string, DataTable> rawDataSource)
    {
        var dataSource = new Dictionary<string, DataTable>();
        var index = 0;
        foreach (var item in rawDataSource)
        {
            if (index > 1)
            {
                dataSource.Add(item.Key, item.Value);
            }

            index++;
        }

        return dataSource;
    }
}