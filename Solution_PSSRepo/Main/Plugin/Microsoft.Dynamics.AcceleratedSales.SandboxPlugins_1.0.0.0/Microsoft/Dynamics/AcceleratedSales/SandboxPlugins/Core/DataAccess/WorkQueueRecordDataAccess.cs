// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.WorkQueueRecordDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using System.Collections.Generic;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class WorkQueueRecordDataAccess
  {
    private readonly IDataStore dataStore;
    private readonly IAcceleratedSalesLogger logger;

    public WorkQueueRecordDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public static List<string> GetWorkQueueRecordAttributes()
    {
      return new List<string>()
      {
        "createdby",
        "createdon",
        "createdonbehalfby",
        "importsequencenumber",
        "modifiedby",
        "modifiedon",
        "modifiedonbehalfby",
        "msdyn_displayattributes",
        "msdyn_duetime",
        "msdyn_endtime",
        "msdyn_entitysetname",
        "msdyn_entitytypecode",
        "msdyn_entitytypedisplayname",
        "msdyn_entitytypelogicalname",
        "msdyn_filterattributes",
        "msdyn_linkedactivityid",
        "msdyn_nextactionerrorstate",
        "msdyn_nextactionid",
        "msdyn_nextactionname",
        "msdyn_nextactionsource",
        "msdyn_nextactionsubtype",
        "msdyn_nextactiontype",
        "msdyn_nextactiontypedisplayname",
        "msdyn_nextactionwaitstate",
        "msdyn_operationparameter",
        "msdyn_primaryentityid",
        "msdyn_primaryname",
        "msdyn_prioritygrade",
        "msdyn_priorityscore",
        "msdyn_sequenceid",
        "msdyn_sequencename",
        "msdyn_sequencestepid",
        "msdyn_sortattributes",
        "msdyn_workqueuerecordid"
      };
    }
  }
}
