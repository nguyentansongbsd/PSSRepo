using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin_Invoice_VNPT_SubmitInvoice.Helpers
{
    public class MoneyToTextHelper
    {
        public static string TienBangChu(string sSoTienIn, bool thapPhan)
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
    }
}
