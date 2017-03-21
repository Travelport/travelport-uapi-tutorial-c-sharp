using ConsoleApplication3.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAPIConsumptionSamples.RailService;

namespace UAPIConsumptionSamples
{
    class RailSvcTest
    {

        internal RailAvailabilitySearchRsp ProcessRailFlow()
        {
            RailAvailabilitySearchRsp railSearchRsp = SearchRail();
            return railSearchRsp;
        }

        private RailAvailabilitySearchRsp SearchRail()
        {

            RailAvailabilitySearchReq railAvailReq = new RailAvailabilitySearchReq()
            {
                TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH),
                AuthorizedBy = "Test",
                BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
                {
                    OriginApplication = "UAPI"
                },
                SearchRailLeg = CreateRailLeg(),
                SearchPassenger = CreatePassenger(),
                RailSearchModifiers = new RailSearchModifiers()
                {
                    PreferredSuppliers = CreateRailSuppliers(),
                }

            };

            RailAvailabilitySearchRsp railAvailRsp = new RailAvailabilitySearchRsp();


            try
            {
                //run the ping request
                //WSDLService.sysPing.showXML(true);
                RailAvailabilitySearchPortTypeClient client = new RailAvailabilitySearchPortTypeClient("RailAvailabilitySearchPort", WsdlService.RAIL_ENDPOINT);
                //Console.WriteLine(client.Endpoint);
                client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
                client.ClientCredentials.UserName.Password = Helper.ReturnPassword();



                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                railAvailRsp = client.service(railAvailReq);
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

            return railAvailRsp;            

        }

        private RailSupplier[] CreateRailSuppliers()
        {
            List<RailSupplier> supplierList = new List<RailSupplier>();
            supplierList.Add(new RailSupplier()
            {
                Code = "2C"
            });

            return supplierList.ToArray();
        }

        private SearchPassenger[] CreatePassenger()
        {
            List<SearchPassenger> passengerList = new List<SearchPassenger>();
            passengerList.Add(new SearchPassenger()
            {
                Age = "34",
                Code = "ADT"
            });

            return passengerList.ToArray();
        }

        private SearchRailLeg[] CreateRailLeg()
        {
            List<SearchRailLeg> railLeg = new List<SearchRailLeg>();

            List<typeSearchLocation> originList = new List<typeSearchLocation>();
            originList.Add(new typeSearchLocation()
            {
                Item = new RailLocation()
                {
                    Code = "U8728600"
                }
            });

            List<typeSearchLocation> destList = new List<typeSearchLocation>();
            destList.Add(new typeSearchLocation()
            {
                Item = new RailLocation()
                {
                    Code = "U8727100"
                }
            });

            List<typeTimeSpec> originTimeList = new List<typeTimeSpec>();
            originTimeList.Add(new typeTimeSpec()
            {
                PreferredTime = DateTime.Now.AddDays(60).ToString("yyyy-MM-dd")
            });

            List<typeTimeSpec> destTimeList = new List<typeTimeSpec>();
            destTimeList.Add(new typeTimeSpec()
            {
                PreferredTime = DateTime.Now.AddDays(65).ToString("yyyy-MM-dd")
            });

            railLeg.Add(new SearchRailLeg()
            {
                SearchOrigin = originList.ToArray(),
                SearchDestination = destList.ToArray(),
                Items = originTimeList.ToArray()
            });

            railLeg.Add(new SearchRailLeg()
            {
                SearchOrigin = destList.ToArray(),
                SearchDestination = originList.ToArray(),
                Items = destTimeList.ToArray()
            });

            return railLeg.ToArray();
        }


        internal void ProcessRailBookFlow(RailPricingSolution lowestPrice, List<RailJourney> journey, List<RailSegment> selectedSegmentList, List<RailFare> railFareList, List<RailBookingInfo> bookingInfoList)
        {
            UniversalService.RailCreateReservationReq railReservationReq = new UniversalService.RailCreateReservationReq();

            railReservationReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

            railReservationReq.BillingPointOfSaleInfo = new UniversalService.BillingPointOfSaleInfo() 
            { 
                OriginApplication = "UAPI"
            };

            railReservationReq.BookingTraveler = AddBookingTravler();
            railReservationReq.RailPricingSolution = new UniversalService.RailPricingSolution()
            {
                Key = lowestPrice.Key,
                OfferId = lowestPrice.OfferId,
                TotalPrice = lowestPrice.TotalPrice,
                ApproximateTotalPrice = lowestPrice.ApproximateTotalPrice,
                ProviderCode = lowestPrice.ProviderCode,
                SupplierCode = lowestPrice.SupplierCode,
                RailPricingInfo = AddRailPricingInfo(lowestPrice.RailPricingInfo, railFareList, bookingInfoList),
                
            };

        }
        

