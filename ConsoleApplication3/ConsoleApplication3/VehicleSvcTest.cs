using ConsoleApplication3.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAPIConsumptionSamples.VehicleService;

namespace UAPIConsumptionSamples
{
    class VehicleSvcTest
    {
        internal void ProcessVehicleFlow()
        {
            SearchVehicle();
        }

        private void SearchVehicle()
        {
            VehicleSearchAvailabilityReq vehReq = new VehicleSearchAvailabilityReq()
            {
                    
                    TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH),
                    AuthorizedBy = "Test",
                    BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
                    {
                        OriginApplication = "F8C867BC-Sandbox"
                    },
                    VehicleDateLocation = new VehicleDateLocation()
                    {
                        PickupLocation = "DEN",
                        PickupDateTime = "2015-11-23T13:50:00",
                        PickupLocationType = typeVehicleLocation.Terminal,
                        PickupLocationNumber = "01",//?????

                        ReturnLocation = "DEN",
                        ReturnDateTime = "2015-11-24T14:00:00",
                        ReturnLocationType = typeVehicleLocation.Terminal,
                        ReturnLocationNumber = "01"//?????                                    
                   },
                   ReturnAllRates = true

            };


            try
            {
                //run the ping request
                //WSDLService.sysPing.showXML(true);
                VehicleSearchServicePortTypeClient client = new VehicleSearchServicePortTypeClient("VehicleSearchServicePort", WsdlService.VEHICLE_ENDPOINT);
                //Console.WriteLine(client.Endpoint);
                client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
                client.ClientCredentials.UserName.Password = Helper.ReturnPassword();                



                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                VehicleSearchAvailabilityRsp vehRsp = client.service(vehReq);
                //print results.. payload and trace ID are echoed back in response                
                //Console.WriteLine(rsp.TraceId);
                //Console.WriteLine(rsp.TransactionId);
            }
            catch (Exception e)
            {
                //usually only the error message is useful, not the full stack
                //trace, since the stack trace in is your address space...
                Console.WriteLine("Error : " + e.Message);
            }

        }
    }
}
