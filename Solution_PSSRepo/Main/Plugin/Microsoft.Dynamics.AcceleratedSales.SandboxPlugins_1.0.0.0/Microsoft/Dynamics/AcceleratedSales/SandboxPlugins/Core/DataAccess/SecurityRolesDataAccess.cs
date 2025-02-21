// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SecurityRolesDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class SecurityRolesDataAccess
  {
    private const string Role = "role";
    private const string RoleId = "roleid";
    private const string RoleTemplateId = "roletemplateid";
    private const string ParentRootRoleid = "parentrootroleid";
    private const string SystemUserRoles = "systemuserroles";
    private const string SystemUserId = "systemuserid";
    private const string TeamRoles = "teamroles";
    private const string TeamId = "teamid";
    private const string TeamMembership = "teammembership";
    private const string SystemAdminRoleTemplateId = "627090FF-40A3-4053-8790-584EDC5BE201";
    private IDataStore dataStore;
    private IAcceleratedSalesLogger logger;

    public SecurityRolesDataAccess(IDataStore dataStore, IAcceleratedSalesLogger logger)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public EntityCollection FetchRolesForCurrentUser()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression query = new QueryExpression()
        {
          EntityName = "role",
          ColumnSet = new ColumnSet(new string[2]
          {
            "roleid",
            "parentrootroleid"
          }),
          Distinct = true
        };
        LinkEntity linkEntity = new LinkEntity()
        {
          LinkFromEntityName = "role",
          LinkFromAttributeName = "roleid",
          LinkToEntityName = "systemuserroles",
          LinkToAttributeName = "roleid"
        };
        linkEntity.LinkCriteria.AddCondition(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
        query.LinkEntities.Add(linkEntity);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(query);
        stopwatch.Stop();
        this.logger.LogWarning("SecurityRolesDataAccess.FetchRecords.Count: " + entityCollection.Entities.Count.ToString(), callerName: nameof (FetchRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SecurityRolesDataAccess.FetchRecords.Exception", ex, callerName: nameof (FetchRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SecurityRolesDataAccess.FetchRecords.end.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
      }
    }

    public RolePrivilege[] GetUserPrivilagesByName(string privilegeName)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        if (privilegeName == null)
          throw new ArgumentNullException(nameof (privilegeName));
        return ((RetrieveUserPrivilegeByPrivilegeNameResponse) this.dataStore.Elevate().Execute((OrganizationRequest) new RetrieveUserPrivilegeByPrivilegeNameRequest()
        {
          PrivilegeName = privilegeName,
          UserId = this.dataStore.RetrieveUserId()
        })).RolePrivileges;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SecurityRolesDataAccess.GetUserPrivilagesByName.Exception", ex, callerName: nameof (GetUserPrivilagesByName), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        return (RolePrivilege[]) null;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SecurityRolesDataAccess.GetUserPrivilagesByName.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (GetUserPrivilagesByName), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
      }
    }

    public EntityCollection FetchTeamRolesForCurrentUser()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        QueryExpression query = new QueryExpression()
        {
          EntityName = "role",
          ColumnSet = new ColumnSet(new string[2]
          {
            "roleid",
            "parentrootroleid"
          }),
          Distinct = true
        };
        LinkEntity linkEntity1 = new LinkEntity()
        {
          LinkFromEntityName = "role",
          LinkFromAttributeName = "roleid",
          LinkToEntityName = "teamroles",
          LinkToAttributeName = "roleid"
        };
        LinkEntity linkEntity2 = new LinkEntity()
        {
          LinkFromEntityName = "teamroles",
          LinkFromAttributeName = "teamid",
          LinkToEntityName = "teammembership",
          LinkToAttributeName = "teamid"
        };
        linkEntity2.LinkCriteria.AddCondition(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
        linkEntity1.LinkEntities.Add(linkEntity2);
        query.LinkEntities.Add(linkEntity1);
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(query);
        stopwatch.Stop();
        this.logger.LogWarning("SecurityRolesDataAccess.FetchTeamRolesForCurrentUser.Count: " + entityCollection.Entities.Count.ToString(), callerName: nameof (FetchTeamRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        return entityCollection;
      }
      catch (Exception ex)
      {
        this.logger.LogError("SecurityRolesDataAccess.FetchTeamRolesForCurrentUser.Exception", ex, callerName: nameof (FetchTeamRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        throw;
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("SecurityRolesDataAccess.FetchTeamRolesForCurrentUser.End.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (FetchTeamRolesForCurrentUser), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
      }
    }

    public bool TryCheckUserIsSystemAdminOrNot()
    {
      QueryExpression query1 = new QueryExpression("role");
      query1.Criteria.AddCondition("roletemplateid", ConditionOperator.Equal, (object) "627090FF-40A3-4053-8790-584EDC5BE201");
      query1.AddLink("systemuserroles", "roleid", "roleid").LinkCriteria.AddCondition("systemuserid", ConditionOperator.EqualUserId);
      if (this.dataStore.RetrieveMultiple(query1).Entities.Count > 0)
      {
        this.logger.LogWarning("SecurityRolesDataAccess.TryCheckUserIsSystemAdminOrNot: User role matched to admin", callerName: nameof (TryCheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        return true;
      }
      QueryExpression query2 = new QueryExpression("teamroles");
      query2.ColumnSet = new ColumnSet(new string[1]
      {
        "roleid"
      });
      query2.AddLink("teammembership", "teamid", "teamid").LinkCriteria.AddCondition(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
      query2.AddLink("role", "roleid", "roleid").LinkCriteria.AddCondition("roletemplateid", ConditionOperator.Equal, (object) "627090FF-40A3-4053-8790-584EDC5BE201");
      if (this.dataStore.RetrieveMultiple(query2).Entities.Count > 0)
      {
        this.logger.LogWarning("SecurityRolesDataAccess.TryCheckUserIsSystemAdminOrNot: Team role matched to admin", callerName: nameof (TryCheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
        return true;
      }
      this.logger.LogWarning("SecurityRolesDataAccess.TryCheckUserIsSystemAdminOrNot: No admin role", callerName: nameof (TryCheckUserIsSystemAdminOrNot), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\SecurityRolesDataAccess.cs");
      return false;
    }
  }
}