        private UniversalService.RailPricingInfo[] AddRailPricingInfo(RailPricingInfo[] railPricingInfo, List<RailFare> railFareList, List<RailBookingInfo> bookingInfoList)
        {
            if (railPricingInfo != null)
            {
                List<UniversalService.RailPricingInfo> railPricingList = new List<UniversalService.RailPricingInfo>();

                IEnumerator<RailPricingInfo> priceList = railPricingInfo.ToList().GetEnumerator();


                while (priceList.MoveNext())
                {
                    RailPricingInfo priceInfo = priceList.Current;

                    IEnumerator<RailBookingInfo> bookingInfos = priceInfo.RailBookingInfo.ToList().GetEnumerator();

                    List<UniversalService.RailBookingInfo> railBookingInfos = new List<UniversalService.RailBookingInfo>();

                    while (bookingInfos.MoveNext())
                    {
                        RailBookingInfo info = bookingInfos.Current;
                        railBookingInfos.Add(new UniversalService.RailBookingInfo()
                        {
                            RailFareRef = info.RailFareRef,
                            RailJourneyRef = info.RailJourneyRef
                        });
                    }

                    railPricingList.Add(new UniversalService.RailPricingInfo()
                    {
                        Key = priceInfo.Key,
                        TotalPrice = priceInfo.TotalPrice,
                        ApproximateTotalPrice = priceInfo.ApproximateTotalPrice,
                        Items = railFareList.ToArray(),
                        RailBookingInfo = railBookingInfos.ToArray()
                    });
                }


                return railPricingList.ToArray();
            }

            return null;
        }

        private UniversalService.BookingTraveler[] AddBookingTravler()
        {
            List<UniversalService.BookingTraveler> travelers = new List<UniversalService.BookingTraveler>();

            //Adding First Booking Traveler
            UniversalService.BookingTraveler traveler = new UniversalService.BookingTraveler();
            traveler.DOB = DateTime.Now.AddYears(-28);
            traveler.Gender = "M";
            traveler.TravelerType = "ADT";
            traveler.Key = "gr8AVWGCR064r57Jt0+8bA==";
            traveler.Nationality = "US";

            traveler.BookingTravelerName = new UniversalService.BookingTravelerName()
            {
                First = "Jack",
                Last = "Smith",
                Prefix = "Mr"
            };

            UniversalService.DeliveryInfoShippingAddress shipping = new UniversalService.DeliveryInfoShippingAddress()
            {
                AddressName = "Home",
                Street = new string[] { "2914 N. Dakota Avenue" },
                City = "Denver",
                State = new UniversalService.State()
                {
                    Value = "CO"
                },
                PostalCode = "80206",
                Country = "US"
            };

            List<UniversalService.DeliveryInfo> deliveryInfoList = new List<UniversalService.DeliveryInfo>();
            UniversalService.DeliveryInfo deliveryInfo = new UniversalService.DeliveryInfo()
            {
                ShippingAddress = shipping
            };

            deliveryInfoList.Add(deliveryInfo);

            traveler.DeliveryInfo = deliveryInfoList.ToArray();

            List<UniversalService.PhoneNumber> phoneList = new List<UniversalService.PhoneNumber>();

            UniversalService.PhoneNumber phoneNum = new UniversalService.PhoneNumber()
            {
                AreaCode = "303",
                CountryCode = "1",
                Number = "3333333",
                Location = "DEN"
            };

            phoneList.Add(phoneNum);

            traveler.PhoneNumber = phoneList.ToArray();

            List<UniversalService.Email> emailList = new List<UniversalService.Email>();

            UniversalService.Email email = new UniversalService.Email()
            {
                EmailID = "test@travelport.com",
                Type = "Home"
            };

            emailList.Add(email);

            traveler.Email = emailList.ToArray();

            List<UniversalService.typeStructuredAddress> addressList = new List<UniversalService.typeStructuredAddress>();

            UniversalService.typeStructuredAddress address = new UniversalService.typeStructuredAddress()
            {
                AddressName = "Home",
                Street = new string[] { "2914 N. Dakota Avenue" },
                City = "Denver",
                State = new UniversalService.State()
                {
                    Value = "CO"
                },
                PostalCode = "80206",
                Country = "US"
            };

            addressList.Add(address);

            traveler.Address = addressList.ToArray();

            travelers.Add(traveler);

            return travelers.ToArray();
        }
    }
}
