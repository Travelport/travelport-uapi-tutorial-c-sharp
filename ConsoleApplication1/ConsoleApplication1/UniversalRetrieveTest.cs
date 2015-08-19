using ConsoleApplication1.UniversalService;
using ConsoleApplication1.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class UniversalRetrieveTest
    {

        internal UniversalRecordRetrieveRsp RetrieveRecord(string urLocatorCode)
        {
            UniversalRecordRetrieveReq univRetReq = new UniversalRecordRetrieveReq();
            UniversalRecordRetrieveRsp univRetRsp;

            univRetReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            
            BillingPointOfSaleInfo billSaleInfo = new BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = "UAPI";

            univRetReq.BillingPointOfSaleInfo = billSaleInfo;

            univRetReq.Item = urLocatorCode;

            UniversalRecordRetrieveServicePortTypeClient client = new UniversalRecordRetrieveServicePortTypeClient("UniversalRecordRetrieveServicePort", WsdlService.UNIVERSAL_ENDPOINT);
            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                univRetRsp = client.service(null, univRetReq);
                //Console.WriteLine(lowFareSearchRsp.AirSegmentList.Count());

                IEnumerator airReservationDetails = univRetRsp.UniversalRecord.Items.GetEnumerator();

                String airLocatorCode;

                while (airReservationDetails.MoveNext())
                {
                    ConsoleApplication1.UniversalService.typeBaseAirReservation airReservation = (ConsoleApplication1.UniversalService.typeBaseAirReservation)airReservationDetails.Current;
                    airLocatorCode = airReservation.LocatorCode;

                    if (!string.IsNullOrEmpty(airLocatorCode))
                    {
                        AirTicketTest ticketing = new AirTicketTest();
                        ticketing.GenerateTicket(airLocatorCode);
                    }
                }

                return univRetRsp;
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                client.Abort();
                return null;
            }

        }
    }
}
