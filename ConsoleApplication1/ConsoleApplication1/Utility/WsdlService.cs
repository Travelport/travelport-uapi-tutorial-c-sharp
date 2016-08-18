using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Utility
{
    class WsdlService
    {
        public static string URL_PREFIX = "C://Users/MachineuserName/Documents/Visual Studio 2012/Projects/ConsoleApplication1/ConsoleApplication1";

        public static string SYSTEM_WSDL = "Wsdl/System_v32_0/system.wsdl";
        public static string AIR_WSDL = "Wsdl/Air_v35_0/air.wsdl";
        public static string HOTEL_WSDL = "Wsdl/Hotel_v35_0/hotel.wsdl";
        public static string VEHICLE_WSDL = "Wsdl/Vehicle_v35_0/vehicle.wsdl";
        public static string UNIVERSAL_WSDL = "Wsdl/universal_v35_0/universal.wsdl";
        public static string UTIL_WSDL = "Wsdl/util_35_0/util.wsdl";

        public static string ENDPOINT_PREFIX = "https://americas.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/";
        //"https://twsprofiler.travelport.com/Service/Default.ashx/";        
        static public String SYSTEM_ENDPOINT = ENDPOINT_PREFIX + "SystemService";
        static public String AIR_ENDPOINT = ENDPOINT_PREFIX + "AirService";
        static public String HOTEL_ENDPOINT = ENDPOINT_PREFIX + "HotelService";
        static public String VEHICLE_ENDPOINT = ENDPOINT_PREFIX + "VehicleService";
        static public String UTIL_ENDPOINT = ENDPOINT_PREFIX + "UtilService";
        static public String LOOKUP_ENDPOINT = ENDPOINT_PREFIX + "ReferenceDataLookupService";
        static public String UNIVERSAL_ENDPOINT = ENDPOINT_PREFIX + "UniversalRecordService";






        /*public static Uri getURLForWSDL(String wsdlFileInThisProject)
        {
            try
            {
                Uri url = new Uri(URL_PREFIX + wsdlFileInThisProject);
                return url;
            }
            catch (UriFormatException e)
            {
                throw new Exception("The URL to access the WSDL was not "
                        + "well-formed! Check the URLPREFIX value in the class "
                        + "WSDLService in the file Helper.java.  We tried to "
                        + "to use this url:\n" + URL_PREFIX + wsdlFileInThisProject + e.Message);

            }
        }*/
    }
}
