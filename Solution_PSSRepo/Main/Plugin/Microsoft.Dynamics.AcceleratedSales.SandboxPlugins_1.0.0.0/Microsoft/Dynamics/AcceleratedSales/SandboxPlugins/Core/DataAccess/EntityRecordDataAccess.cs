// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.EntityRecordDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class EntityRecordDataAccess
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public EntityRecordDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public EntityCollection FetchEntityRecords(QueryExpression queryExpression)
    {
      if (queryExpression == null)
        throw new ArgumentNullException(nameof (queryExpression));
      this.logger.LogWarning("EntityRecordDataAccess.FetchEntityRecords.EntityName: " + queryExpression.EntityName, callerName: nameof (FetchEntityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(queryExpression);
        this.logger.LogWarning("EntityRecordDataAccess.FetchEntityRecords.Count: " + entityCollection?.Entities?.Count.ToString(), callerName: nameof (FetchEntityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("EntityRecordDataAccess.FetchEntityRecords.Exception", ex, callerName: nameof (FetchEntityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("EntityRecordDataAccess.FetchEntityRecords.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchEntityRecords), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
      }
    }

    public EntityMetadata FetchEntityMetadata(string entityName)
    {
      if (string.IsNullOrEmpty(entityName))
        throw new ArgumentException(nameof (entityName));
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        if (this.dataStore.Execute((OrganizationRequest) new RetrieveEntityRequest()
        {
          RetrieveAsIfPublished = false,
          LogicalName = entityName,
          EntityFilters = (EntityFilters.Attributes | EntityFilters.Relationships)
        }) is RetrieveEntityResponse retrieveEntityResponse && retrieveEntityResponse.EntityMetadata != null)
          return retrieveEntityResponse.EntityMetadata;
        this.logger.LogWarning("EntityRecordDataAccess.FetchEntityMetadata.Response.IsNull", callerName: nameof (FetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
        return (EntityMetadata) null;
      }
      catch (Exception ex)
      {
        this.logger.LogError("FetchEntityMetadata API Failure", ex, callerName: nameof (FetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning(string.Format("FetchEntityMetadata.Completed.Entityname: {0}.Duration: {1}", (object) entityName, (object) stopwatch.ElapsedMilliseconds), callerName: nameof (FetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\EntityRecordDataAccess.cs");
      }
    }
  }
}
