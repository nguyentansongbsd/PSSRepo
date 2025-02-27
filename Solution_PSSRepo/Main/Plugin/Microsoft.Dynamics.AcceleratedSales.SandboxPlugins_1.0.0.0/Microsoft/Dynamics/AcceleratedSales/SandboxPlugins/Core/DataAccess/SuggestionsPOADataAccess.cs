// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SuggestionsPOADataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SuggestionsPOADataAccess
  {
    private const int QueryResultSetLimit = 500;
    private const string SuggestionsPOAEntityName = "msdyn_suggestionprincipalobjectaccess";
    private const string PrincipalId = "msdyn_principalid";
    private const string SuggestionId = "msdyn_salessuggestionid";
    private const string AccessRightsMask = "msdyn_accessrightsmask";
    private const string StateCode = "statecode";
    private const int OpenStateCode = 0;
    private readonly IAcceleratedSalesLogger logger;
    private readonly IDataStore dataStore;
    private readonly string[] suggestionPrincipalObjectAccessAttributes = new string[3]
    {
      "msdyn_principalid",
      "msdyn_accessrightsmask",
      "msdyn_salessuggestionid"
    };

    public SuggestionsPOADataAccess(IAcceleratedSalesLogger logger, IDataStore dataStore)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public Dictionary<Guid, int> FetchSuggestionPrincipalObjectAccessRecords(
      object[] accessRightsMaskFilter,
      int queryLimit = 500)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchSuggestionPrincipalObjectAccessRecords.AccessRightsMaskFilter", accessRightsMaskFilter != null ? (object) JsonConvert.SerializeObject((object) accessRightsMaskFilter) : (object) string.Empty);
      try
      {
        FilterExpression childFilter = new FilterExpression();
        QueryExpression query = new QueryExpression();
        int num = Math.Min(queryLimit * 2, 5000);
        query.TopCount = new int?(num);
        query.EntityName = "msdyn_suggestionprincipalobjectaccess";
        query.ColumnSet.AddColumns(this.suggestionPrincipalObjectAccessAttributes);
        childFilter.AddCondition("msdyn_principalid", ConditionOperator.EqualUserOrUserTeams);
        childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
        childFilter.AddCondition("msdyn_accessrightsmask", ConditionOperator.In, accessRightsMaskFilter);
        query.Criteria.AddFilter(childFilter);
        EntityCollection suggestionPOAEntityCollection = this.dataStore.RetrieveMultiple(query);
        stopwatch.Stop();
        this.logger.LogWarning("SuggestionsPOADataAccess.FetchSuggestionPrincipalObjectAccessRecords.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchSuggestionPrincipalObjectAccessRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsPOADataAccess.cs");
        this.logger.LogWarning("SuggestionsPOADataAccess.FetchSuggestionPrincipalObjectAccessRecords.Count: " + suggestionPOAEntityCollection.Entities.Count.ToString(), callerName: nameof (FetchSuggestionPrincipalObjectAccessRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsPOADataAccess.cs");
        return this.ProcessSuggestionPrincipalObjectAccessRecords(suggestionPOAEntityCollection);
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        this.logger.LogError("SuggestionsPOADataAccess.FetchSuggestionPrincipalObjectAccessRecords.Exception", ex, callerName: nameof (FetchSuggestionPrincipalObjectAccessRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsPOADataAccess.cs");
        return new Dictionary<Guid, int>();
      }
    }

    public EntityCollection FetchRecords(
      string entityName,
      QueryFilter[] additionalFilters = null,
      string[] additionalAttributes = null,
      QuerySort sort = null,
      int topCount = 250)
    {
      this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.Start", (object) "Success");
      this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.EntityName", (object) entityName);
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression forSuggestionPoa = this.GetQueryExpressionForSuggestionPOA();
        if (additionalAttributes != null && additionalAttributes.Length != 0)
        {
          this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.AdditionalAttributes", (object) additionalAttributes.Length);
          forSuggestionPoa.ColumnSet.AddColumns(additionalAttributes);
        }
        if (additionalFilters != null && additionalFilters.Length != 0)
        {
          this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.AdditionalFilters", (object) additionalFilters.Length);
          foreach (QueryFilter additionalFilter in additionalFilters)
          {
            if (additionalFilter.AttributeName == "msdyn_accessrightsmask")
            {
              switch (additionalFilter.Operator)
              {
                case ConditionOperator.Equal:
                  forSuggestionPoa.Criteria.AddCondition("msdyn_accessrightsmask", additionalFilter.Operator, (object) (int) Enum.Parse(typeof (OwnershipType), additionalFilter.Values[0].ToString()));
                  break;
                case ConditionOperator.In:
                  List<object> objectList = new List<object>();
                  foreach (object obj in additionalFilter.Values)
                  {
                    if (Enum.TryParse<OwnershipType>(obj.ToString(), out OwnershipType _))
                    {
                      int num = (int) Enum.Parse(typeof (OwnershipType), obj.ToString());
                      objectList.Add((object) num);
                    }
                  }
                  forSuggestionPoa.Criteria.AddCondition("msdyn_accessrightsmask", additionalFilter.Operator, objectList.ToArray());
                  break;
              }
              forSuggestionPoa.Criteria.AddCondition(additionalFilter.ToConditionExpression());
            }
            else
              forSuggestionPoa.Criteria.AddCondition(additionalFilter.ToConditionExpression());
          }
        }
        if (sort != null)
          forSuggestionPoa.Orders.Add(sort.ToOrderExpression());
        forSuggestionPoa.TopCount = new int?(topCount);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(forSuggestionPoa);
        this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.Count", (object) entityCollection.Entities.Count);
        this.logger.LogInfo("SuggestionsPOADataAccess.FetchRecords.Success", callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsPOADataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.Exception", (object) ex);
        this.logger.LogError("SuggestionsPOADataAccess.FetchRecords.Failure", callerName: nameof (FetchRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SuggestionsPOADataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.Duration", (object) stopwatch.ElapsedMilliseconds);
        this.logger.AddCustomProperty("SuggestionsPOADataAccess.FetchRecords.End", (object) "Success");
      }
    }

    private QueryExpression GetQueryExpressionForSuggestionPOA()
    {
      QueryExpression forSuggestionPoa = new QueryExpression();
      forSuggestionPoa.EntityName = "msdyn_suggestionprincipalobjectaccess";
      forSuggestionPoa.ColumnSet.AddColumns("msdyn_principalid", "msdyn_accessrightsmask", "msdyn_salessuggestionid");
      FilterExpression childFilter = new FilterExpression();
      childFilter.AddCondition("msdyn_principalid", ConditionOperator.EqualUserOrUserTeams);
      childFilter.AddCondition("statecode", ConditionOperator.Equal, (object) 0);
      forSuggestionPoa.Criteria.AddFilter(childFilter);
      return forSuggestionPoa;
    }

    private Dictionary<Guid, int> ProcessSuggestionPrincipalObjectAccessRecords(
      EntityCollection suggestionPOAEntityCollection)
    {
      Dictionary<Guid, int> dictionary = new Dictionary<Guid, int>();
      if (suggestionPOAEntityCollection.Entities.Count > 0)
      {
        foreach (Entity entity in (Collection<Entity>) suggestionPOAEntityCollection.Entities)
        {
          EntityReference attributeValue = entity.TryGetAttributeValue<EntityReference>("msdyn_salessuggestionid", new EntityReference());
          int num = entity.TryGetAttributeValue<OptionSetValue>("msdyn_accessrightsmask", new OptionSetValue()).Value;
          if (attributeValue != null && num != 0)
            dictionary.Add(attributeValue.Id, num);
        }
      }
      return dictionary;
    }
  }
}
