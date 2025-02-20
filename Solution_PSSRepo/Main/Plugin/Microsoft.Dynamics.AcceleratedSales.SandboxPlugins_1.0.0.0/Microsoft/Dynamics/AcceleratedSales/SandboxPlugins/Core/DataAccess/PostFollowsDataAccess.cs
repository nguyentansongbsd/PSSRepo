// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.PostFollowsDataAccess
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
  public class PostFollowsDataAccess
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public PostFollowsDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static string GetFollowFetchXml(string entityName, List<string> entityIDs)
    {
      return "<fetch mapping='logical'>\r\n            <entity name='postfollow'>\r\n               <attribute name='regardingobjectid' />\r\n               <attribute name='postfollowid' />\r\n               <filter type='and'>\r\n                  <condition attribute='ownerid' operator='eq-userid' />\r\n                  <condition attribute='regardingobjectid' operator='in'>" + QueryFilterHelpers.FormInConditionString(entityIDs) + "</condition>\r\n               </filter>\r\n            </entity>\r\n         </fetch>";
    }

    public EntityCollection FetchRecords(
      string entityName,
      List<string> entityIDs,
      PaginationInfo paginationInfo = null,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null)
    {
      Dictionary<string, object> customProperties = new Dictionary<string, object>()
      {
        {
          "PostFollowDataAccess.FetchRecords.EntityName",
          (object) (entityName ?? "is null")
        }
      };
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        string followFetchXml = PostFollowsDataAccess.GetFollowFetchXml(entityName, entityIDs);
        customProperties.Add("PostFollowsDataAccess.FetchRecords.ConvertFetchXmlToQueryExpression.Start", (object) followFetchXml);
        QueryExpression queryExpression = this.dataStore.ConvertFetchXmlToQueryExpression(followFetchXml, this.logger);
        customProperties.Add("PostFollowsDataAccess.FetchRecords.ConvertFetchXmlToQueryExpression.End", (object) stopwatch.ElapsedMilliseconds);
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          customProperties.Add("PostFollowDataAccess.FetchRecords.AdditionalAttributes", (object) additionalAttributes);
          queryExpression.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          customProperties.Add("PostFollowDataAccess.FetchRecords.AdditionalFilters", (object) additionalFilters);
          foreach (QueryFilter additionalFilter in additionalFilters)
            queryExpression.Criteria.AddCondition(additionalFilter.ToConditionExpression());
        }
        if (sort != null)
          queryExpression.Orders.Add(sort.ToOrderExpression());
        if (paginationInfo != null)
        {
          queryExpression.PageInfo.Count = paginationInfo.PageCount;
          queryExpression.PageInfo.PageNumber = paginationInfo.PageNumber;
          queryExpression.PageInfo.PagingCookie = paginationInfo.PagingCookie;
          this.logger.AddCustomProperty("PostFollowDataAccess.FetchRecords.PageCount", (object) queryExpression.PageInfo.Count);
          this.logger.AddCustomProperty("PostFollowDataAccess.FetchRecords.PageNumber", (object) queryExpression.PageInfo.PageNumber);
        }
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(queryExpression);
        stopwatch.Stop();
        customProperties.Add("PostFollowDataAccess.FetchRecords.Count", (object) entityCollection.Entities.Count);
        customProperties.Add("PostFollowsDataAccess.FetchRecords.Duration", (object) stopwatch.ElapsedMilliseconds);
        this.logger.LogInfo("PostFollowsDataAccess.FetchRecords.Success", customProperties, nameof (FetchRecords), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\PostFollowsDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        customProperties.Add("PostFollowsDataAccess.FetchRecords.Duration", (object) stopwatch.ElapsedMilliseconds);
        customProperties.Add("PostFollowDataAccess.FetchRecords.Exception", (object) ex);
        this.logger.LogError("PostFollowsDataAccess.FetchRecords.Failure", customProperties, nameof (FetchRecords), "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\PostFollowsDataAccess.cs");
        throw;
      }
    }
  }
}
