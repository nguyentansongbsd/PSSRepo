// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.UserQueryDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.ViewPicker;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class UserQueryDataAccess
  {
    private const string UserQueryEntityName = "userquery";
    private const string UserQueryFetchXmlAttribute = "fetchxml";
    private const string UserQueryIdAttribute = "userqueryid";
    private const string UserQueryNameAttribute = "name";
    private const string UserQueryLayoutXmlAttribute = "layoutxml";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public UserQueryDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public string GetFetchXml(Guid savedQueryId)
    {
      return this.dataStore.Retrieve("userquery", savedQueryId, new ColumnSet(new string[1]
      {
        "fetchxml"
      })).GetAttributeValue<string>("fetchxml");
    }

    public string GetLayoutXml(Guid savedQueryId)
    {
      return this.dataStore.Retrieve("userquery", savedQueryId, new ColumnSet(new string[1]
      {
        "layoutxml"
      })).GetAttributeValue<string>("layoutxml");
    }

    public List<SavedView> GetUserViews(string entityName)
    {
      this.logger.AddCustomProperty("GetUserViews.Start", (object) entityName);
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<SavedView> userViews = new List<SavedView>();
      Stopwatch.StartNew();
      QueryExpression query = new QueryExpression();
      query.EntityName = "userquery";
      query.ColumnSet = new ColumnSet(new string[2]
      {
        "userqueryid",
        "name"
      });
      FilterExpression childFilter = new FilterExpression(LogicalOperator.Or);
      childFilter.AddCondition("returnedtypecode", ConditionOperator.Equal, (object) entityName);
      query.Criteria.AddFilter(childFilter);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) 1);
      query.Criteria.AddCondition("querytype", ConditionOperator.Equal, (object) 0);
      try
      {
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(query);
        int num1;
        if (entityCollection == null)
        {
          num1 = 0;
        }
        else
        {
          int? count = entityCollection.Entities?.Count;
          int num2 = 0;
          num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
        }
        if (num1 != 0)
          userViews = entityCollection.Entities.Select<Entity, SavedView>((Func<Entity, SavedView>) (e => new SavedView()
          {
            SavedQueryId = e.GetAttributeValue<Guid>("userqueryid"),
            Name = e.GetAttributeValue<string>("name"),
            IsDefault = false,
            IsUserQuery = true
          })).ToList<SavedView>();
        this.logger.AddCustomProperty("GetUserViews.Entities.Count", (object) entityCollection?.Entities?.Count);
      }
      catch (Exception ex)
      {
        this.logger.AddCustomProperty("GetUserViews.Exception", (object) ex);
      }
      stopwatch.Stop();
      this.logger.AddCustomProperty("GetUserViews.Duration", (object) stopwatch.ElapsedMilliseconds);
      this.logger.AddCustomProperty("GetUserViews.End", (object) "Success");
      return userViews;
    }
  }
}
