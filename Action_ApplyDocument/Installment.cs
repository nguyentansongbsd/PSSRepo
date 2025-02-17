using System;

namespace Action_ApplyDocument
{
    public class Installment
    {
        public DateTime InterestStarDate { get; set; }

        public int Intereststartdatetype { get; set; }

        public int Gracedays { get; set; }

        public int LateDays { get; set; }

        public Decimal MaxPercent { get; set; }

        public Decimal MaxAmount { get; set; }

        public Decimal InterestPercent { get; set; }

        public Decimal InterestCharge { get; set; }

        public DateTime Duedate { get; set; }
    }
}
