// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.WorklistViewConfiguration
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.AcceleratedSales.Shared.SandboxServices.Platform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model
{
  public class WorklistViewConfiguration
  {
    [JsonProperty(PropertyName = "paginationInfo")]
    public PaginationInfo PaginationInfo { get; set; }

    [JsonProperty(PropertyName = "viewId")]
    public string ViewId { get; set; }

    [JsonProperty(PropertyName = "fetchXml")]
    public string FetchXml { get; set; }

    [JsonProperty(PropertyName = "savedQueryEntityName")]
    public string SavedQueryEntityName { get; set; }

    [JsonProperty(PropertyName = "savedQueryId")]
    public string SavedQueryId { get; set; }

    [JsonProperty(PropertyName = "entityName")]
    public string EntityName { get; set; }

    [JsonProperty(PropertyName = "userLocaleId")]
    public int UserLocaleId { get; set; }

    [JsonProperty(PropertyName = "attributes")]
    public List<string> Attributes { get; set; }

    [JsonProperty(PropertyName = "primaryRecordIds")]
    public List<string> PrimaryRecordIds { get; set; }

    [JsonProperty(PropertyName = "filters")]
    public Dictionary<string, List<QueryFilter>> Filters { get; set; }

    [JsonProperty(PropertyName = "sort")]
    public Dictionary<string, QuerySort> Sort { get; set; }

    [JsonProperty(PropertyName = "search")]
    public string Search { get; set; }

    [JsonProperty(PropertyName = "parsedSearchedDate")]
    public string ParsedSearchedDate { get; set; }

    [JsonProperty(PropertyName = "relatedEntities")]
    public List<string> RelatedEntities { get; set; }

    [JsonProperty(PropertyName = "topCount")]
    public int TopCount { get; set; }

    [JsonProperty(PropertyName = "resultSize")]
    public int ResultSize { get; set; }

    [JsonProperty(PropertyName = "columns")]
    public List<ColumnsData> Columns { get; set; }

    [JsonProperty(PropertyName = "isAdminUserCheckNeeded")]
    public bool IsAdminUserCheckNeeded { get; set; }

    public static WorklistViewConfiguration FromRequestPayload(
      GetWorklistDataRequest requestPayload,
      IAcceleratedSalesLogger logger)
    {
      WorklistViewConfiguration viewConfiguration;
      try
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        viewConfiguration = JsonConvert.DeserializeObject<WorklistViewConfiguration>(requestPayload?.Payload);
        stopwatch.Stop();
        logger.AddCustomProperty("WorklistViewConfiguration.FromRequestPayload.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        logger.AddCustomProperty("WorklistViewConfiguration.FromRequestPayload.Exception", (object) ex);
        logger.AddCustomProperty("WorklistViewConfiguration.FromRequestPayload.ExceptionStackTrace", (object) ex.StackTrace);
        throw new CrmException("Invalid request.", ex, 1879506952);
      }
      WorklistViewConfiguration.ValidatedThrowErrorIfRequiredParametersMissing(viewConfiguration);
      WorklistViewConfiguration.SetViewConfigDefaults(viewConfiguration);
      return viewConfiguration;
    }

    public static WorklistViewConfiguration FromRequestPayloadForSA(
      GetWorklistDataRequest requestPayload,
      IAcceleratedSalesLogger logger)
    {
      WorklistViewConfiguration viewConfiguration;
      try
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        viewConfiguration = JsonConvert.DeserializeObject<WorklistViewConfiguration>(requestPayload?.Payload);
        stopwatch.Stop();
        logger.AddCustomProperty("WorklistViewConfiguration.FromWorklistDataRequestPayload.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        logger.AddCustomProperty("WorklistViewConfiguration.FromWorklistDataRequestPayload.Exception", (object) ex);
        logger.AddCustomProperty("WorklistViewConfiguration.FromWorklistDataRequestPayload.ExceptionStackTrace", (object) ex.StackTrace);
        throw new CrmException("Invalid request.", ex, 1879506952);
      }
      WorklistViewConfiguration.ValidatedThrowErrorIfRequiredParametersMissingForSA(viewConfiguration);
      WorklistViewConfiguration.SetViewConfigDefaults(viewConfiguration);
      return viewConfiguration;
    }

    public static WorklistViewConfiguration FromFilteredDataRequestPayload(
      GetWorklistFilteredDataRequest requestPayload,
      IAcceleratedSalesLogger logger)
    {
      WorklistViewConfiguration viewConfiguration;
      try
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        viewConfiguration = JsonConvert.DeserializeObject<WorklistViewConfiguration>(requestPayload?.Payload);
        stopwatch.Stop();
        logger.AddCustomProperty("WorklistViewConfiguration.FromFilteredDataRequestPayload.Duration", (object) stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        throw new CrmException("Invalid request.", ex, 1879506952);
      }
      WorklistViewConfiguration.ValidatedThrowErrorIfRequiredParametersMissing(viewConfiguration);
      WorklistViewConfiguration.SetViewConfigDefaults(viewConfiguration);
      return viewConfiguration;
    }

    public static List<string> GetRelatedEntities(string viewId)
    {
      if (viewId == "salesaccelerator")
        return new List<string>()
        {
          "msdyn_salessuggestion",
          "msdyn_sequencetargetstep",
          "activitypointer",
          "msdyn_workqueuestate"
        };
      return new List<string>()
      {
        "activitypointer",
        "msdyn_sequencetargetstep"
      };
    }

    private static void SetViewConfigDefaults(WorklistViewConfiguration worklistViewConfig)
    {
      worklistViewConfig.RelatedEntities = worklistViewConfig.RelatedEntities ?? WorklistViewConfiguration.GetRelatedEntities(worklistViewConfig.ViewId);
      worklistViewConfig.Attributes = worklistViewConfig.Attributes ?? new List<string>();
      worklistViewConfig.UserLocaleId = worklistViewConfig.UserLocaleId == 0 ? 1033 : worklistViewConfig.UserLocaleId;
      worklistViewConfig.ViewId = worklistViewConfig.ViewId ?? "entitylistview";
      worklistViewConfig.TopCount = worklistViewConfig.TopCount == 0 ? 250 : worklistViewConfig.TopCount;
      worklistViewConfig.ResultSize = worklistViewConfig.ResultSize == 0 ? 250 : worklistViewConfig.ResultSize;
      WorklistViewConfiguration viewConfiguration = worklistViewConfig;
      PaginationInfo paginationInfo = worklistViewConfig.PaginationInfo;
      if (paginationInfo == null)
        paginationInfo = new PaginationInfo()
        {
          PageNumber = 1,
          PagingCookie = string.Empty,
          PageCount = 250
        };
      viewConfiguration.PaginationInfo = paginationInfo;
      worklistViewConfig.Filters = worklistViewConfig.Filters ?? new Dictionary<string, List<QueryFilter>>();
      worklistViewConfig.Sort = worklistViewConfig.Sort ?? new Dictionary<string, QuerySort>();
      worklistViewConfig.Search = worklistViewConfig.Search ?? string.Empty;
      worklistViewConfig.ParsedSearchedDate = worklistViewConfig.ParsedSearchedDate ?? string.Empty;
      worklistViewConfig.Columns = worklistViewConfig.Columns ?? new List<ColumnsData>();
    }

    private static void ValidatedThrowErrorIfRequiredParametersMissing(
      WorklistViewConfiguration config)
    {
      if (config == null || string.IsNullOrWhiteSpace(config.FetchXml) && string.IsNullOrWhiteSpace(config.SavedQueryId) || string.IsNullOrWhiteSpace(config.ViewId) || config.ViewId != "entitylistview" || string.IsNullOrWhiteSpace(config.EntityName))
        throw new CrmException("WorklistViewConfiguration.RequiredRequestParametersMissing", 1879441412);
    }

    private static void ValidatedThrowErrorIfRequiredParametersMissingForSA(
      WorklistViewConfiguration config)
    {
      if (config == null || string.IsNullOrEmpty(config.ViewId) || config.ViewId != "salesaccelerator")
        throw new CrmException("WorklistViewConfiguration.RequiredRequestParametersMissing", 1879441410);
    }
  }
}
