using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Create_PaymentNotices
{
    public class Plugin_Create_PaymentNotices : IPlugin
    {
        IPluginExecutionContext context = null;
        IOrganizationServiceFactory factory = null;
        IOrganizationService service = null;
        ITracingService tracingService = null;
        Entity en = new Entity();
        public void Execute(IServiceProvider serviceProvider)
        {

            context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = (IOrganizationService)factory.CreateOrganizationService(context.UserId);
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Entity entity = (Entity)context.InputParameters["Target"];
            Guid recordId = entity.Id;
            Entity enCreated = service.Retrieve(entity.LogicalName, entity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Entity enUpdate=new Entity(enCreated.LogicalName,enCreated.Id);

            //lấy Instalment detail 
            EntityReference enInsDetailRef =(EntityReference)enCreated["bsd_paymentschemedetail"];
            Entity enInsDetail = service.Retrieve(enInsDetailRef.LogicalName,enInsDetailRef.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            // lấy Option Entry
            EntityReference enOPRef = (EntityReference)enCreated["bsd_optionentry"];
            Entity EnOP = service.Retrieve(enOPRef.LogicalName, enOPRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            #region Amount in EDA (bsd_amountofthisphase) mapping từ Instalment của đợt làm payment notices
            enUpdate["bsd_amountofthisphase"] = enInsDetail["bsd_amountofthisphase"];
            #endregion

            #region bsd_totaladvancepayment (Số tiền thanh toán trước)-->Field tính toán (Mapping từ option entry)-->Field tính toán
            enUpdate["bsd_totaladvancepayment"] = EnOP["bsd_totaladvancepayment"];
            #endregion

            #region (Tổng số tiền thanh toán trước)-->bsd_totalprepaymentamount=bsd_totaladvancepayment+ bsd_amountwaspaid (của đợt paymnent notices)-->Hiển thị số âm (Field tính toán)
            enUpdate["bsd_totalprepaymentamount"] = new Money(((Money)EnOP["bsd_totaladvancepayment"]).Value+ ((Money)enInsDetail["bsd_amountwaspaid"]).Value);
            #endregion

            #region Shortfall in previous Installment (Số tiền chưa thanh toán các đợt trước) (bsd_Shoftfall Installment= Sum(bsd_balance) Tổng số tiền chưa thanh toán của các đợt trước đợt làm payment notice -->Field tính toán
            decimal sum_bsd_balance = 0;
            int orderNumberInsDetail = (int)enInsDetail["bsd_ordernumber"];
            //query lấy ra các đợt trước đợt PaymentNotices
            var query = new QueryExpression("bsd_paymentschemedetail");
            query.ColumnSet.AllColumns = true;
            query.Criteria.AddCondition("bsd_optionentry", ConditionOperator.Equal, EnOP.Id.ToString());
            query.Criteria.AddCondition("bsd_ordernumber", ConditionOperator.LessThan, orderNumberInsDetail);
            var insDetailList = service.RetrieveMultiple(query);
            if(insDetailList.Entities.Count > 0)
            {
                foreach(var item in insDetailList.Entities)
                {
                    sum_bsd_balance += ((Money)item["bsd_balance"]).Value;
                }
            }
            enUpdate["bsd_shortfallinpreviousinstallment"] = new Money(sum_bsd_balance);
            #endregion

            #region Amount to transfer (Số tiền phải chuyển)=bsd_totaladvancepayment+Totalprepaymentamount+bsd_Shoftfall Installment -->Field tính toán
            enUpdate["bsd_amounttotransfer"] = new Money(
                ((Money)enUpdate["bsd_amountofthisphase"]).Value-
                ((Money)enUpdate["bsd_totalprepaymentamount"]).Value +
                ((Money)enUpdate["bsd_shortfallinpreviousinstallment"]).Value
                );
            #endregion
            service.Update(enUpdate);
        }
    }
}
