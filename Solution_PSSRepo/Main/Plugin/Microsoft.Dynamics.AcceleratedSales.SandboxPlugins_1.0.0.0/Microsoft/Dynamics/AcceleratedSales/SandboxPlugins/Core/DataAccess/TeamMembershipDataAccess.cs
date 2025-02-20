// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.TeamMembershipDataAccess
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model;
using Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.Model.Interface;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public class TeamMembershipDataAccess : ITeamMembershipDataAccess
  {
    private const string TeamMembership = "teammembership";
    private const string TeamId = "teamid";
    private const string SystemUserId = "systemuserid";
    private readonly string[] teamMembershipAttributes = new string[1]
    {
      "teamid"
    };
    private readonly IAcceleratedSalesLogger logger;
    private readonly IDataStore dataStore;
    private List<Guid> teamIdsUserIsPartOf = (List<Guid>) null;

    public TeamMembershipDataAccess(IAcceleratedSalesLogger logger, IDataStore dataStore)
    {
      this.dataStore = dataStore;
      this.logger = logger;
    }

    public bool IsEqualUserOrTeam(Guid id)
    {
      if (!(id == this.dataStore.RetrieveUserId()))
        return this.IsInvokingUserATeamMemberOf(id);
      this.logger.LogWarning(string.Format("TeamMembershipDataAccess.IsEqualUserOrTeam.{0}.IsInvokingUser: ", (object) id) + true.ToString(), callerName: nameof (IsEqualUserOrTeam), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\TeamMembershipDataAccess.cs");
      return true;
    }

    private bool IsInvokingUserATeamMemberOf(Guid teamId)
    {
      bool flag = this.RetrieveTeamIdsInvokingUserIsPartOfMemo().Contains(teamId);
      this.logger.LogWarning(string.Format("TeamMembershipDataAccess.IsInvokingUserATeamMember.{0}: ", (object) teamId) + flag.ToString(), callerName: nameof (IsInvokingUserATeamMemberOf), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\TeamMembershipDataAccess.cs");
      return flag;
    }

    private List<Guid> RetrieveTeamIdsInvokingUserIsPartOfMemo()
    {
      if (this.teamIdsUserIsPartOf == null)
        this.teamIdsUserIsPartOf = this.RetrieveTeamIdsInvokingUserIsPartOf();
      return this.teamIdsUserIsPartOf;
    }

    private List<Guid> RetrieveTeamIdsInvokingUserIsPartOf()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<Guid> teamsUserIsPartOf = new List<Guid>();
      try
      {
        EntityCollection entityCollection = this.dataStore.RetrieveMultiple(this.GetQueryExpression());
        if (entityCollection?.Entities != null)
          entityCollection.Entities.ToList<Entity>().ForEach((Action<Entity>) (teamMembership =>
          {
            Guid guid;
            if (!teamMembership.TryGetAttributeValue<Guid>("teamid", ref guid))
              return;
            teamsUserIsPartOf.Add(guid);
          }));
      }
      catch (Exception ex)
      {
        this.logger.LogError("TeamMembershipDataAccess.RetrieveTeamIdsInvokingUserIsPartOf.Exception", ex, callerName: nameof (RetrieveTeamIdsInvokingUserIsPartOf), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\TeamMembershipDataAccess.cs");
      }
      finally
      {
        stopwatch.Stop();
        this.logger.LogWarning("TeamMembershipDataAccess.RetrieveTeamIdsInvokingUserIsPartOf.Duration: " + stopwatch.ElapsedMilliseconds.ToString(), callerName: nameof (RetrieveTeamIdsInvokingUserIsPartOf), callerFile: "C:\\__w\\1\\s\\solutions\\AcceleratedSales\\AcceleratedSalesSandboxPlugins\\Core\\DataAccess\\TeamMembershipDataAccess.cs");
      }
      return teamsUserIsPartOf;
    }

    private QueryExpression GetQueryExpression()
    {
      FilterExpression filterExpression = new FilterExpression();
      filterExpression.AddCondition(new ConditionExpression("systemuserid", ConditionOperator.EqualUserId));
      return new QueryExpression()
      {
        EntityName = "teammembership",
        ColumnSet = new ColumnSet(this.teamMembershipAttributes),
        Criteria = filterExpression
      };
    }
  }
}
