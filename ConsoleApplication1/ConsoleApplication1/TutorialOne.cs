using ConsoleApplication1;
using ConsoleApplication1.AirService;
using ConsoleApplication1.SystemService;
using ConsoleApplication1.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace UAPIConsumptionSamples
{
    class TutorialOne
    {
        static void Main(string[] args)
        {
            //
		    // PING REQUEST
		    //
		    String payload= "this my payload; there are many like it but this one is mine";
		    String someTraceId = "doesntmatter-8176";
		    String originApp = "UAPI";
		
		    //set up the request parameters into a PingReq object
		    PingReq req = new PingReq();
		    req.Payload = payload;
		    req.TraceId = someTraceId;
		
		    ConsoleApplication1.SystemService.BillingPointOfSaleInfo billSetInfo = new ConsoleApplication1.SystemService.BillingPointOfSaleInfo();
		    billSetInfo.OriginApplication = originApp;
		
		    req.BillingPointOfSaleInfo = billSetInfo;
            req.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            Console.WriteLine(req);

		
		
		    try {
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
                

			    PingRsp rsp = client.service(req);
			    //print results.. payload and trace ID are echoed back in response
			    Console.WriteLine(rsp.Payload);
			    //Console.WriteLine(rsp.TraceId);
                //Console.WriteLine(rsp.TransactionId);

                AirportDetails airports = new AirportDetails();
                //Here we are getting the list of airports, we can use it anyway we want
                IDictionary<String, String> airportsList = airports.AllAirportsList();               
                

                //Here we are getting the list of airports in a particular city, we are harcoding the city as New York here
                IDictionary<String, String> airportInCityList = airports.GetAllAiportsFromParticualrCity("New York");


                AirSvcTest airTest = new AirSvcTest();
                airTest.Availability();

                AirLFSTest lfsTest = new AirLFSTest();
                Boolean solutionResult = false; //Change it to true if you want AirPricingSolution, by default it is false
                                                //and will send AirPricePoint in the result
                LowFareSearchRsp lowFareRsp = lfsTest.LowFareShop(solutionResult);

                if (lowFareRsp != null)
                {

                    typeBaseAirSegment[] airSegments = lowFareRsp.AirSegmentList;
                    List<typeBaseAirSegment> pricingSegments =  new List<typeBaseAirSegment>();
                    
                    IEnumerator items = lowFareRsp.Items.GetEnumerator();
                    AirPricingSolution lowestFare = null;
                    AirPricePoint lowest = null;
                    
                    while (items.MoveNext())
                    {
                        if (solutionResult)
                        {
                            AirPricingSolution airPricingSolution = (AirPricingSolution)items.Current;
                            if (lowestFare == null)
                            {
                                lowestFare = airPricingSolution;
                            }
                            else
                            {
                                if (Helper.ConvertToDecimal(lowestFare.TotalPrice) > Helper.ConvertToDecimal(airPricingSolution.TotalPrice))
                                {
                                    lowestFare = airPricingSolution;
                                }
                            }
                        }
                        else
                        {
                            AirPricePointList airPricePointList = (AirPricePointList)items.Current;

                            if (airPricePointList != null)
                            {
                                foreach (var airPricePoint in airPricePointList.AirPricePoint)
                                {
                                    if (lowest == null)
                                    {
                                        lowest = airPricePoint;
                                    }
                                    else
                                    {
                                        if (Helper.ConvertToDecimal(lowest.TotalPrice) > Helper.ConvertToDecimal(airPricePoint.TotalPrice))
                                        {
                                            lowest = airPricePoint;
                                        }
                                    }
                                }
                            }
                        }
                       
                    }
                    if (lowestFare != null)
                    {
                        IEnumerator journeys = lowestFare.Journey.GetEnumerator();
                        while (journeys.MoveNext())
                        {
                            Journey journeyDetails = (Journey)journeys.Current;
                            if (journeyDetails != null)
                            {
                                AirSegmentRef[] segmentRef = journeyDetails.AirSegmentRef;
                                string refKey = segmentRef[0].Key;
                                IEnumerator airSegmentList = airSegments.GetEnumerator();
                                while (airSegmentList.MoveNext())
                                {
                                    typeBaseAirSegment airSeg = (typeBaseAirSegment)airSegmentList.Current;
                                    if (airSeg.Key.CompareTo(refKey) == 0)
                                    {
                                        pricingSegments.Add(airSeg);
                                        break;
                                    }

                                }
                            }
                        }                                               
                    }

                    if (lowest != null)
                    {
                        IEnumerator pricingInfos = lowest.AirPricingInfo.GetEnumerator();

                        while (pricingInfos.MoveNext())
                        {
                            AirPricingInfo priceInfo = (AirPricingInfo)pricingInfos.Current;
                            if (priceInfo != null)
                            {
                                foreach (var flightOption in priceInfo.FlightOptionsList)
                                {
                                    FlightOption option = flightOption;
                                    IEnumerator options = option.Option.GetEnumerator();
                                    if (options.MoveNext())
                                    {
                                        Option opt = (Option)options.Current;
                                        if (opt != null)
                                        {
                                            IEnumerator bookingInfoList = opt.BookingInfo.GetEnumerator();
                                            if (bookingInfoList.MoveNext())
                                            {
                                                BookingInfo bookingInfo = (BookingInfo)bookingInfoList.Current;
                                                if(bookingInfo != null)
                                                {
                                                    String key = bookingInfo.SegmentRef;
                                                    IEnumerator airSegmentList = airSegments.GetEnumerator();
                                                    while (airSegmentList.MoveNext())
                                                    {
                                                        typeBaseAirSegment airSeg = (typeBaseAirSegment)airSegmentList.Current;
                                                        if (airSeg.Key.CompareTo(key) == 0)
                                                        {
                                                            pricingSegments.Add(airSeg);
                                                            break;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    AirPriceRsp priceRsp = AirReq.AirPrice(pricingSegments);
                    AirPricingSolution lowestPrice = null;
                    if (priceRsp != null)
                    {
                        if (priceRsp.AirPriceResult != null)
                        {
                            IEnumerator priceResults = priceRsp.AirPriceResult.GetEnumerator();
                            if (priceResults.MoveNext())//We would take  the first Price Result and will Search for the lowest Price
                            {
                                AirPriceResult result = (AirPriceResult)priceResults.Current;
                                if (result.AirPricingSolution != null)
                                {
                                    IEnumerator priceingSolutions = result.AirPricingSolution.GetEnumerator();
                                    while (priceingSolutions.MoveNext())
                                    {
                                        AirPricingSolution priceSol = (AirPricingSolution)priceingSolutions.Current;
                                        if (lowestPrice == null)
                                        {
                                            lowestPrice = priceSol;
                                        }
                                        else
                                        {
                                            if (Helper.ConvertToDecimal(lowestPrice.TotalPrice) > Helper.ConvertToDecimal(priceSol.TotalPrice))
                                            {
                                                lowestPrice = priceSol;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (lowestPrice != null && priceRsp.AirItinerary != null)
                        {
                            AirBookTest book = new AirBookTest();
                            ConsoleApplication1.UniversalService.AirCreateReservationRsp bookResponse = book.AirBook(lowestPrice, priceRsp.AirItinerary);

                            if (bookResponse != null)
                            {
                                var urLocatorCode = bookResponse.UniversalRecord.LocatorCode;
                                Console.WriteLine("Universal Record Locator Code :" + urLocatorCode);
                            }
                        }
                    }
                }
		    } catch (Exception e) {
			    //usually only the error message is useful, not the full stack
			    //trace, since the stack trace in is your address space...
			    Console.WriteLine("Error : "+e.Message);
		    }
        }

    }
}
