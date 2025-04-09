using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Action_Pricelist_btncopyPL
{
    public class Action_Pricelist_btncopyPL : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            EntityReference target = (EntityReference)context.InputParameters["Target"];
            Entity pricemaster = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
            string namemaster = "";
            int countdetail = 0;
            if (pricemaster.Contains("bsd_phaselaunchid"))
            {
                var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_phaseslaunch'>
                                <filter>
                                  <condition attribute='statuscode' operator='neq' value='100000000'/>
                                  <condition attribute='bsd_phaseslaunchid' operator='eq' value='{((EntityReference)pricemaster["bsd_phaselaunchid"]).Id}'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection listphase = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (listphase.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException("Phase launch is not launch. Cannot copy price list.");
                }
                else
                {
                    var fetchXmlitem = $@"
                                    <fetch>
                                      <entity name='productpricelevel'>
                                        <filter>
                                          <condition attribute='pricelevelid' operator='eq' value='{pricemaster.Id}'/>
                                        </filter>
                                        <link-entity name='product' from='productid' to='productid'>
                                          <filter>
                                            <condition attribute='statuscode' operator='in'>
                                              <value>1</value>
                                              <value>100000000</value>
                                              <value>100000004</value>
                                            </condition>
                                          </filter>
                                        </link-entity>
                                      </entity>
                                    </fetch>";
                    EntityCollection listitem = service.RetrieveMultiple(new FetchExpression(fetchXmlitem));
                    if (listitem.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("Units are not enough condition for copying. Please check again.\nKhông có sản phẩm thỏa điều kiện sao chép. Vui lòng kiểm tra lại thông tin.");
                    }
                    else
                    {
                        var fetchXmlmaster = $@"
                                    <fetch>
                                      <entity name='pricelevel'>
                                        <filter>
                                          <condition attribute='pricelevelid' operator='eq' value='{pricemaster.Id}'/>
                                        </filter>
                                      </entity>
                                    </fetch>";
                        EntityCollection listmaster = service.RetrieveMultiple(new FetchExpression(fetchXmlmaster));
                        if (listmaster.Entities.Count > 0)
                        {
                            Guid idmaster = new Guid();
                            foreach (Entity master in listmaster.Entities)
                            {
                                Entity copymaster = new Entity(master.LogicalName);
                                master.Attributes.Remove("pricelevelid");
                                namemaster = "Copy-" + (string)master["name"];
                                master["name"] = "Copy-" + (string)master["name"];
                                master["bsd_approved"] = false;
                                master["bsd_pricelistcopy"] = master.ToEntityReference();
                                master["bsd_description"] = namemaster + " from price list name " + (string)pricemaster["name"] + DateTime.Now.ToString();
                                master.Id = Guid.NewGuid();
                                copymaster = master;
                                idmaster = service.Create(copymaster);
                            }
                            foreach (Entity detail in listitem.Entities)
                            {
                                Entity enMaster = service.Retrieve("pricelevel", idmaster, new ColumnSet(true));
                                Entity copydetail = new Entity(detail.LogicalName);
                                detail.Attributes.Remove("productpricelevelid");
                                detail["pricelevelid"] = enMaster.ToEntityReference();

                                detail.Id = Guid.NewGuid();
                                copydetail = detail;
                                service.Create(copydetail);
                                countdetail++;
                            }
                        }

                    }
                }

            }
            else
            {
                var fetchXmlitem = $@"
                                    <fetch>
                                      <entity name='productpricelevel'>
                                        <filter>
                                          <condition attribute='pricelevelid' operator='eq' value='{pricemaster.Id}'/>
                                        </filter>
                                        <link-entity name='product' from='productid' to='productid'>
                                          <filter>
                                            <condition attribute='statuscode' operator='in'>
                                              <value>1</value>
                                              <value>100000000</value>
                                              <value>100000004</value>
                                            </condition>
                                          </filter>
                                        </link-entity>
                                      </entity>
                                    </fetch>";
                EntityCollection listitem = service.RetrieveMultiple(new FetchExpression(fetchXmlitem));
                if (listitem.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException("Units are not enough condition for copying. Please check again.\nKhông có sản phẩm thỏa điều kiện sao chép. Vui lòng kiểm tra lại thông tin.");
                }
                else
                {
                    var fetchXmlmaster = $@"
                                    <fetch>
                                      <entity name='pricelevel'>
                                        <filter>
                                          <condition attribute='pricelevelid' operator='eq' value='{pricemaster.Id}'/>
                                        </filter>
                                      </entity>
                                    </fetch>";
                    EntityCollection listmaster = service.RetrieveMultiple(new FetchExpression(fetchXmlmaster));
                    if (listmaster.Entities.Count > 0)
                    {
                        Guid idmaster = new Guid();
                        foreach (Entity master in listmaster.Entities)
                        {
                            Entity copymaster = new Entity(master.LogicalName);
                            master.Attributes.Remove("pricelevelid");
                            namemaster = "Copy-" + (string)master["name"];
                            master["name"] = "Copy-" + (string)master["name"];
                            master["bsd_approved"] = false;
                            master["bsd_pricelistcopy"] = master.ToEntityReference();
                            master["bsd_description"] = namemaster + " from price list name " + (string)pricemaster["name"] + DateTime.Now.ToString();
                            master.Id = Guid.NewGuid();
                            copymaster = master;
                            idmaster = service.Create(copymaster);
                        }
                        
                        foreach (Entity detail in listitem.Entities)
                        {
                            Entity enMaster = service.Retrieve("pricelevel", idmaster, new ColumnSet(true));
                           
                            Entity copydetail = new Entity(detail.LogicalName);
                            detail.Attributes.Remove("productpricelevelid");
                            detail["pricelevelid"] = enMaster.ToEntityReference();
                            detail.Id = Guid.NewGuid();
                            copydetail = detail;
                            service.Create(copydetail);
                            countdetail++;
                        }
                    }

                }

            }
            context.OutputParameters["Data"] = "Result have " + countdetail.ToString() + " detail record."+"Title: " + namemaster;

        }
    }
}
