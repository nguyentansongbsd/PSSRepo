using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PssFunctionApp.Entities;
using PssFunctionApp.Reponsitory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PssFunctionApp.Reponsitory
{
    public class UnitReponsitory : IUnitReponsitory
    {
        private readonly ServiceClient _serviceClient;
        public UnitReponsitory(ServiceClient serviceClient) 
        {
            _serviceClient = serviceClient;
        }
        public async Task<Unit> getUnitById(string unitId)
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
            <fetch>
              <entity name=""account"">
                <attribute name=""name"" />
                <filter>
                  <condition attribute=""accountid"" operator=""eq"" value=""{unitId}"" />
                </filter>
              </entity>
            </fetch>";
            EntityCollection rs = this._serviceClient.RetrieveMultiple(new FetchExpression(fetchXml));
            if (rs.Entities.Count <= 0) return null;
            Entity enUnit = rs.Entities[0];
            Unit unit = new Unit();
            unit.name = enUnit.Contains("name") ? enUnit["name"].ToString() : null;
            return unit;
        }
    }
}
