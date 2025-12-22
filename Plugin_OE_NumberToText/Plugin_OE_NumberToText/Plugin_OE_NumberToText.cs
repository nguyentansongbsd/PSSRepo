using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin_OE_NumberToText
{
    public class Plugin_OE_NumberToText : IPlugin
    {
        IOrganizationService service = null;
        ITracingService traceService = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = factory.CreateOrganizationService(context.UserId);
            traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            traceService.Trace("start");
            if (context.Depth > 4) return;

            Entity target = (Entity)context.InputParameters["Target"];
            traceService.Trace($"target {target.LogicalName} {target.Id}");

            switch (target.LogicalName)
            {
                case "bsd_paymentschemedetail":
                    Entity enIns = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_optionentry", "bsd_amountwaspaid" }));
                    if (!enIns.Contains("bsd_optionentry") ||
                        ("Create".Equals(context.MessageName) && (!enIns.Contains("bsd_amountwaspaid") || ((Money)enIns["bsd_amountwaspaid"]).Value == 0)))
                        return;

                    Up_totalpaidincludecoa(enIns, "bsd_totalamountpaidinstallment");
                    break;
                case "bsd_advancepayment":
                    if (!(target.Contains("statuscode") && ((OptionSetValue)target["statuscode"]).Value == 100000000))  //Collected
                        return;

                    Entity enAdvencePayment = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_optionentry" }));
                    if (!enAdvencePayment.Contains("bsd_optionentry"))
                        return;

                    Up_totalpaidincludecoa(enAdvencePayment, "bsd_totaladvancepayment");
                    break;
                case "salesorder":
                    Entity enSalesOrder = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa", "bsd_refundableamount",
                                                                            "bsd_additionalpayment", "bsd_finalrefundamount"}));
                    Entity enUp = new Entity(enSalesOrder.LogicalName, enSalesOrder.Id);
                    enUp["bsd_totalpaidincludecoatext"] = GetTienBangChu_VN(enSalesOrder, "bsd_totalpaidincludecoa");
                    enUp["bsd_totalpaidincludecoatexten"] = GetTienBangChu_ENG(enSalesOrder, "bsd_totalpaidincludecoa");

                    enUp["bsd_refundableamounttext"] = GetTienBangChu_VN(enSalesOrder, "bsd_refundableamount");
                    enUp["bsd_refundableamounttexten"] = GetTienBangChu_ENG(enSalesOrder, "bsd_refundableamount");

                    enUp["bsd_additionalpaymenttext"] = GetTienBangChu_VN(enSalesOrder, "bsd_additionalpayment");
                    enUp["bsd_additionalpaymenttexten"] = GetTienBangChu_ENG(enSalesOrder, "bsd_additionalpayment");

                    enUp["bsd_finalrefundamounttext"] = GetTienBangChu_VN(enSalesOrder, "bsd_finalrefundamount");
                    enUp["bsd_finalrefundamounttexten"] = GetTienBangChu_ENG(enSalesOrder, "bsd_finalrefundamount");

                    service.Update(enUp);
                    break;
            }

            traceService.Trace("done");
        }

        private void Up_totalpaidincludecoa(Entity enTarget, string fieldReset)
        {
            EntityReference refOE = (EntityReference)enTarget["bsd_optionentry"];
            CalculateRollupFieldRequest request = new CalculateRollupFieldRequest
            {
                Target = refOE,
                FieldName = fieldReset
            };
            service.Execute(request);

            Entity enSalesOrder = service.Retrieve(refOE.LogicalName, refOE.Id, new ColumnSet(new string[] { "bsd_totalpaidincludecoa" }));

            Entity enUp = new Entity(enSalesOrder.LogicalName, enSalesOrder.Id);
            enUp["bsd_totalpaidincludecoatext"] = GetTienBangChu_VN(enSalesOrder, "bsd_totalpaidincludecoa");
            enUp["bsd_totalpaidincludecoatexten"] = GetTienBangChu_ENG(enSalesOrder, "bsd_totalpaidincludecoa");

            service.Update(enUp);
        }

        private string GetTienBangChu_VN(Entity enHD, string getName)
        {
            decimal tien = enHD.Contains(getName) ? ((Money)enHD[getName]).Value : 0;
            string[] sotien = tien.ToString().Split('.');
            return TienBangChu(sotien[0], false);
        }

        public string TienBangChu(string sSoTienIn, bool thapPhan)
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

            string tmpChuoiZero = "";
            Regex r = new Regex(@"^[0]*");
            if (thapPhan && sSoTien.StartsWith("0"))
            {
                foreach (char tmpSo in sSoTienIn)
                {
                    if (tmpSo.ToString() == "0") tmpChuoiZero += "không ";
                }
            }

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

            return am + tmpChuoiZero + temp + chuoi.Substring(1, chuoi.Length - 2) + " đồng";

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

        private string GetTienBangChu_ENG(Entity enHD, string getName)
        {
            decimal tien = enHD.Contains(getName) ? ((Money)enHD[getName]).Value : 0;
            return NumberToWords(tien, "Vietnamese Dong");
        }

        public string NumberToWords(decimal number, string currency)
        {
            if (number == 0) return "zero";

            bool isNegative = number < 0;
            number = Math.Abs(number);

            decimal integerPart = Math.Floor(number);
            decimal fractionalPart = number - integerPart; // Phần thập phân

            StringBuilder words = new StringBuilder();
            if (isNegative) words.Append("negative ");

            ConvertIntegerToWords(integerPart, words);

            if (fractionalPart > 0)
            {
                words.Append(" point ");
                ConvertDecimalToWords(fractionalPart, words);
            }

            return CapitalizeFirstLetter(words.ToString().Trim()) + " " + currency;
        }

        private void ConvertIntegerToWords(decimal number, StringBuilder words)
        {
            if (number == 0)
            {
                words.Append("zero");
                return;
            }

            string[] units = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            string[] tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string[] scales = { "", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion" };

            int scaleIndex = 0;
            while (number > 0)
            {
                decimal chunk = number % 1000;
                if (chunk > 0)
                {
                    if (words.Length > 0) words.Insert(0, " ");
                    words.Insert(0, ConvertThreeDigitNumber((int)chunk, units, tens) + (scaleIndex > 0 ? " " + scales[scaleIndex] : ""));
                }
                number = Math.Floor(number / 1000);
                scaleIndex++;
            }
        }

        private string ConvertThreeDigitNumber(int number, string[] units, string[] tens)
        {
            if (number == 0) return "";

            StringBuilder words = new StringBuilder();
            int hundreds = number / 100;
            int remainder = number % 100;

            if (hundreds > 0)
            {
                words.Append(units[hundreds] + " hundred");
            }

            if (remainder > 0)
            {
                if (words.Length > 0) words.Append(" and ");
                if (remainder < 20)
                    words.Append(units[remainder]);
                else
                {
                    words.Append(tens[remainder / 10]);
                    if (remainder % 10 > 0)
                        words.Append(" " + units[remainder % 10]);
                }
            }

            return words.ToString();
        }

        private void ConvertDecimalToWords(decimal number, StringBuilder words)
        {
            string[] units = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            number -= Math.Floor(number); // Chỉ lấy phần thập phân
            number = Math.Round(number, 28); // Giữ tối đa 28 chữ số thập phân

            while (number > 0)
            {
                number *= 10;
                int digit = (int)number; // Lấy phần nguyên đầu tiên
                words.Append(units[digit] + " ");
                number -= digit; // Loại bỏ phần nguyên vừa lấy
            }
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}