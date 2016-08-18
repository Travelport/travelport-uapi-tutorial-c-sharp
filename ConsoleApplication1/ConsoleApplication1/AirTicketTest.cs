using ConsoleApplication1.AirService;
using ConsoleApplication1.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class AirTicketTest
    {
        internal void GenerateTicket(string airLocatorCode)
        {
            AirTicketingReq airTicketReq = new AirTicketingReq();
            AirTicketingRsp airTicketRsp;

            airTicketReq.BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
            {
                OriginApplication = "UAPI"
            };

            airTicketReq.AirReservationLocatorCode = new AirReservationLocatorCode()
            {
                Value = airLocatorCode
            };

            List<AirTicketingModifiers> ticketingModifiers = new List<AirTicketingModifiers>();

            List<FormOfPayment> fopList = new List<FormOfPayment>();
            fopList.Add(new FormOfPayment()
            {
                FulfillmentIDType = typeFulfillmentIDType.CreditCard,
                Type = "Credit",
                Item = new CreditCard()
                {
                    Number = "4111111111111111",
                    ExpDate = "2016-06",
                    CVV = "123",
                    Type = "VI",
                    BillingAddress = new typeStructuredAddress()
                    {
                        AddressName = "Home",
                        Street = new string[] { "2914 N. Dakota Avenue" },
                        City = "Denver",
                        State = new State()
                        {
                            Value = "CO"
                        },
                        PostalCode = "80206",
                        Country = "US"
                    }
                }
            });

            ticketingModifiers.Add(new AirTicketingModifiers()
            {
                FormOfPayment = fopList.ToArray()
            });

            airTicketReq.AirTicketingModifiers = ticketingModifiers.ToArray();

            airTicketReq.TargetBranch = Utility.CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

            AirTicketingPortTypeClient client = new AirTicketingPortTypeClient("AirTicketingPort", WsdlService.AIR_ENDPOINT);
            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                airTicketRsp = client.service(airTicketReq);
                //Console.WriteLine(lowFareSearchRsp.AirSegmentList.Count());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Tickting : " + e.Message);
            }

        }
    }
}
