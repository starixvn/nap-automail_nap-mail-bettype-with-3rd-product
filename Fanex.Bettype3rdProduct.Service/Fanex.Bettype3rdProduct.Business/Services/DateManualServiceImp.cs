using System;

namespace Fanex.Bettype3rdProduct.Business.Services;

public class DateManualServiceImp : DateService
{
    public DateTime DateStart => AppConfigService.Config.DateStart;

    public DateTime DateEnd => AppConfigService.Config.DateEnd;
}