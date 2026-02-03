using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using System.Text.RegularExpressions;

namespace Plugin_ReadMoney
{
    public class Plugin_ReadMoney : IPlugin
    {
        IOrganizationService service = null;
        IOrganizationServiceFactory factory = null;

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            int times = 1;
            //if (context.Depth > 1)
            //    return;
            if (context.MessageName == "Update" || context.MessageName == "Create")
            {
                Entity target = (Entity)context.InputParameters["Target"];
                string smoney = "";
                Int64 dmoney = 0;
                string vn = "";
                string en = "";
                //Entity target = new Entity(target.LogicalName);
                //target.Id = target.Id;

                #region -- Quotation Reservation
                if (target.LogicalName == "quote")
                {
                    // Listed Price
                    if (target.Contains("bsd_detailamount") && ((Money)target["bsd_detailamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_detailamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_detailamount"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Listed Price");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_txdetailamountvn"] = vn;
                        target["bsd_txdetailamounten"] = en;
                    }

                    // Total Amount
                    if (target.Contains("totalamount") && ((Money)target["totalamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["totalamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["totalamount"]).Value);

                        if (dmoney > 0)
                        {
                            vn = TienBangChu(smoney);
                            en = read(dmoney);
                            target["bsd_readmoneyvn"] = vn;
                            target["bsd_readmoneyen"] = en;

                        }
                        //else
                        //{
                        //    throw new InvalidPluginExecutionException("Total Amount");
                        //}


                    }

                    // Depsoit Fee Received
                    if (target.Contains("bsd_depositfeereceived") && ((Money)target["bsd_depositfeereceived"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_depositfeereceived"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_depositfeereceived"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Depsoit Fee Received");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_txdepositvn"] = vn;
                        target["bsd_txdepositen"] = en;

                    }

                    // Depsoit Fee
                    if (target.Contains("bsd_depositfee") && ((Money)target["bsd_depositfee"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_depositfee"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_depositfee"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Depsoit Fee");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_deposittextvn"] = vn;
                        target["bsd_deposittexten"] = en;

                    }

                    // Total VAT Tax
                    if (target.Contains("totaltax") && ((Money)target["totaltax"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["totaltax"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["totaltax"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Tax");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_txtotaltaxvn"] = vn;
                        target["bsd_txtotaltaxen"] = en;
                    }

                    // Maintenance Fee
                    if (target.Contains("bsd_freightamount") && ((Money)target["bsd_freightamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_freightamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_freightamount"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Maintenance Fee");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_txmainternancevn"] = vn;
                        target["bsd_txmainternanceen"] = en;
                    }

                    // 30.10.2017 - Han - Net Selling Price
                    if (target.Contains("bsd_totalamountlessfreight") && ((Money)target["bsd_totalamountlessfreight"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_totalamountlessfreight"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_totalamountlessfreight"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Net Selling Price");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_readnetvn"] = vn;
                        target["bsd_readneten"] = en;
                    }

                    // 01.03.2018 - Han - Update Management Fee into Installment
                    if (target.Contains("bsd_managementfee") && ((Money)target["bsd_managementfee"]) != null)
                    {
                        decimal manafee = ((Money)target["bsd_managementfee"]).Value;
                        EntityCollection GetInstall = get_InstallmentManaFee(service, target.Id, times);

                        if (GetInstall.Entities.Count > 0)
                        {
                            Entity InstEn = GetInstall.Entities[0];
                            InstEn["bsd_managementamount"] = new Money(manafee);
                            service.Update(InstEn);
                        }
                        else
                        {
                            times = 2;
                            EntityCollection GetInstallEHD = get_InstallmentManaFee(service, target.Id, times);
                            if (GetInstallEHD.Entities.Count > 0)
                            {
                                Entity InstEn = GetInstallEHD.Entities[0];
                                InstEn["bsd_managementamount"] = new Money(manafee);
                                service.Update(InstEn);
                            }
                        }

                        //03.05.2018 - Han - Bo sung phan doc tien
                        smoney = (Convert.ToInt64(((Money)target["bsd_managementfee"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_managementfee"]).Value);

                        if (dmoney < 0)
                            throw new InvalidPluginExecutionException("Mana Fee");

                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_managementfeetextvn"] = vn;
                        target["bsd_managementfeetexten"] = en;
                    }

                }
                #endregion

                #region -- Sub Sale
                else if (target.LogicalName == "bsd_assign" && target.Contains("bsd_assignfeeamount") && ((Money)target["bsd_assignfeeamount"]) != null)
                {
                    smoney = (Convert.ToInt64(((Money)target["bsd_assignfeeamount"]).Value)).ToString();
                    dmoney = Convert.ToInt64(((Money)target["bsd_assignfeeamount"]).Value);
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_readmoneyvn"] = vn;
                    target["bsd_readmoneyen"] = en;
                }
                #endregion

                #region -- Queues
                else if (target.LogicalName == "opportunity" && target.Contains("bsd_queuingfee") && ((Money)target["bsd_queuingfee"]) != null)
                {
                    smoney = (Convert.ToInt64(((Money)target["bsd_queuingfee"]).Value)).ToString();
                    dmoney = Convert.ToInt64(((Money)target["bsd_queuingfee"]).Value);
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_readmoneyvn"] = vn;
                    target["bsd_readmoneyen"] = en;
                }

                else if (target.LogicalName == "opportunity" && target.Contains("bsd_queuingfee") && ((Money)target["bsd_queuingfee"]) == null)
                {
                    smoney = "0";
                    dmoney = 0;
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_readmoneyvn"] = vn;
                    target["bsd_readmoneyen"] = en;
                }
                #endregion

                #region -- Payment
                else if (target.LogicalName == "bsd_payment" && (target.Contains("bsd_amountpay") || target.Contains("bsd_totalamountpayablephase")) /*&& ((OptionSetValue)target["statuscode"]).Value == 100000000*/)
                {
                    //Entity pay = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_amountpay", "bsd_totalamountpayablephase" }));
                    if (target != null && target.Contains("bsd_amountpay"))
                    {
                        var bsd_amountpay = (Money)target["bsd_amountpay"];
                        if (bsd_amountpay != null)
                        {
                            smoney = Convert.ToInt64(bsd_amountpay.Value).ToString();
                            dmoney = Convert.ToInt64(bsd_amountpay.Value);
                            if (dmoney > 0)
                            {
                                vn = TienBangChu(smoney);
                                en = read(dmoney);
                                target["bsd_txamountpayvn"] = vn;
                                target["bsd_txamountpayen"] = en;
                            }
                        }
                    }

                    //if (target.Contains("bsd_amountpay"))
                    //{
                    //    smoney = (Convert.ToInt64(((Money)target["bsd_amountpay"]).Value)).ToString();
                    //    dmoney = Convert.ToInt64(((Money)target["bsd_amountpay"]).Value);
                    //    if(dmoney !=null && dmoney > 0)
                    //    {
                    //        vn = TienBangChu(smoney);
                    //        en = read(dmoney);
                    //        target["bsd_txamountpayvn"] = vn;
                    //        target["bsd_txamountpayen"] = en;
                    //    }
                    //}
                    if (target != null && target.Contains("bsd_totalamountpayablephase"))
                    {
                        var bsd_totalamountpay = (Money)target["bsd_totalamountpayablephase"];
                        if (bsd_totalamountpay != null)
                        {
                            smoney = (Convert.ToInt64(((Money)target["bsd_totalamountpayablephase"]).Value)).ToString();
                            dmoney = Convert.ToInt64(((Money)target["bsd_totalamountpayablephase"]).Value);
                            if (dmoney > 0)
                            {
                                vn = TienBangChu(smoney);
                                en = read(dmoney);
                                target["bsd_txtotalamountpayablephasevn"] = vn;
                                target["bsd_txtotalamountpayablephaseen"] = en;
                            }
                        }
                    }

                }
                #endregion

                #region -- Installment
                else if (target.LogicalName == "bsd_paymentschemedetail" && target.Contains("bsd_amountofthisphase"))
                {
                    smoney = (Convert.ToInt64(((Money)target["bsd_amountofthisphase"]).Value)).ToString();
                    dmoney = Convert.ToInt64(((Money)target["bsd_amountofthisphase"]).Value);
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_txamountofphasevn"] = vn;
                    target["bsd_txamountofphaseen"] = en;
                }
                #endregion

                #region -- Units
                else if (target.LogicalName == "product" && target.Contains("bsd_maintenancefeespercent"))
                {
                    if (target.Contains("bsd_maintenancefeespercent"))
                    {
                        smoney = (Convert.ToInt64(((Decimal)target["bsd_maintenancefeespercent"]))).ToString();
                        dmoney = Convert.ToInt64(((Decimal)target["bsd_maintenancefeespercent"]));
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_txmaintanencevn"] = vn;
                        target["bsd_txmaintanenceen"] = en;
                    }
                    if (context.MessageName == "Create")
                    {
                        if (target.Contains("bsd_blocknumber") && !target.Contains("bsd_landlot"))
                            target["bsd_landlot"] = target["bsd_blocknumber"];
                        if (target.Contains("bsd_floor") && !target.Contains("bsd_plotnumber"))
                            target["bsd_plotnumber"] = target["bsd_floor"];
                    }
                }
                #endregion

                #region -- Option Entry
                else if (target.LogicalName == "salesorder")
                {
                    if (target.Contains("bsd_daamount") && ((Money)target["bsd_daamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_daamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_daamount"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_daamountvn"] = vn;
                        target["bsd_daamounten"] = en;
                    }

                    // 27.12.2017 - Han - Net Selling Price
                    if (target.Contains("bsd_totalamountlessfreight") && ((Money)target["bsd_totalamountlessfreight"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_totalamountlessfreight"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_totalamountlessfreight"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_readnetvn"] = vn;
                        target["bsd_readneten"] = en;

                        //Entity OptionEn = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa" }));
                        //decimal tmpop = OptionEn.Contains("bsd_totalpaidincludecoa") ? ((Money)OptionEn["bsd_totalpaidincludecoa"]).Value : 0;

                        //smoney = (Convert.ToInt64(tmpop)).ToString();
                        //dmoney = Convert.ToInt64(tmpop);
                        //vn = TienBangChu(smoney);
                        //en = read(dmoney);
                        //target["bsd_amountpaidvn"] = vn;
                        //target["bsd_amountpaiden"] = en;
                    }
                    if (target.Contains("totaltax") && ((Money)target["totaltax"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["totaltax"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["totaltax"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_totaltaxvn"] = vn;
                        target["bsd_totaltaxen"] = en;

                        //Entity OptionEn = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa" }));
                        //decimal tmpop = OptionEn.Contains("bsd_totalpaidincludecoa") ? ((Money)OptionEn["bsd_totalpaidincludecoa"]).Value : 0;

                        //smoney = (Convert.ToInt64(tmpop)).ToString();
                        //dmoney = Convert.ToInt64(tmpop);
                        //vn = TienBangChu(smoney);
                        //en = read(dmoney);
                        //target["bsd_amountpaidvn"] = vn;
                        //target["bsd_amountpaiden"] = en;
                    }
                    Entity OE = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));
                    decimal total = 0;
                    decimal amountnet = OE.Contains("bsd_totalamountlessfreight") ? ((Money)OE["bsd_totalamountlessfreight"]).Value : 0;
                    decimal amounttax = OE.Contains("totaltax") ? ((Money)OE["totaltax"]).Value : 0;
                    total = amountnet + amounttax;
                    // throw new InvalidPluginExecutionException("total " + total );
                    smoney = (Convert.ToInt64(total)).ToString();
                    dmoney = Convert.ToInt64(total);
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_totalafternetvn"] = vn;
                    target["bsd_totalafterneten"] = en;
                    decimal totalins = 0;
                    var fetchXml = $@"
                            <fetch>
                              <entity name='bsd_paymentschemedetail'>
                                <attribute name='bsd_amountofthisphase' />
                                <filter>
                                  <condition attribute='statecode' operator='eq' value='0'/>
                                  <condition attribute='bsd_ordernumber' operator='le' value='2'/>
                                  <condition attribute='bsd_optionentry' operator='eq' value='{OE.Id}'/>
                                </filter>
                              </entity>
                            </fetch>";
                    EntityCollection list = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (list.Entities.Count > 0)
                    {
                        foreach (Entity item in list.Entities)
                        {
                            totalins += ((Money)item["bsd_amountofthisphase"]).Value;
                        }
                    }

                    smoney = (Convert.ToInt64(totalins)).ToString();
                    dmoney = Convert.ToInt64(totalins);
                    vn = TienBangChu(smoney);
                    en = read(dmoney);
                    target["bsd_totalsohovn"] = vn;
                    target["bsd_totalsohoen"] = en;
                    //Entity OptionEn = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa" }));
                    //decimal tmpop = OptionEn.Contains("bsd_totalpaidincludecoa") ? ((Money)OptionEn["bsd_totalpaidincludecoa"]).Value : 0;

                    //smoney = (Convert.ToInt64(tmpop)).ToString();
                    //dmoney = Convert.ToInt64(tmpop);
                    //vn = TienBangChu(smoney);
                    //en = read(dmoney);
                    //target["bsd_amountpaidvn"] = vn;
                    //target["bsd_amountpaiden"] = en;


                    // 30.01.2018 - Han - Total Amount Paid
                    // 17.04.2018 - Han - Ko su dung , dung field Confirmation Amount
                    //if (target.Contains("bsd_totalpaidincludecoa") && ((Money)target["bsd_totalpaidincludecoa"]) != null)
                    if (target.Contains("bsd_totalamountpaid") && ((Money)target["bsd_totalamountpaid"]) != null)
                    {
                        Entity OptionEn = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa" }));
                        decimal tmpop = OptionEn.Contains("bsd_totalpaidincludecoa") ? ((Money)OptionEn["bsd_totalpaidincludecoa"]).Value : 0;

                        smoney = (Convert.ToInt64(tmpop)).ToString();
                        dmoney = Convert.ToInt64(tmpop);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);

                        target["bsd_totalamountpaid"] = new Money(tmpop);
                        target["bsd_amountpaidvn"] = vn;
                        target["bsd_amountpaiden"] = en;
                    }

                    // 17.04.2018 - Han - FIN-HN req - Confirmation Amount
                    if (target.Contains("bsd_confirmationamount") && ((Money)target["bsd_confirmationamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_confirmationamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_confirmationamount"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);

                        target["bsd_amountpaidvn"] = vn;
                        target["bsd_amountpaiden"] = en;
                    }

                    // 01.02.2018 - Han - Update Management Fee
                    if (target.Contains("bsd_managementfee") && ((Money)target["bsd_managementfee"]) != null)
                    {
                        decimal manafee = ((Money)target["bsd_managementfee"]).Value;
                        EntityCollection GetInstall = get_InstallmentManaFee(service, target.Id, times);
                        if (GetInstall.Entities.Count > 0)
                        {
                            Entity InstEn = GetInstall.Entities[0];
                            InstEn["bsd_managementamount"] = new Money(manafee);
                            service.Update(InstEn);
                        }
                        else
                        {
                            times = 2;
                            EntityCollection GetInstallEHD = get_InstallmentManaFee(service, target.Id, times);
                            if (GetInstallEHD.Entities.Count > 0)
                            {
                                Entity InstEn = GetInstallEHD.Entities[0];
                                InstEn["bsd_managementamount"] = new Money(manafee);
                                service.Update(InstEn);
                            }
                        }

                        //03.05.2018 - Han - Bo sung phan doc tien
                        smoney = (Convert.ToInt64(((Money)target["bsd_managementfee"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_managementfee"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_managementfeetextvn"] = vn;
                        target["bsd_managementfeetexten"] = en;
                    }

                    // 13.03.2018 - Han - Total Amount
                    if (target.Contains("totalamount") && ((Money)target["totalamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["totalamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["totalamount"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_totalpricevn"] = vn;
                        target["bsd_totalpriceen"] = en;
                    }

                    // 03.05.2018 - Han - Maintenance Fee
                    if (target.Contains("bsd_freightamount") && ((Money)target["bsd_freightamount"]) != null)
                    {
                        smoney = (Convert.ToInt64(((Money)target["bsd_freightamount"]).Value)).ToString();
                        dmoney = Convert.ToInt64(((Money)target["bsd_freightamount"]).Value);
                        vn = TienBangChu(smoney);
                        en = read(dmoney);
                        target["bsd_maintenancefeetextvn"] = vn;
                        target["bsd_maintenancefeetexten"] = en;
                    }
                }
                #endregion

                // service.Update(target);
            }

        }

        public static string TienBangChu(string sSoTienIn)
        {
            string am = "";
            if (sSoTienIn.StartsWith("-"))
            {
                am = "Âm ";
                sSoTienIn = sSoTienIn.Remove(0, 1);
            }
            string sSoTien = sSoTienIn;
            if (sSoTien == "0")
                return "Không";
            Regex r = new Regex(@"^[0]*");
            sSoTien = r.Replace(sSoTien, "");
            if (sSoTien.Substring(0, 1) == "0")
                return "Không ";

            string[] DonVi = { "", "nghìn ", "triệu ", "tỷ ", "nghìn tỷ ", "triệu tỷ ", "tỷ tỷ " };
            string so = null;
            string chuoi = "";
            string temp = null;
            byte id = 0;

            while ((!sSoTien.Equals("")))
            {
                if (sSoTien.Length != 0)
                {
                    so = getNum(sSoTien);
                    //sSoTien = Left(sSoTien, sSoTien.Length - so.Length);
                    sSoTien = sSoTien.Substring(0, sSoTien.Length - so.Length);
                    temp = setNum(so);
                    so = temp;
                    if (!so.Equals(""))
                    {
                        temp = temp + DonVi[id];
                        chuoi = temp + chuoi;
                    }
                    id += 1;
                }
            }
            temp = chuoi.Substring(0, 1).ToUpper();

            return am + temp + chuoi.Substring(1, chuoi.Length - 2);

        }

        private static string getNum(string sSoTien)
        {
            string so = null;

            if (sSoTien.Length >= 3)
            {
                //so = VB.Right(sSoTien.Substring(sSoTien.Length-4, 3);
                so = sSoTien.Substring(sSoTien.Length - 3, 3);
            }
            else
            {
                so = sSoTien.Substring(0, sSoTien.Length);
            }
            return so;
        }

        private static string setNum(string sSoTien)
        {
            string chuoi = "";
            bool flag0 = false;
            bool flag1 = false;
            string temp = null;

            temp = sSoTien;
            string[] kyso = { "không ", "một ", "hai ", "ba ", "bốn ", "năm ", "sáu ", "bảy ", "tám ", "chín " };
            //Xet hang tram
            if (sSoTien.Length == 3)
            {
                if (!(sSoTien.Substring(0, 1) == "0" && sSoTien.Substring(1, 1) == "0" && sSoTien.Substring(2, 1) == "0"))
                {
                    chuoi = kyso[Convert.ToInt16(sSoTien.Substring(0, 1))] + "trăm ";
                }
                sSoTien = sSoTien.Substring(1, 2);
            }
            //Xet hang chuc
            if (sSoTien.Length == 2)
            {
                // if (VB.Left(sSoTien, 1) == 0)
                if (sSoTien.Substring(0, 1) == "0")
                {
                    if (sSoTien.Substring(1, 1) != "0")
                    {
                        chuoi = chuoi + "linh ";
                    }
                    flag0 = true;
                }
                else
                {
                    if (sSoTien.Substring(0, 1) == "1")
                    {
                        chuoi = chuoi + "mười ";
                    }
                    else
                    {
                        chuoi = chuoi + kyso[Convert.ToInt16(sSoTien.Substring(0, 1))] + "mươi ";
                        flag1 = true;
                    }
                }
                sSoTien = sSoTien.Substring(1, 1);
            }
            //Xet hang don vi
            if (sSoTien.Substring(sSoTien.Length - 1, 1) != "0")
            {
                if (sSoTien.Substring(0, 1) == "5" & !flag0)
                {
                    if (temp.Length == 1)
                    {
                        chuoi = chuoi + "năm ";
                    }
                    else
                    {
                        chuoi = chuoi + "lăm ";
                    }
                }
                else
                {
                    if (sSoTien.Substring(0, 1) == "1" && !(!flag1 | flag0) & !string.IsNullOrEmpty(chuoi))
                    {
                        chuoi = chuoi + "mốt ";
                    }
                    else
                    {
                        chuoi = chuoi + kyso[Convert.ToInt16(sSoTien.Substring(0, 1))] + "";
                    }
                }
            }


            return chuoi;
        }

        private string read(Int64 number)
        {
            string a = "";
            a = NumberToWords(number);
            a = a.Trim();
            a = a.Substring(0, 1).ToUpper() + a.Substring(1, a.Length - 1);
            a = a.Replace("   ", " ");
            a = a.Replace("  ", " ");
            a = a.Replace("- ", "-");
            return a;
        }

        public static string NumberToWords(Int64 number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000000000000000) > 0)
            {
                words += NumberToWords(number / 1000000000000000000) + " thousands quadrillion ";
                number %= 1000000000000000000;
            }

            if ((number / 1000000000000000) > 0)
            {
                words += NumberToWords(number / 1000000000000000) + " quadrillion ";
                number %= 1000000000000000;
            }

            if ((number / 1000000000000) > 0)
            {
                words += NumberToWords(number / 1000000000000) + " trillion ";
                number %= 1000000000000;
            }

            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " billion ";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += " and ";

                var unitsMap = new[] { " zero", " one", " two", " three", " four", " five", " six", " seven", " eight", " nine", " ten", " eleven", " twelve", " thirteen", " fourteen", " fifteen", " sixteen", " seventeen", " eighteen", " nineteen" };
                var tensMap = new[] { " zero", " ten", " twenty", " thirty", " forty", " fifty", " sixty", " seventy", " eighty", " ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        private EntityCollection get_InstallmentManaFee(IOrganizationService crmservices, Guid OptionID, int Times)
        {// get installment with duedate calculating method = es handover 
            string fetchXml = "";
            if (Times == 1)
            {
                fetchXml =
                   @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_name' />
                    <attribute name='bsd_duedatecalculatingmethod' />
                    <attribute name='bsd_maintenanceamount' />
                    <attribute name='bsd_managementamount' />
                    <attribute name='bsd_paymentschemedetailid' />
                    <filter type='and' >
                      <condition attribute='bsd_managementamount' operator='gt' value='0' />
                      <filter type='or' >
                        <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                        <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      </filter>
                    </filter>
                  </entity>
                </fetch>";
            }
            else
            {
                fetchXml =
                   @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >
                  <entity name='bsd_paymentschemedetail' >
                    <attribute name='bsd_name' />
                    <attribute name='bsd_duedatecalculatingmethod' />
                    <attribute name='bsd_maintenanceamount' />
                    <attribute name='bsd_managementamount' />
                    <attribute name='bsd_paymentschemedetailid' />
                    <filter type='and' >
                      <condition attribute='bsd_duedatecalculatingmethod' operator='eq' value='100000002' />
                      <filter type='or' >
                        <condition attribute='bsd_reservation' operator='eq' value='{0}' />
                        <condition attribute='bsd_optionentry' operator='eq' value='{0}' />
                      </filter>
                    </filter>
                  </entity>
                </fetch>";
            }

            fetchXml = string.Format(fetchXml, OptionID);
            EntityCollection entc = crmservices.RetrieveMultiple(new FetchExpression(fetchXml));
            return entc;
        }
    }
}
