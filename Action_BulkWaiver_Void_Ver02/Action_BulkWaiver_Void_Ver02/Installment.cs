// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Installment
// Assembly: Action_BulkWaiver_Void, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75ea86a04a814594
// MVID: 3A65A6E8-584D-4322-BEC4-3FD1D76E2BF8
// Assembly location: C:\Users\BSD\Desktop\Action_BulkWaiver_Void.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;

namespace Action_BulkWaiver_Void_Ver02
{
    public class Installment
    {
        private Common common;
        private IOrganizationService service;
        private Entity enInstallment;

        public DateTime InterestStarDate { get; set; }

        public int Intereststartdatetype { get; set; }

        public int Gracedays { get; set; }


        public decimal MaxPercent { get; set; }

        public decimal MaxAmount { get; set; }

        public decimal InterestPercent { get; set; }

        public DateTime Duedate { get; set; }

        public Installment(IOrganizationService service, Entity enInstallment)
        {
            enInstallment = service.Retrieve(enInstallment.LogicalName, enInstallment.Id, new ColumnSet(true));
            //getInterestStartDate();
        }
        private void getInterestStartDate()
        {
            //EntityReference entityReference1 = enInstallment.Contains("bsd_optionentry") ? (EntityReference)enInstallment["bsd_optionentry"] : (EntityReference)null;
            //Entity entity1 = enInstallment.Contains("bsd_optionentry") ? service.Retrieve(entityReference1.LogicalName, entityReference1.Id, new ColumnSet(true)) : (Entity)null;
            //EntityReference entityReference2 = entity1.Contains("bsd_paymentscheme") ? (EntityReference)entity1["bsd_paymentscheme"] : (EntityReference)null;
            //Entity entity2 = service.Retrieve(entityReference2.LogicalName, entityReference2.Id, new ColumnSet(true));
            //EntityReference entityReference3 = entity2.Contains("bsd_interestratemaster") ? (EntityReference)entity2["bsd_interestratemaster"] : (EntityReference)null;
            //if (entityReference3 == null)
            //    return;
            //Entity entity3 = service.Retrieve(entityReference3.LogicalName, entityReference3.Id, new ColumnSet(true));
            //Gracedays = entity3.Contains("bsd_gracedays") ? (int)entity3["bsd_gracedays"] : 0;
            //Intereststartdatetype = ((OptionSetValue)entity3["bsd_intereststartdatetype"]).Value;
            //Duedate = enInstallment.Contains("bsd_duedate") ? common.RetrieveLocalTimeFromUTCTime((DateTime)enInstallment["bsd_duedate"]) : new DateTime(0L);
            //MaxPercent = entity3.Contains("bsd_toleranceinterestpercentage") ? (decimal)entity3["bsd_toleranceinterestpercentage"] : 100M;
            //MaxAmount = entity3.Contains("bsd_toleranceinterestamount") ? (decimal)entity3["bsd_toleranceinterestamount"] : 0M;
            //InterestPercent = entity3.Contains("bsd_termsinterestpercentage") ? (decimal)entity3["bsd_termsinterestpercentage"] : 0M;
            //InterestStarDate = Duedate.AddDays((double)(Gracedays + 1));
        }
    }
}
