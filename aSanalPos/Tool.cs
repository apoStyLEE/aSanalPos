using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aSanalPos
{
    public class Tool
    {
        public static string GetIp()
        {
            if (string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.ServerVariables["remote_addr"]))
            {
                return "127.0.0.1";
            }
            else
            {
                return System.Web.HttpContext.Current.Request.ServerVariables["remote_addr"].ToString();
            }
        }

        public static bool IsNumeric(string value)
        {
            int result;
            return int.TryParse(value, out result);
        }
        
        public static bool IsTcKimlik(string tcKimlikNo)
        {
            bool returnvalue = false;

            if (tcKimlikNo.Length == 11)
            {
                char[] charlar = tcKimlikNo.ToCharArray(0, 10);
                int sayi = 0;

                foreach (char item in charlar)
                {
                    sayi += int.Parse(item.ToString());
                }
                string sayistr = sayi.ToString();

                if (sayistr.Substring(sayistr.Length - 1) == tcKimlikNo.Substring(10))
                {
                    returnvalue = true;
                }
            }
            return returnvalue;
        }

        public static string RandomNumber()
        {
            Random r = new Random();
            string strRsayi = r.Next(1, 10000000).ToString() + String.Format("{0:T}", DateTime.Now).Replace(":", string.Empty);
            return strRsayi;
        }
    }
}
