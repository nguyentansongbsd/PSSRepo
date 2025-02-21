// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.WorkQueueStateDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class WorkQueueStateDataAccess
  {
    public const string WorkQueueState = "msdyn_workqueuestate";
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public WorkQueueStateDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static string GetWorkQueueStateFetchXml(string entityName, List<string> entityIDs)
    {
      return "<fetch mapping='logical'>\r\n                   <entity name='msdyn_workqueuestate'>\r\n                      <attribute name='msdyn_nextactionid' />\r\n                      <attribute name='msdyn_isread' />\r\n                      <attribute name='msdyn_workqueuestateid' />\r\n                      <filter type='and'>\r\n                         <condition attribute='ownerid' operator='eq-userid' />\r\n                         <condition attribute='msdyn_nextactionid' operator='in'>" + QueryFilterHelpers.FormInConditionString(entityIDs) + "</condition>\r\n                      </filter>\r\n                   </entity>\r\n                </fetch>";
    }

    public EntityCollection FetchRecords(
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null,
      int topCount = 250)
    {
      this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.Start", (object) "Success");
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression queryExpression1 = new QueryExpression("msdyn_workqueuestate");
        queryExpression1.ColumnSet = new ColumnSet(new string[3]
        {
          "msdyn_nextactionid",
          "msdyn_isread",
          "msdyn_workqueuestateid"
        });
        QueryExpression queryExpression2 = queryExpression1;
        FilterExpression filterExpression = new FilterExpression();
        filterExpression.FilterOperator = LogicalOperator.And;
        filterExpression.Conditions.Add(new ConditionExpression("ownerid", ConditionOperator.EqualUserId));
        queryExpression2.Criteria = filterExpression;
        QueryExpression query = queryExpression1;
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.AdditionalAttributes", (object) additionalAttributes);
          query.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.AdditionalFilters", (object) additionalFilters);
          foreach (QueryFilter additionalFilter in additionalFilters)
            query.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          query.Orders.Add(sort.ToOrderExpression());
        if (paginationInfo != null)
        {
          query.PageInfo.Count = paginationInfo.PageCount;
          query.PageInfo.PageNumber = paginationInfo.PageNumber;
          query.PageInfo.PagingCookie = paginationInfo.PagingCookie;
          this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.PageCount", (object) query.PageInfo.Count);
          this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.PageNumber", (object) query.PageInfo.PageNumber);
        }
        else
          query.TopCount = new int?(topCount);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(query);
        stopwatch.Stop();
        this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.Count", (object) entityCollection.Entities.Count);
        this.logger.LogInfo("WorkQueueStateDataAccess.FetchRecords.Success", callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueStateDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.Exception", (object) ex);
        this.logger.LogError("WorkQueueStateDataAccess.FetchRecords.Failure", callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\WorkQueueStateDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.Duration", (object) stopwatch.ElapsedMilliseconds);
        this.logger.AddCustomProperty("WorkQueueStateDataAccess.FetchRecords.End", (object) "Success");
      }
    }
  }
}
