using System.Collections.Generic;

namespace Fanex.Bettype3rdProduct.Business.Services;

public interface DateProviderService
{
    DateService Date();
}

public class DateProviderServiceImp : DateProviderService
{
    public DateService Date()
    {
        var date = new Dictionary<bool, DateService>
        {
            { true, new DateManualServiceImp() },
            { false , new DateDailyServiceImp() }
        };

        return date[AppConfigService.Config.IsManual];
    }
}