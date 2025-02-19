// Decompiled with JetBrains decompiler
// Type: Plugin_CollectionMeeting_GenerateTermination.bsd_followuplist
// Assembly: Plugin_CollectionMeeting_GenerateTermination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f7afacec0aa430c5
// MVID: 48B5B8C3-1D78-484D-B78A-C63DDA7C8A96
// Assembly location: C:\Users\ngoct\Downloads\Plugin_CollectionMeeting_GenerateTermination_1.0.0.0.dll

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Plugin_CollectionMeeting_GenerateTermination2
{
  public class bsd_followuplist
  {
    public Entity ent { get; set; }

    public Decimal TotalForfeitureAmount { get; set; }

    public Decimal TotalPaid { get; set; }

    public Decimal Refund { get; set; }

    public bsd_followuplist() => this.defaultVal();

    public bsd_followuplist(Entity _ent)
    {
      if (_ent == null)
        return;
      this.ent = _ent;
    }

    private void defaultVal()
    {
      this.ent = new Entity();
      this.TotalForfeitureAmount = 0M;
      this.TotalPaid = 0M;
      this.Refund = 0M;
    }

    public info_Error getByID(IOrganizationService service, Guid ent_id)
    {
      info_Error byId = new info_Error();
      try
      {
        QueryExpression query = new QueryExpression(nameof (bsd_followuplist));
        query.ColumnSet = new ColumnSet(true);
        query.TopCount = new int?(1);
        FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
        filterExpression.AddCondition(new ConditionExpression("bsd_followuplistid", ConditionOperator.Equal, (object) ent_id));
        query.Criteria = filterExpression;
        EntityCollection entityCollection = service.RetrieveMultiple((QueryBase) query);
        if (entityCollection.Entities.Any<Entity>())
        {
          byId.result = true;
          byId.count = entityCollection.Entities.Count;
          byId.entc = entityCollection;
          byId.ent_First = entityCollection.Entities[0];
          byId.createMessageAdd(string.Format("Done. Total record = {0}", (object) byId.count));
        }
        else
        {
          byId.result = false;
          byId.createMessageAdd(string.Format("Done, not data. Total record = 0."));
        }
      }
      catch (Exception ex)
      {
        byId.result = false;
        throw new Exception(string.Format("#.Error sys {0}", (object) ex.Message));
      }
      return byId;
    }

    public enum bsd_takeoutmoney
    {
      Refund = 100000000, // 0x05F5E100
      Forfeiture = 100000001, // 0x05F5E101
    }
  }
}
