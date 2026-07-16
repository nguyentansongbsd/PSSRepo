using Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Helpers;
using Action_Invoice_VNPT_UpdateInvoiceNoBySchedule.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

namespace Action_Invoice_VNPT_UpdateInvoiceNoBySchedule
{
    public class Action_Invoice_VNPT_UpdateInvoiceNoBySchedule : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        ITracingService tracingService;

        EntityReference Target;
        Entity enInvoice;
        Entity enAccount;

        public void Execute(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            this.serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.service = serviceFactory.CreateOrganizationService(context.UserId);
            this.tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Init();
        }
        private void Init()
        {
            try
            {
                this.Target = (EntityReference)context.InputParameters["Target"];
                this.enInvoice = service.Retrieve(Target.LogicalName, Target.Id, new ColumnSet("bsd_project", "bsd_invoicecode",
                    "bsd_serialno", "bsd_formno"));
                this.enAccount = GetAccount();
                if (enAccount == null) return;
                if (!enAccount.Contains("bsd_webpublishservice") || !enAccount.Contains("bsd_adminaccount") || !enAccount.Contains("bsd_adminpassword")
                    || !enAccount.Contains("bsd_webserviceaccount") || !enAccount.Contains("bsd_webservicepassword")) throw new InvalidPluginExecutionException("Cau hinh thieu thong tin vnpt.");

                var request = new SoapEnvelope
                {
                    Body = new SoapBody
                    {
                        GetMCCQThueByFkeysNoXMLSign = new GetMCCQThueByFkeysNoXMLSignRequest
                        {
                            Account = enAccount["bsd_adminaccount"].ToString(),
                            ACpass = enAccount["bsd_adminpassword"].ToString(),
                            Username = enAccount["bsd_webserviceaccount"].ToString(),
                            Password = enAccount["bsd_webservicepassword"].ToString(),
                            Pattern = this.enInvoice.Contains("bsd_formno") ? this.enInvoice["bsd_formno"].ToString() : null,
                            Fkeys = this.enInvoice.Contains("bsd_invoicecode") ? this.enInvoice["bsd_invoicecode"].ToString() : null
                        }
                    }
                };
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "http://tempuri.org/");
                ns.Add("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                string dataxml = XmlFormatHelper.FormatXml<SoapEnvelope>(request, ns);
                tracingService.Trace("xml: " + dataxml);

                SoapEnvelopeResponse soapEnvelopeResponse = ApiHelper.PostVNPT(enAccount["bsd_webpublishservice"].ToString(), dataxml, this.tracingService);

                /// Lấy kết quả trả về, giải mã base64 và deserialize thành object DSHDon
                string dataxmlresponseBase64 = soapEnvelopeResponse.Body.GetMCCQThueByFkeysNoXMLSignResponse.Result;
                if (string.IsNullOrWhiteSpace(dataxmlresponseBase64))
                {
                    tracingService.Trace("VNPT trả về Result = null");
                    return;
                }
                if (dataxmlresponseBase64.StartsWith("ERR:", StringComparison.OrdinalIgnoreCase))
                {
                    tracingService.Trace("VNPT trả về Result = ERR");
                    return;
                }
                string dataxmlresponse = ConvertBase64Helper.ConvertBase64ToString(dataxmlresponseBase64);

                /// update so hoa don vao invoice
                var serializer = new XmlSerializer(typeof(DSHDon));
                DSHDon dSHDon;
                using (var reader = new StringReader(dataxmlresponse))
                {
                    dSHDon = (DSHDon)serializer.Deserialize(reader);
                }
                HDon hdon = dSHDon.HDon.FirstOrDefault();
                if (hdon.TThai == 2)
                {
                    Entity enUpInvoie = new Entity(Target.LogicalName, Target.Id);
                    enUpInvoie["bsd_invoiceno"] = hdon.SHDon.ToString("D8");
                    enUpInvoie["statuscode"] = new OptionSetValue(100000002); // Released
                    this.service.Update(enUpInvoie);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
        private Entity GetAccount() // Chu dau tu
        {
            Entity enProject = service.Retrieve(((EntityReference)enInvoice["bsd_project"]).LogicalName, ((EntityReference)enInvoice["bsd_project"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_investor", "bsd_projectcode"));
            if (!enProject.Contains("bsd_investor")) return null;
            Entity enAccount = service.Retrieve(((EntityReference)enProject["bsd_investor"]).LogicalName,
                ((EntityReference)enProject["bsd_investor"]).Id,
                new ColumnSet("bsd_webpublishservice", "bsd_adminaccount", "bsd_adminpassword",
                "bsd_webserviceaccount", "bsd_webservicepassword", "bsd_name"));
            return enAccount;
        }
    }
}
