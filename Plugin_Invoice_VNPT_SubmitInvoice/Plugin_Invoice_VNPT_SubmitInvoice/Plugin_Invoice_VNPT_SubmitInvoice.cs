using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk;
using Plugin_Invoice_VNPT_SubmitInvoice.Helpers;
using Plugin_Invoice_VNPT_SubmitInvoice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Management;
using System.Xml;
using System.Xml.Serialization;

namespace Plugin_Invoice_VNPT_SubmitInvoice
{
    public class Plugin_Invoice_VNPT_SubmitInvoice : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        ITracingService tracingService;

        Entity Target = null;
        Entity enInvoice = null;
        Entity enAccount = null; // Chu dau tu
        Entity enCustomer = null; // Nguoi mua hang

        SoapEnvelope soapEnvelope = new SoapEnvelope();
        SoapBody soapBody = new SoapBody();
        ImportInvByPatternRequest importInvByPatternRequest = new ImportInvByPatternRequest();

        string invoicecode = string.Empty;
        string email = string.Empty;
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
                if (this.context.Depth > 3) return;
                this.Target = (Entity)context.InputParameters["Target"];
                this.enInvoice = service.Retrieve(Target.LogicalName, Target.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                if (((OptionSetValue)enInvoice["statuscode"]).Value != 100000003) return; // Approved = 100000003
                enAccount = GetAccount();
                enCustomer = GetCustomer();
                if (enAccount == null) return;
                if (!enAccount.Contains("bsd_webpublishservice") || !enAccount.Contains("bsd_adminaccount") || !enAccount.Contains("bsd_adminpassword")
                    || !enAccount.Contains("bsd_webserviceaccount") || !enAccount.Contains("bsd_webservicepassword")) throw new InvalidPluginExecutionException("The VNPT configuration is missing information.");

                importInvByPatternRequest.Account = enAccount["bsd_adminaccount"].ToString();
                importInvByPatternRequest.ACpass = enAccount["bsd_adminpassword"].ToString();
                importInvByPatternRequest.username = enAccount["bsd_webserviceaccount"].ToString();
                importInvByPatternRequest.password = enAccount["bsd_webservicepassword"].ToString();
                importInvByPatternRequest.pattern = this.enInvoice.Contains("bsd_formno") ? this.enInvoice["bsd_formno"].ToString() : null;
                importInvByPatternRequest.serial = this.enInvoice.Contains("bsd_serialno") ? this.enInvoice["bsd_serialno"].ToString() : null;
                importInvByPatternRequest.convert = 0;

                importInvByPatternRequest.xmlInvData = CData();

                soapBody.ImportInvByPattern = importInvByPatternRequest;
                soapEnvelope.Body = soapBody;

                var ns = new XmlSerializerNamespaces();
                ns.Add("", "http://tempuri.org/");
                ns.Add("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                string dataxml = XmlFormatHelper.FormatXml<SoapEnvelope>(soapEnvelope, ns);
                tracingService.Trace("xmldata: " + dataxml);
                
                ApiHelper.PostVNPT(enAccount["bsd_webpublishservice"].ToString(), dataxml,this.tracingService);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
        private Entity GetAccount() // Chu dau tu
        {
            Entity enProject = service.Retrieve(((EntityReference)enInvoice["bsd_project"]).LogicalName, ((EntityReference)enInvoice["bsd_project"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_investor", "bsd_projectcode", "bsd_emailinvoice", "bsd_invoicecode"));
            this.invoicecode = enProject.Contains("bsd_invoicecode") ? enProject["bsd_invoicecode"].ToString() : null;
            this.email = enProject.Contains("bsd_emailinvoice") ? enProject["bsd_emailinvoice"].ToString() : null;
            if (!enProject.Contains("bsd_investor")) return null;
            Entity enAccount = service.Retrieve(((EntityReference)enProject["bsd_investor"]).LogicalName, 
                ((EntityReference)enProject["bsd_investor"]).Id,
                new Microsoft.Xrm.Sdk.Query.ColumnSet("bsd_webpublishservice", "bsd_adminaccount", "bsd_adminpassword", 
                "bsd_webserviceaccount", "bsd_webservicepassword", "bsd_name"));
            return enAccount;
        }
        private Entity GetCustomer()
        {
            if (!enInvoice.Contains("bsd_purchaser")) return null;
            string[] attibutes = ((EntityReference)enInvoice["bsd_purchaser"]).LogicalName == "contact" ? 
                new string[] { "bsd_permanentaddress1", "bsd_fullname", "bsd_identitycardnumber", "bsd_passport" } :
                new string[] { "bsd_address", "bsd_name", "bsd_registrationcode", "emailaddress1" };
            //string attibuteID = ((EntityReference)enInvoice["bsd_purchaser"]).LogicalName == "contact" ? "bsd_identitycardnumber" : "bsd_address";
            Entity enCustomer = service.Retrieve(((EntityReference)enInvoice["bsd_purchaser"]).LogicalName, ((EntityReference)enInvoice["bsd_purchaser"]).Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(attibutes));
            return enCustomer;
        }
        private string CData()
        {
            Invoices inv = new Invoices
            {
                Inv = new List<InvoiceWrapper>()
            };
            InvoiceWrapper invWrapper = new InvoiceWrapper();
            Invoice invoice = new Invoice();
            Products products = new Products
            {
                Product = new List<Product>()
            };

            invWrapper.key = this.enInvoice.Contains("bsd_invoicecode")
                ? this.enInvoice["bsd_invoicecode"].ToString()
                : null;
            /// Map các trường dữ liệu khác của hóa đơn vào đối tượng invoice tại đây
            if (this.enInvoice.Contains("bsd_purchaser") && ((EntityReference)this.enInvoice["bsd_purchaser"]).LogicalName == "contact")
            {
                invoice.Buyer = this.enCustomer.Contains("bsd_fullname") ? this.enCustomer["bsd_fullname"].ToString() : null;
                invoice.CusAddress = this.enCustomer.Contains("bsd_permanentaddress1") ? this.enCustomer["bsd_permanentaddress1"].ToString() : null;
                invoice.SHChieu = this.enCustomer.Contains("bsd_passport") ? this.enCustomer["bsd_passport"].ToString() : null;
                invoice.CCCDan = this.enCustomer.Contains("bsd_identitycardnumber") ? this.enCustomer["bsd_identitycardnumber"].ToString() : null;
                invoice.EmailDeliver = this.email;
            }
            else if (this.enInvoice.Contains("bsd_purchaser") && ((EntityReference)this.enInvoice["bsd_purchaser"]).LogicalName == "account")
            {
                invoice.CusName = this.enCustomer.Contains("bsd_name") ? this.enCustomer["bsd_name"].ToString() : null;
                invoice.CusAddress = this.enCustomer.Contains("bsd_address") ? this.enCustomer["bsd_address"].ToString() : null;
                invoice.CusTaxCode = this.enCustomer.Contains("bsd_registrationcode") ? this.enCustomer["bsd_registrationcode"].ToString() : null;
                invoice.EmailDeliver = this.enCustomer.Contains("emailaddress1") ? this.enCustomer["emailaddress1"].ToString() : null;
            }

            invoice.ArisingDate = this.enInvoice.Contains("bsd_issueddate") ? ((DateTime)this.enInvoice["bsd_issueddate"]).ToString("dd/MM/yyyy") : null;
            invoice.PaymentMethod = this.enInvoice.Contains("bsd_paymentmethod") ? this.enInvoice.FormattedValues["bsd_paymentmethod"] : null;
            invoice.CusCode = this.invoicecode + ((EntityReference)this.enInvoice["bsd_units"]).Name;
            invoice.PaymentStatus = "0";
            invoice.CurrencyUnit = "VND";
            invoice.ExchangeRate = "1";

            var invoiceCode = this.enInvoice.Contains("bsd_documentno") ? this.enInvoice["bsd_documentno"].ToString() : null;
            invoice.Extra2 = invoiceCode; //!string.IsNullOrEmpty(invoiceCode) && invoiceCode.Length >= 6 ? invoiceCode.Substring(invoiceCode.Length - 6) : invoiceCode;

            decimal totalNoVat = 0;
            decimal bsd_invoiceamount = this.enInvoice.Contains("bsd_invoiceamountb4vat") ? ((Money)this.enInvoice["bsd_invoiceamountb4vat"]).Value : 0;
            decimal bsd_vatamount = this.enInvoice.Contains("bsd_vatamount") ? ((Money)this.enInvoice["bsd_vatamount"]).Value : 0;

            Product product1 = new Product()
            {
                ProdName = this.enInvoice.Contains("bsd_name") ? this.enInvoice["bsd_name"].ToString() : null,
                VATRate = this.enInvoice.Contains("bsd_taxcodevalue") && Math.Round((decimal)this.enInvoice["bsd_taxcodevalue"],0) == 0 ? "-1" : "10",
                VATAmount = bsd_vatamount != 0 ? Math.Round(bsd_vatamount, 0).ToString() : null,
                Total = bsd_invoiceamount != 0 ? Math.Round(bsd_invoiceamount, 0).ToString() : null,
            };
            products.Product.Add(product1);
            if (enInvoice.Contains("bsd_namelandvalue"))
            {
                totalNoVat = this.enInvoice.Contains("bsd_handoveramount") ? ((Money)this.enInvoice["bsd_handoveramount"]).Value : 0;
                invoice.GrossValue = totalNoVat != 0 ? Math.Round(totalNoVat, 0).ToString() : null;
                Product product2 = new Product()
                {
                    ProdName = this.enInvoice.Contains("bsd_namelandvalue") ? this.enInvoice["bsd_namelandvalue"].ToString() : null,
                    Total = this.enInvoice.Contains("bsd_handoveramount") ? Math.Round(((Money)this.enInvoice["bsd_handoveramount"]).Value, 0).ToString() : null,
                    VATRate = "-1"
                };
                products.Product.Add(product2);
            }
            invoice.Products = products;

            invoice.GrossValue10 = bsd_invoiceamount != 0 ? Math.Round(bsd_invoiceamount, 0).ToString() : null;
            invoice.VatAmount10 = bsd_vatamount != 0 ? Math.Round(bsd_vatamount, 0).ToString() : null;

            decimal amount = bsd_invoiceamount + bsd_vatamount + totalNoVat;
            invoice.Total = Math.Round(bsd_invoiceamount + totalNoVat, 0).ToString();
            invoice.VATAmount = Math.Round(bsd_vatamount, 0).ToString();
            invoice.Amount = Math.Round(amount, 0).ToString();
            invoice.AmountInWords = MoneyToTextHelper.TienBangChu(invoice.Amount,false);
            
            invWrapper.Invoice = invoice;
            inv.Inv.Add(invWrapper);

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            string dataxml = XmlFormatHelper.FormatXml<Invoices>(inv, ns);
            return dataxml;
        }
    }
}
