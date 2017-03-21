using UAPIConsumptionSamples;
using UAPIConsumptionSamples.SystemService;
using ConsoleApplication3.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using UAPIConsumptionSamples.RailService;
using System.Collections;

namespace UAPIConsumptionSamples
{
    class TutorialThree
    {
        static void Main(string[] args)
        {
            //
            // PING REQUEST
            //
            String payload = "this my payload; there are many like it but this one is mine";
            String someTraceId = "doesntmatter-8176";
            String originApp = "UAPI";

            //set up the request parameters into a PingReq object
            PingReq req = new PingReq();
            req.Payload = payload;
            req.TraceId = someTraceId;

            UAPIConsumptionSamples.SystemService.BillingPointOfSaleInfo billSetInfo = new UAPIConsumptionSamples.SystemService.BillingPointOfSaleInfo();
            billSetInfo.OriginApplication = originApp;

            req.BillingPointOfSaleInfo = billSetInfo;
            req.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            Console.WriteLine(req);



            try
            {
                //run the ping request
                //WSDLService.sysPing.showXML(true);
                SystemPingPortTypeClient client = new SystemPingPortTypeClient("SystemPingPort", WsdlService.SYSTEM_ENDPOINT);
                //Console.WriteLine(client.Endpoint);
                client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
                client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
                /*var httpHeaders = new Dictionary<string, string>();
                httpHeaders.Add("Username", "travelportsuperadmin");
                httpHeaders.Add("Password", "abc123");
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));*/

                /*HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
                httpRequestProperty.Headers[HttpRequestHeader.Authorization] = "Basic " +
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(client.ClientCredentials.UserName.UserName +
                    ":" + client.ClientCredentials.UserName.Password));

                 using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                    {
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] =
                            httpRequestProperty;
                        return client.processRequest(castRequest) as TSRsp;
                    } 


                OperationContext.Current.OutgoingMessageProperties*/



                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));
                //String soapMsg = Helper.ObjectToSOAP(req);
                PingRsp rsp = client.service(req);
                //print results.. payload and trace ID are echoed back in response
                Console.WriteLine(rsp.Payload);
                //Console.WriteLine(rsp.TraceId);
                //Console.WriteLine(rsp.TransactionId);
            }
            catch (Exception e)
            {
                //usually only the error message is useful, not the full stack
                //trace, since the stack trace in is your address space...
                Console.WriteLine("Error : " + e.Message);
            }

            VehicleSvcTest vehicleTest = new VehicleSvcTest();
            vehicleTest.ProcessVehicleFlow();

            RailSvcTest railtest = new RailSvcTest();
            RailAvailabilitySearchRsp railSearchRsp = railtest.ProcessRailFlow();

            if (railSearchRsp != null)
            {
                RailPricingSolution lowestPrice = new RailPricingSolution(){
                    TotalPrice = "0"
                };

                List<RailJourney> journey = new List<RailJourney>();

                List<RailSegment> selectedSegmentList = new List<RailSegment>();

                List<RailFare> railFareList = new List<RailFare>();

                List<RailBookingInfo> bookingInfoList = new List<RailBookingInfo>();

                if (railSearchRsp.RailPricingSolution != null && railSearchRsp.RailPricingSolution.Count<RailPricingSolution>() > 0)
                {
                    IEnumerator<RailPricingSolution> railPricingSoltionList = railSearchRsp.RailPricingSolution.ToList().GetEnumerator();
                    while (railPricingSoltionList.MoveNext())
                    {
                        RailPricingSolution railPriceSol = railPricingSoltionList.Current;

                        if (Helper.ReturnValue(lowestPrice.TotalPrice) == 0)
                        {
                            lowestPrice = railPriceSol;
                        }
                        else if (Helper.ReturnValue(railPriceSol.TotalPrice) < Helper.ReturnValue(lowestPrice.TotalPrice))
                        {
                            lowestPrice = railPriceSol;
                        }
                    }


                    if (Helper.ReturnValue(lowestPrice.TotalPrice) > 0)
                    {
                        IEnumerator<RailJourney> journeyList = railSearchRsp.RailJourneyList.ToList().GetEnumerator();
                        IEnumerator journeyRefList = lowestPrice.Items.GetEnumerator();

                        while (journeyRefList.MoveNext())
                        {
                            RailJourneyRef j = (RailJourneyRef)journeyRefList.Current;

                            while (journeyList.MoveNext())
                            {
                                RailJourney currJourney = journeyList.Current;
                                if (j.Key.CompareTo(currJourney.Key) == 0)
                                {
                                    journey.Add(currJourney);
                                }
                            }
                        }
                    }

                    IEnumerator<RailJourney> railJourneyList = journey.GetEnumerator();
                    IEnumerator<RailSegment> railSegmentList = railSearchRsp.RailSegmentList.ToList().GetEnumerator();

                    while (railJourneyList.MoveNext())
                    {
                        RailJourney railJourney = railJourneyList.Current;

                        IEnumerator segmentRefList = railJourney.Items.GetEnumerator();
                        while (segmentRefList.MoveNext())
                        {
                            RailSegmentRef segRef = (RailSegmentRef)segmentRefList.Current;

                            while (railSegmentList.MoveNext())
                            {
                                RailSegment segment = railSegmentList.Current;
                                if (segRef.Key.CompareTo(segment.Key) == 0)
                                {
                                    selectedSegmentList.Add(segment);
                                }
                            }
                        }
                    }

                    IEnumerator<RailPricingInfo> railPriceInfoList = lowestPrice.RailPricingInfo.ToList().GetEnumerator();
                    IEnumerator<RailFare> railFares = railSearchRsp.RailFareList.ToList().GetEnumerator();

                    while (railPriceInfoList.MoveNext())
                    {
                        RailPricingInfo priceInfo = railPriceInfoList.Current;

                        IEnumerator fareList = priceInfo.Items.ToList().GetEnumerator();

                        while (fareList.MoveNext())
                        {
                            RailFareRef fareRef = (RailFareRef)fareList.Current;

                            while (railFares.MoveNext())
                            {
                                RailFare fare = railFares.Current;

                                if (fareRef.Key.CompareTo(fare.Key) == 0)
                                {
                                    railFareList.Add(fare);
                                }
                            }
                        }

                        IEnumerator<RailBookingInfo> infoList = priceInfo.RailBookingInfo.ToList().GetEnumerator();
                        while (infoList.MoveNext())
                        {
                            RailBookingInfo bookingInfo = infoList.Current;
                            bookingInfoList.Add(bookingInfo);
                        }

                    }


                }

                railtest.ProcessRailBookFlow(lowestPrice, journey, selectedSegmentList, railFareList, bookingInfoList);
                

            }
        }

    }
}
