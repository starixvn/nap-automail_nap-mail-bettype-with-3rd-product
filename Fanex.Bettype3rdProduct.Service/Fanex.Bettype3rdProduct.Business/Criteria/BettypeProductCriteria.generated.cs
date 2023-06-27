using System;
using Fanex.Data;
using Fanex.Data.Repository;

namespace BettypeWith3rdProducts.Business.Criteria
{
    public class BettypeProductCriteria : CriteriaBase
    {
        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public int? Periodid { get; set; }

        public int? Sportid { get; set; }

        public int StatisticId { get; set; }

        [SpParam(Ignored = true)]
        public string StoreProcedure { get; set; }

        public override string GetSettingKey()
        {
            return StoreProcedure;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}