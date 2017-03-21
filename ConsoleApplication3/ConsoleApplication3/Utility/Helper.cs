using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text.RegularExpressions;

namespace ConsoleApplication3.Utility
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

        public static decimal ReturnValue(string input)
        {
            decimal value = 0;

            if (!string.IsNullOrEmpty(input))
            {
                string regex = Regex.Replace(input, "[^-?\\d+\\.]", "");
                //Match match = regex.Match(input);
                if (!string.IsNullOrEmpty(regex))
                {
                    value = decimal.Parse(regex, CultureInfo.InvariantCulture);
                }
            }

            return value;
        }

        public static Dictionary<string, string> ReturnHttpHeader()
        {
            var httpHeaders = new Dictionary<string, string>();
            //httpHeaders.Add("Username", Helper.RetrunUsername());
            //httpHeaders.Add("Password", Helper.ReturnPassword());

            httpHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Helper.RetrunUsername() + ":" + Helper.ReturnPassword())));
            httpHeaders.Add("Accept-Encoding", "gzip");

            return httpHeaders;
        }

        public static string ObjectToSOAP(object Object)
        {
            if (Object == null)
            {
                throw new ArgumentException("Object can not be null");
            }
            
            using (MemoryStream Stream = new MemoryStream())
            {
                SoapFormatter Serializer = new SoapFormatter();
                Serializer.Serialize(Stream, Object);
                Stream.Flush();

                return UTF8Encoding.UTF8.GetString(Stream.GetBuffer(), 0, (int)Stream.Position);
            }

        }
    }
}
