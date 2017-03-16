using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2.Utility
{
    class Helper
    {


        // return a date that is n days in future
        public static String daysInFuture(int n)
        {
            DateTime now = DateTime.Now, future;
            future = now.AddDays(n);
            return string.Format("{0:yyyy-MM-dd}", future);
        }

        public static string ReturnPassword()
        {

            return CommonUtility.GetConfigValue(ProjectConstants.PASSWORD);
        }

        public static string RetrunUsername()
        {
            return CommonUtility.GetConfigValue(ProjectConstants.USERNAME);
        }

        public static Dictionary<string, string> ReturnHttpHeader()
        {
            var httpHeaders = new Dictionary<string, string>();
            httpHeaders.Add("Username", Helper.RetrunUsername());
            httpHeaders.Add("Password", Helper.ReturnPassword());

            return httpHeaders;
        }

        public static double parseNumberWithCurrency(String numberWithCurrency)
        {
            // first 3 chars are currency code
            String price = numberWithCurrency.Substring(3);
            return Double.Parse(price);
        }
    }
}
