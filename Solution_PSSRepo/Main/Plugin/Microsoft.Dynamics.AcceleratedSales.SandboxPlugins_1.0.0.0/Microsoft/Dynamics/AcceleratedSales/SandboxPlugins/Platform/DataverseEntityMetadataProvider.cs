// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform.DataverseEntityMetadataProvider
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Platform
{
  public class DataverseEntityMetadataProvider : IEntityMetadataProvider
  {
    private const string WorkspaceFCSNamespace = "SalesService.Workspace";
    private const string EnablePreFetchMetadata = "EnablePreFetchMetadata";
    private readonly EntityRecordDataAccess entityDataAccess;
    private readonly IAcceleratedSalesLogger logger;
    private readonly Dictionary<string, EntityMetadata> metadataCache;
    private readonly IDataStore dataStore;

    public DataverseEntityMetadataProvider(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.entityDataAccess = new EntityRecordDataAccess(dataStore, logger);
      this.logger = logger;
      this.metadataCache = new Dictionary<string, EntityMetadata>();
    }

    public AttributeMetadata[] GetAttributes(string entityName)
    {
      return this.GetEntityMetadata(entityName)?.Attributes ?? new AttributeMetadata[0];
    }

    public EntityMetadata GetEntityMetadata(string entityName)
    {
      EntityMetadata entityMetadata1;
      if (this.metadataCache.TryGetValue(entityName, out entityMetadata1))
        return entityMetadata1;
      EntityMetadata entityMetadata2 = this.entityDataAccess.FetchEntityMetadata(entityName);
      this.metadataCache[entityName] = entityMetadata2;
      return entityMetadata2;
    }

    public string GetIconVectorUrl(string entityName)
    {
      string iconVectorName = this.GetEntityMetadata(entityName)?.IconVectorName;
      return !string.IsNullOrEmpty(iconVectorName) ? "/WebResources/" + iconVectorName : string.Empty;
    }

    public string GetObjectTypeIconUrl(string entityName)
    {
      EntityMetadata entityMetadata = this.GetEntityMetadata(entityName);
      return entityMetadata != null && !entityMetadata.IsCustomEntity.Value && entityMetadata.ObjectTypeCode.HasValue ? string.Format("/_imgs/svg_{0}.svg", (object) entityMetadata.ObjectTypeCode.Value) : string.Empty;
    }

    public OneToManyRelationshipMetadata[] GetOneToManyRelationships(string entityName)
    {
      return this.GetEntityMetadata(entityName)?.ManyToOneRelationships ?? new OneToManyRelationshipMetadata[0];
    }

    public OneToManyRelationshipMetadata[] GetManyToOneRelationships(string entityName)
    {
      return this.GetEntityMetadata(entityName)?.OneToManyRelationships ?? new OneToManyRelationshipMetadata[0];
    }

    public string GetPrimaryImageUrlAttributeName(string entityName)
    {
      string primaryImageAttribute = this.GetEntityMetadata(entityName)?.PrimaryImageAttribute;
      return !string.IsNullOrEmpty(primaryImageAttribute) ? primaryImageAttribute + "_url" : string.Empty;
    }

    public ExecuteMultipleRequest GetExecuteMultipleRequest(MetadataQueryParams metadataQueryParams)
    {
      new MetadataPropertiesExpression().AllProperties = false;
      ExecuteMultipleRequest executeMultipleRequest = new ExecuteMultipleRequest()
      {
        Settings = new ExecuteMultipleSettings()
        {
          ContinueOnError = true,
          ReturnResponses = true
        },
        Requests = new OrganizationRequestCollection()
      };
      if (metadataQueryParams.AllAttributesEntities.Count > 0)
      {
        EntityQueryExpression entityQueryExpression = this.GetBaseEntityQueryExpression();
        entityQueryExpression.Criteria.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.In, (object) metadataQueryParams.AllAttributesEntities.ToArray()));
        entityQueryExpression.LabelQuery.FilterLanguages.Add(metadataQueryParams.LocalId);
        entityQueryExpression.RelationshipQuery.Criteria.Conditions.Add(new MetadataConditionExpression("SchemaName", MetadataConditionOperator.In, (object) metadataQueryParams.Relationships.ToArray()));
        RetrieveMetadataChangesRequest metadataChangesRequest = new RetrieveMetadataChangesRequest()
        {
          Query = entityQueryExpression,
          ClientVersionStamp = (string) null,
          DeletedMetadataFilters = DeletedMetadataFilters.Entity
        };
        executeMultipleRequest.Requests.Add((OrganizationRequest) metadataChangesRequest);
      }
      EntityQueryExpression entityQueryExpression1 = this.GetBaseEntityQueryExpression();
      List<string> list = metadataQueryParams.Entities.Where<string>((Func<string, bool>) (e => !metadataQueryParams.AllAttributesEntities.Contains(e))).ToList<string>();
      entityQueryExpression1.Criteria.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.In, (object) list.ToArray()));
      entityQueryExpression1.AttributeQuery.Criteria = new MetadataFilterExpression(LogicalOperator.And);
      entityQueryExpression1.AttributeQuery.Criteria.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.In, (object) metadataQueryParams.Attributes.ToArray()));
      entityQueryExpression1.LabelQuery.FilterLanguages.Add(metadataQueryParams.LocalId);
      entityQueryExpression1.RelationshipQuery.Criteria.Conditions.Add(new MetadataConditionExpression("SchemaName", MetadataConditionOperator.In, (object) metadataQueryParams.Relationships.ToArray()));
      RetrieveMetadataChangesRequest metadataChangesRequest1 = new RetrieveMetadataChangesRequest()
      {
        Query = entityQueryExpression1
      };
      executeMultipleRequest.Requests.Add((OrganizationRequest) metadataChangesRequest1);
      return executeMultipleRequest;
    }

    public void PreFetchEntityMetadata(MetadataQueryParams metadataQueryParams)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      if (!this.dataStore.IsFCSEnabled("SalesService.Workspace", "EnablePreFetchMetadata"))
        this.logger.LogWarning("PreFetchEntityMetadata.Skipped: PreFetchMetadataDisabled", callerName: nameof (PreFetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Platform\\DataverseEntityMetadataProvider.cs");
      else if (metadataQueryParams == null)
      {
        this.logger.LogWarning("PreFetchEntityMetadata: Entity metadataQueryParams is null", callerName: nameof (PreFetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Platform\\DataverseEntityMetadataProvider.cs");
      }
      else
      {
        try
        {
          ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse) this.dataStore.Execute((OrganizationRequest) this.GetExecuteMultipleRequest(metadataQueryParams));
          int num = 0;
          foreach (ExecuteMultipleResponseItem response in (Collection<ExecuteMultipleResponseItem>) multipleResponse.Responses)
          {
            ExecuteMultipleResponseItem item = response;
            if (item.Fault != null)
            {
              this.logger.Execute("AcceleratedSales.DataverseEntityMetadataProvider.Logging", (Action) (() => this.logger.AddCustomProperty("WorklistDataProviderService.PreFetchEntityMetadata.Error.", (object) JsonConvert.SerializeObject((object) item, Formatting.None, new JsonSerializerSettings()
              {
                NullValueHandling = NullValueHandling.Ignore
              }))));
            }
            else
            {
              foreach (EntityMetadata entityMetadata in (Collection<EntityMetadata>) (item.Response as RetrieveMetadataChangesResponse).EntityMetadata)
              {
                if (!this.metadataCache.ContainsKey(entityMetadata.LogicalName))
                {
                  this.metadataCache[entityMetadata.LogicalName] = entityMetadata;
                  ++num;
                }
              }
            }
          }
          this.logger.LogWarning("PreFetchEntityMetadata.Success: " + num.ToString(), callerName: nameof (PreFetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Platform\\DataverseEntityMetadataProvider.cs");
        }
        catch (Exception ex)
        {
          this.logger.LogError("PreFetchEntityMetadata.Exception", ex, callerName: nameof (PreFetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Platform\\DataverseEntityMetadataProvider.cs");
        }
        finally
        {
          this.logger.LogWarning("PreFetchEntityMetadata.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (PreFetchEntityMetadata), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Platform\\DataverseEntityMetadataProvider.cs");
          stopwatch.Stop();
        }
      }
    }

    private EntityQueryExpression GetBaseEntityQueryExpression()
    {
      MetadataPropertiesExpression propertiesExpression = new MetadataPropertiesExpression()
      {
        AllProperties = false
      };
      propertiesExpression.PropertyNames.AddRange("Attributes", "OneToManyRelationships", "ManyToOneRelationships", "IconVectorName", "PrimaryImageAttribute", "PrimaryIdAttribute", "IsCustomEntity", "ObjectTypeCode", "DisplayName", "IsActivity");
      MetadataFilterExpression filterExpression = new MetadataFilterExpression(LogicalOperator.And);
      EntityQueryExpression entityQueryExpression = new EntityQueryExpression();
      entityQueryExpression.Properties = propertiesExpression;
      entityQueryExpression.Criteria = filterExpression;
      entityQueryExpression.AttributeQuery = this.GetBaseAttributQueryExpression();
      entityQueryExpression.LabelQuery = this.GetBaseLabelQueryExpression();
      entityQueryExpression.RelationshipQuery = this.GetBaseRelationshipQueryExpression();
      return entityQueryExpression;
    }

    private AttributeQueryExpression GetBaseAttributQueryExpression()
    {
      MetadataPropertiesExpression propertiesExpression = new MetadataPropertiesExpression()
      {
        AllProperties = false
      };
      propertiesExpression.PropertyNames.AddRange("OptionSet", "AttributeType", "SchemaName", "LogicalName", "DisplayName", "Targets");
      AttributeQueryExpression attributQueryExpression = new AttributeQueryExpression();
      attributQueryExpression.Properties = propertiesExpression;
      return attributQueryExpression;
    }

    private LabelQueryExpression GetBaseLabelQueryExpression() => new LabelQueryExpression();

    private RelationshipQueryExpression GetBaseRelationshipQueryExpression()
    {
      MetadataPropertiesExpression propertiesExpression = new MetadataPropertiesExpression()
      {
        AllProperties = false
      };
      propertiesExpression.PropertyNames.AddRange("ReferencedEntity", "ReferencingEntity", "ReferencingAttribute", "ReferencedAttribute", "SchemaName", "IsCustomRelationship");
      RelationshipQueryExpression relationshipQueryExpression = new RelationshipQueryExpression();
      relationshipQueryExpression.Properties = propertiesExpression;
      return relationshipQueryExpression;
    }
  }
}
