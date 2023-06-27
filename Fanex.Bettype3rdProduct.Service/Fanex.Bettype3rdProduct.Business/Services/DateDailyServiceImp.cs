using System;

namespace Fanex.Bettype3rdProduct.Business.Services;

public class DateDailyServiceImp : DateService
{
    public DateTime DateStart
    {
        get
        {
            var date = DateTime.Now.AddDays(-1);
            return Convert.ToDateTime($"{date:yyyy-MM-01}");
        }
    }

    public DateTime DateEnd
    {
        get
        {
            var date = DateTime.Now.AddDays(-1);
            return Convert.ToDateTime($"{date:yyyy-MM-dd}");
        }
    }
}