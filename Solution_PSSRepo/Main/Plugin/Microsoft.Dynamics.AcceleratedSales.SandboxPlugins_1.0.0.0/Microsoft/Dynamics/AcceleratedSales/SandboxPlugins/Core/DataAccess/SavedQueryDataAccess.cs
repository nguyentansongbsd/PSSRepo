// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SavedQueryDataAccess
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
  public class SavedQueryDataAccess
  {
    private const string SavedQueryEntityName = "savedquery";
    private const string SavedQueryFetchXmlAttribute = "fetchxml";
    private const string SavedQueryLayoutXmlAttribute = "layoutxml";
    private const string SavedQueryIdAttribute = "savedqueryid";
    private const string SavedQueryNameAttribute = "name";
    private const string SavedQueryIsDefaultAttribute = "isdefault";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public SavedQueryDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public string GetFetchXml(Guid savedQueryId)
    {
      return this.dataStore.Retrieve("savedquery", savedQueryId, new ColumnSet(new string[1]
      {
        "fetchxml"
      })).GetAttributeValue<string>("fetchxml");
    }

    public string GetLayoutXml(Guid savedQueryId)
    {
      return this.dataStore.Retrieve("savedquery", savedQueryId, new ColumnSet(new string[1]
      {
        "layoutxml"
      })).GetAttributeValue<string>("layoutxml");
    }

    public List<SavedView> GetSystemViews(string entityName)
    {
      this.logger.AddCustomProperty("GetSystemViews.Start", (object) entityName);
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<SavedView> systemViews = new List<SavedView>();
      Stopwatch.StartNew();
      QueryExpression query = new QueryExpression();
      query.EntityName = "savedquery";
      query.ColumnSet = new ColumnSet(new string[3]
      {
        "savedqueryid",
        "name",
        "isdefault"
      });
      FilterExpression childFilter = new FilterExpression(LogicalOperator.Or);
      childFilter.AddCondition("returnedtypecode", ConditionOperator.Equal, (object) entityName);
      query.Criteria.AddFilter(childFilter);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) 1);
      query.Criteria.AddCondition("querytype", ConditionOperator.Equal, (object) 0);
      FilterExpression filterExpression = query.Criteria.AddFilter(LogicalOperator.Or);
      filterExpression.AddCondition("queryapi", ConditionOperator.Null);
      filterExpression.AddCondition("queryapi", ConditionOperator.Equal, (object) string.Empty);
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
          systemViews = entityCollection.Entities.Select<Entity, SavedView>((Func<Entity, SavedView>) (e => new SavedView()
          {
            SavedQueryId = e.GetAttributeValue<Guid>("savedqueryid"),
            Name = e.GetAttributeValue<string>("name"),
            IsDefault = e.GetAttributeValue<bool>("isdefault"),
            IsUserQuery = false
          })).ToList<SavedView>();
        this.logger.LogWarning("GetSystemViews.Entities.Count: " + entityCollection?.Entities?.Count.ToString(), callerName: nameof (GetSystemViews), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("GetSystemViews.Exception", ex, callerName: nameof (GetSystemViews), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      }
      stopwatch.Stop();
      this.logger.LogWarning(string.Format("GetSystemViews.End.Success.Duration: {0}", (object) stopwatch.ElapsedMilliseconds), callerName: nameof (GetSystemViews), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      return systemViews;
    }

    public string GetQuickFindFetchXml(string entityName)
    {
      this.logger.LogWarning("GetQuickFindFetchXml.Start: " + entityName, callerName: nameof (GetQuickFindFetchXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      Stopwatch stopwatch = Stopwatch.StartNew();
      string quickFindFetchXml = string.Empty;
      Stopwatch.StartNew();
      QueryExpression query = new QueryExpression();
      query.EntityName = "savedquery";
      query.ColumnSet = new ColumnSet(new string[2]
      {
        "savedqueryid",
        "fetchxml"
      });
      FilterExpression childFilter = new FilterExpression(LogicalOperator.Or);
      childFilter.AddCondition("returnedtypecode", ConditionOperator.Equal, (object) entityName);
      query.Criteria.AddFilter(childFilter);
      query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (object) 1);
      query.Criteria.AddCondition("querytype", ConditionOperator.Equal, (object) 4);
      query.Criteria.AddCondition("isquickfindquery", ConditionOperator.Equal, (object) true);
      query.Criteria.AddCondition("isdefault", ConditionOperator.Equal, (object) true);
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
          quickFindFetchXml = entityCollection.Entities[0].GetAttributeValue<string>("fetchxml");
        this.logger.LogWarning("GetQuickFindFetchXml.Entities.Count: " + entityCollection?.Entities?.Count.ToString(), callerName: nameof (GetQuickFindFetchXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      }
      catch (Exception ex)
      {
        this.logger.LogError("GetQuickFindFetchXml.Exception", ex, callerName: nameof (GetQuickFindFetchXml), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SavedQueryDataAccess.cs");
      }
      stopwatch.Stop();
      this.logger.AddCustomProperty("GetQuickFindFetchXml.End.Success.Duration: ", (object) stopwatch.ElapsedMilliseconds);
      return quickFindFetchXml;
    }
  }
}
