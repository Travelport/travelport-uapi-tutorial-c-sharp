using ConsoleApplication2.Utility;
using ConsoleApplication2.HotelService;
using ConsoleApplication2.UniversalService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class HotelTutorial
    {

        string hotelLoc = "ATL";
        string hotelRefPoint = "EIFFEL TOWER";
        int numberOfAdults = 2;
        int numberOfRooms = 1;
        string providerCode = "1G";

        BaseHotelSearchRsp hotelSearchAvailabilityResponse;
        HotelDetailsRsp detailsResponse;
        HotelCreateReservationRsp hotelCreateReservationRsp;
        HotelRulesRsp rulesResponse;

        private static String closestHotelCode;
        private static String cheapestHotelCode;
        private static String rateSupplier;

        HotelService.HostToken hostToken = new HotelService.HostToken();        

        #region public method

        public static String ClosestHotelCode
        {
            get { return HotelTutorial.closestHotelCode; }
            set { HotelTutorial.closestHotelCode = value; }
        }

        public static String CheapestHotelCode
        {
            get { return HotelTutorial.cheapestHotelCode; }
            set { HotelTutorial.cheapestHotelCode = value; }
        }

        public static String RateSupplier
        {
            get { return HotelTutorial.rateSupplier; }
            set { HotelTutorial.rateSupplier = value; }
        }

        /// <summary>
        /// Create hotel Search request and get hotel availability
        /// </summary>
        /// <returns></returns>
        public BaseHotelSearchRsp HotelAvailabilty()
        {
            //Create Hotel Search Availabilty Request
            HotelSearchAvailabilityReq hotelSearchAvailabilityRequest = new HotelSearchAvailabilityReq(); // Create an instance of HotelSearchReq and set the
                                                                                                          //required parameters

            hotelSearchAvailabilityRequest.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            hotelSearchAvailabilityRequest.TraceId = "Trace";
            hotelSearchAvailabilityRequest.AuthorizedBy = "user";
            

            //Add billing point of sale information
            HotelService.BillingPointOfSaleInfo billSaleInfo = new HotelService.BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = CommonUtility.GetConfigValue(ProjectConstants.APP);

            hotelSearchAvailabilityRequest.BillingPointOfSaleInfo = billSaleInfo;

            HotelSearchLocation hotelSearchLocation = new HotelSearchLocation();
            hotelSearchLocation.HotelLocation = new HotelLocation()
            {
                Location = hotelLoc,
                LocationType = typeHotelLocation.City
            };

            /*hotelSearchLocation.ReferencePoint = new typeHotelReferencePoint()
            {
                Value = hotelRefPoint
            };*/

            /*List<string> streetAddress = new List<string>();
            streetAddress.Add("300 Galleria Pkway");

            hotelSearchLocation.HotelAddress = new ConsoleApplication2.HotelService.typeStructuredAddress()
            {
                Street = streetAddress.ToArray()
            };

            

            hotelSearchLocation.HotelAddress.City = "Atlanta";*/


            hotelSearchAvailabilityRequest.HotelSearchLocation = hotelSearchLocation;
            HotelService.HotelStay hotelStay = GetHotelStay();

            hotelSearchAvailabilityRequest.HotelStay = hotelStay;
            //HotelChain hc = new HotelChain();
            //hc.Code = "HI";

            HotelSearchModifiers hotelSearchModifiers = new HotelSearchModifiers()
            {
                NumberOfAdults = numberOfAdults,
                NumberOfRooms = numberOfRooms,
                PermittedProviders = new HotelService.PermittedProviders(){
                    Provider = new HotelService.Provider(){
                        Code = providerCode
                    }
                },
                //PermittedChains = new HotelChain[]{hc},
                AvailableHotelsOnly = true              

            };

            hotelSearchAvailabilityRequest.HotelSearchModifiers = hotelSearchModifiers;

            HotelSearchServicePortTypeClient hotelSearchclient = new HotelSearchServicePortTypeClient("HotelSearchServicePort", WsdlService.HOTEL_ENDPOINT);
            hotelSearchclient.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            hotelSearchclient.ClientCredentials.UserName.Password = Helper.ReturnPassword();

            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                hotelSearchclient.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                hotelSearchAvailabilityResponse = hotelSearchclient.service(hotelSearchAvailabilityRequest);
                Console.WriteLine(hotelSearchAvailabilityResponse.HotelSearchResult.Count());
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                hotelSearchclient.Abort();
            }

            return hotelSearchAvailabilityResponse;

        }
        /// <summary>
        /// select a date 20 days later from today
        /// </summary>
        /// <returns></returns>
        private HotelService.HotelStay GetHotelStay()
        {
            HotelService.HotelStay hotelStay = new HotelService.HotelStay()
            {
                CheckinDate = Convert.ToDateTime(Helper.daysInFuture(20)),
                CheckoutDate = Convert.ToDateTime(Helper.daysInFuture(27))
            };

            return hotelStay;
        }

        /// <summary>
        /// Select the cheapest hotel from the available hotels and get the details of the selected hotel
        /// </summary>
        /// <param name="hotelResponse"></param>
        /// <returns></returns>
        public HotelDetailsRsp HotelDetails(BaseHotelSearchRsp hotelResponse)
        {

            HotelSearchResult closest = null;
            HotelSearchResult cheapest = null;

            int lowestDistance = Int32.MaxValue;
            double lowestPrice = Int32.MaxValue;

            IEnumerator<HotelSearchResult> searchResults = hotelResponse.HotelSearchResult.ToList().GetEnumerator();
            while (searchResults.MoveNext())
            {
                HotelSearchResult result = searchResults.Current;
                IEnumerator<HotelService.HotelProperty> hotelProperties = result.HotelProperty.ToList().GetEnumerator();
                if(result.RateInfo != null && result.RateInfo.Count() > 0)
                {
                    while (hotelProperties.MoveNext())
                    {
                        HotelService.HotelProperty property = hotelProperties.Current;
                        if (property.Availability.CompareTo(ConsoleApplication2.HotelService.typeHotelAvailability.Available) == 0)
                        {

                            if (property.ReserveRequirement.CompareTo(HotelService.typeReserveRequirement.Other) == 0)
                            {
                                continue;
                            }

                            if (property.Distance != null)//check lowest distance for closet hotel from the reference point
                            {
                                int distance = Convert.ToInt32(property.Distance.Value);
                                if (distance < lowestDistance)
                                {
                                    ClosestHotelCode = property.HotelCode;
                                    closest = result;
                                    lowestDistance = distance;
                                }
                            }


                            IEnumerator<RateInfo> hotelRates = result.RateInfo.ToList().GetEnumerator();
                            while (hotelRates.MoveNext())
                            {
                                RateInfo rate = hotelRates.Current;
                                double minRate = 0.0;
                                if (rate.MinimumAmount != null)
                                {
                                    minRate = Helper.parseNumberWithCurrency(rate.MinimumAmount);
                                }
                                else if (rate.ApproximateMinimumStayAmount != null)
                                {
                                    minRate = Helper.parseNumberWithCurrency(rate.ApproximateMinimumStayAmount);
                                }
                                else if (rate.ApproximateMinimumAmount != null)
                                {
                                    minRate = Helper.parseNumberWithCurrency(rate.ApproximateMinimumAmount);
                                }

                                if (minRate == 0.0)
                                {
                                    if (rate.MaximumAmount != null)
                                    {
                                        minRate = Helper.parseNumberWithCurrency(rate.MaximumAmount) / 2;
                                    }
                                    else if (rate.ApproximateMinimumAmount != null)
                                    {
                                        minRate = Helper.parseNumberWithCurrency(rate.ApproximateMinimumAmount) / 2;
                                    }
                                    else if (rate.ApproximateMaximumAmount != null)
                                    {
                                        minRate = Helper.parseNumberWithCurrency(rate.ApproximateMaximumAmount) / 2;
                                    }

                                }

                                if (minRate < lowestPrice)/// Check the lowest price
                                {
                                    CheapestHotelCode = property.HotelCode;
                                    cheapest = result;
                                    lowestPrice = minRate;
                                    if (rate.RateSupplier != null)
                                    {
                                        RateSupplier = rate.RateSupplier;
                                    }
                                }
                            }

                        }
                    }

                }
            }


             if (hotelResponse.HostToken != null)
            {
                hostToken = hotelResponse.HostToken;
            }

            if (closest == null)
            {
                HotelSearchResult[] hotelSearchResult = new HotelSearchResult[1];
                hotelSearchResult[0] = cheapest;                
            }

            HotelSearchResult[] hotelSearchResultBoth = new HotelSearchResult[2];
            hotelSearchResultBoth[0] = cheapest;
            hotelSearchResultBoth[1] = closest;


            HotelDetailsReq detailsRequest = new HotelDetailsReq();
            detailsRequest.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            detailsRequest.TraceId = "Trace";
            detailsRequest.AuthorizedBy = "User";
            detailsRequest.ReturnMediaLinks = true;

            HotelService.BillingPointOfSaleInfo billSaleInfo = new HotelService.BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = CommonUtility.GetConfigValue(ProjectConstants.APP);

            detailsRequest.BillingPointOfSaleInfo = billSaleInfo;
            detailsRequest.HotelProperty = cheapest.HotelProperty[0];// Cheapsest hotel selected

            HotelService.HotelDetailsModifiers hotelDetailsModifiers = new HotelService.HotelDetailsModifiers();
            hotelDetailsModifiers.HotelStay = GetHotelStay();
            hotelDetailsModifiers.NumberOfAdults = numberOfAdults;
            hotelDetailsModifiers.NumberOfRooms = numberOfRooms;
            hotelDetailsModifiers.RateRuleDetail = HotelService.typeRateRuleDetail.Complete;
            hotelDetailsModifiers.PermittedProviders = new HotelService.PermittedProviders()
            {
                Provider = new HotelService.Provider()
                {
                    Code = providerCode
                }
            };

            detailsRequest.HotelDetailsModifiers = hotelDetailsModifiers;


            HotelDetailsServicePortTypeClient detailsClient = new HotelDetailsServicePortTypeClient("HotelDetailsServicePort", WsdlService.HOTEL_ENDPOINT);            
            detailsClient.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            detailsClient.ClientCredentials.UserName.Password = Helper.ReturnPassword();

            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                detailsClient.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                detailsResponse = detailsClient.service(detailsRequest);                
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                detailsClient.Abort();
            }

            HotelRulesRsp hotelRulesResponse = HotelRules(cheapest.HotelProperty[0]);


            return detailsResponse;
        }

        private HotelRulesRsp HotelRules(HotelService.HotelProperty hotelProperty)
        {
            HotelRulesReq hotelRules = new HotelRulesReq();            
            
            HotelService.HotelRulesReqHotelRulesLookup rulesLookup = new HotelService.HotelRulesReqHotelRulesLookup();
            rulesLookup.Base = "";
            rulesLookup.RatePlanType = "";

            HotelService.BillingPointOfSaleInfo billSaleInfo = new HotelService.BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = CommonUtility.GetConfigValue(ProjectConstants.APP);

            hotelRules.BillingPointOfSaleInfo = billSaleInfo;
            
            HotelService.HotelProperty hotelProp = new HotelService.HotelProperty(); //HotelProperty is created here
            hotelProp.HotelChain = hotelProperty.HotelChain;
            hotelProp.HotelCode = hotelProperty.HotelCode;
            HotelService.HotelStay hotelStay = GetHotelStay();  //Hotel Stay will pass from this function
            rulesLookup.HotelStay = hotelStay;
            rulesLookup.HotelProperty = hotelProp;   //HotelProperty is added to RulesLookup

            hotelRules.Item = rulesLookup;

            HotelRulesServicePortTypeClient rulesClient = new HotelRulesServicePortTypeClient("HotelRulesServicePort", WsdlService.HOTEL_ENDPOINT);
            rulesClient.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            rulesClient.ClientCredentials.UserName.Password = Helper.ReturnPassword();

            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                rulesClient.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                rulesResponse = rulesClient.service(hotelRules);
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                rulesClient.Abort();
            }

            return rulesResponse;
        }


        #endregion
        /// <summary>
        /// booking the selected hotel
        /// </summary>
        /// <param name="hotelDetailsResponse"></param>
        /// <param name="hostToken"></param>
        /// <returns></returns>
        internal HotelCreateReservationRsp HotelBook(HotelDetailsRsp hotelDetailsResponse, HotelService.HostToken hostToken)
        {
            HotelCreateReservationReq hotelCreateReservationReq = new HotelCreateReservationReq();
            //If you want to create HotelBooking in the same UniversalRecord generated in AirCreateReservationRsp
            hotelCreateReservationReq.UniversalRecordLocatorCode = "Use Universal Record Locator Code generated in the AirBooking";
            UniversalService.BillingPointOfSaleInfo billSaleInfo = new UniversalService.BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = CommonUtility.GetConfigValue(ProjectConstants.APP);

            hotelCreateReservationReq.BillingPointOfSaleInfo = billSaleInfo;
            hotelCreateReservationReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            hotelCreateReservationReq.TraceId = "Trace";

            //create two booking traveler, phone number, email and address details
            UniversalService.BookingTraveler bookingTravelerOne = new UniversalService.BookingTraveler();
            bookingTravelerOne.Age = "47";
            bookingTravelerOne.DOB = Convert.ToDateTime("1967-11-23");
            bookingTravelerOne.Gender = "F";
            bookingTravelerOne.Nationality = "AU";
            bookingTravelerOne.TravelerType = "ADT";
            bookingTravelerOne.BookingTravelerName = new UniversalService.BookingTravelerName(){
                First = "Charlotte",
                Last = "Greene",
                Prefix = "MRS"
            };
            UniversalService.PhoneNumber phoneNumberOne= new UniversalService.PhoneNumber()
            {
                AreaCode = "08",
                CountryCode = "61",
                Location = "PER",
                Number = "40003000",
                Type = UniversalService.PhoneNumberType.Home
            };

            bookingTravelerOne.PhoneNumber = new UniversalService.PhoneNumber[1];
            bookingTravelerOne.PhoneNumber[0] = phoneNumberOne;



            UniversalService.Email emailOne = new UniversalService.Email()
            {
                EmailID = "test@travelport.com",
                Type = "Home"
            };

            bookingTravelerOne.Email = new UniversalService.Email[1];
            bookingTravelerOne.Email[0] = emailOne;

            UniversalService.typeStructuredAddress addressOne = new UniversalService.typeStructuredAddress()
            {
                AddressName = "Home",
                Street = new[] { "10 Charlie Street" },
                City = "Perth",
                State = new UniversalService.State()
                {
                    Value = "WA"
                },
                PostalCode = "6000",
                Country = "AU"
            };

            bookingTravelerOne.Address = new UniversalService.typeStructuredAddress[1];
            bookingTravelerOne.Address[0] = addressOne;



            UniversalService.BookingTraveler bookineTravelerTwo = new UniversalService.BookingTraveler();
            bookineTravelerTwo.Age = "50";
            bookineTravelerTwo.DOB = Convert.ToDateTime("1970-05-09");
            bookineTravelerTwo.Gender = "M";
            bookineTravelerTwo.Nationality = "AU";
            bookineTravelerTwo.TravelerType = "ADT";
            bookineTravelerTwo.BookingTravelerName = new UniversalService.BookingTravelerName()
            {
                First = "Eliott",
                Last = "Greene",
                Prefix = "MR"
            };
            UniversalService.PhoneNumber phoneNumberTwo = new UniversalService.PhoneNumber()
            {
                AreaCode = "08",
                CountryCode = "61",
                Location = "PER",
                Number = "40003000",
                Type = UniversalService.PhoneNumberType.Home
            };

            bookineTravelerTwo.PhoneNumber = new UniversalService.PhoneNumber[1];
            bookineTravelerTwo.PhoneNumber[0] = phoneNumberTwo;


            UniversalService.Email emailTwo = new UniversalService.Email()
            {
                EmailID = "test@travelport.com",
                Type = "Home"
            };

            bookineTravelerTwo.Email = new UniversalService.Email[1];
            bookineTravelerTwo.Email[0] = emailTwo;

            UniversalService.typeStructuredAddress addressTwo = new UniversalService.typeStructuredAddress()
            {
                AddressName = "Home",
                Street = new[] { "10 Charlie Street" },
                City = "Perth",
                State = new UniversalService.State()
                {
                    Value = "WA"
                },
                PostalCode = "6000",
                Country = "AU"
            };

            bookineTravelerTwo.Address = new UniversalService.typeStructuredAddress[1];
            bookineTravelerTwo.Address[0] = addressTwo;

            hotelCreateReservationReq.BookingTraveler = new UniversalService.BookingTraveler[2];
            hotelCreateReservationReq.BookingTraveler[0] = bookingTravelerOne;
            hotelCreateReservationReq.BookingTraveler[1] = bookineTravelerTwo;

            HotelService.GuaranteeInfo hotelGurrenteeInfo = null;
            RequestedHotelDetails reqHotelDetails = new RequestedHotelDetails();
            //select a hotel rate details and book the hotel using that one
            IEnumerator items = hotelDetailsResponse.Items.GetEnumerator();
            while(items.MoveNext()){
                var item = items.Current;
                reqHotelDetails = (RequestedHotelDetails)item;
            }
            
            if (reqHotelDetails.HotelRateDetail.Count() > 0)
            {
                IEnumerator<HotelService.HotelRateDetail> rateDetails = reqHotelDetails.HotelRateDetail.ToList().GetEnumerator();
                if (rateDetails.MoveNext())
                {
                    HotelService.HotelRateDetail rateDetail = rateDetails.Current;
                    List<ConsoleApplication2.UniversalService.HotelRateDetail> hotelRateDetails = new List<UniversalService.HotelRateDetail>();
                    hotelRateDetails.Add(new UniversalService.HotelRateDetail()
                    {
                        ApproximateBase = rateDetail.ApproximateBase ?? null,
                        ApproximateRateGuaranteed = rateDetail.ApproximateRateGuaranteed,
                        ApproximateRateGuaranteedSpecified = rateDetail.ApproximateRateGuaranteedSpecified,
                        ApproximateSurcharge = rateDetail.ApproximateSurcharge ?? null,
                        ApproximateTax = rateDetail.ApproximateTax ?? null,
                        ApproximateTotal = rateDetail.ApproximateTotal ?? null,
                        Base = rateDetail.Base ?? null,
                        RateCategory = rateDetail.RateCategory ?? null,
                        RatePlanType = rateDetail.RatePlanType ?? null,              
                        RateOfferId = rateDetail.RateOfferId ?? null,
                        Tax = rateDetail.Tax ?? null,
                        RateSupplier = rateDetail.RateSupplier ?? null,
                        Total = rateDetail.Total ?? null
                    });
                    hotelCreateReservationReq.HotelRateDetail = hotelRateDetails.ToArray();

                    hotelGurrenteeInfo = rateDetail.GuaranteeInfo;
                }
            }


            hotelCreateReservationReq.HotelProperty = new UniversalService.HotelProperty()
            {
                HotelChain = reqHotelDetails.HotelProperty.HotelChain ?? null,
                HotelCode = reqHotelDetails.HotelProperty.HotelCode ?? null,
                HotelLocation = reqHotelDetails.HotelProperty.HotelLocation ?? null,
                Name = reqHotelDetails.HotelProperty.Name ?? null,
                PropertyAddress = reqHotelDetails.HotelProperty.PropertyAddress ?? null,
            };

            hotelCreateReservationReq.HotelStay = new UniversalService.HotelStay()
            {
                CheckinDate = GetHotelStay().CheckinDate,
                CheckoutDate = GetHotelStay().CheckoutDate
            };

            //create payment info
            UniversalService.CreditCard ccInfo= new UniversalService.CreditCard()
            {
                BankCountryCode = GetCreditCardDetails().BankCountryCode,
                BankName = GetCreditCardDetails().BankName,
                Number = GetCreditCardDetails().Number,
                Type = GetCreditCardDetails().Type,
                ExpDate = GetCreditCardDetails().ExpDate,
                Name = bookingTravelerOne.BookingTravelerName.First + bookingTravelerOne.BookingTravelerName.Last,
                CVV = GetCreditCardDetails().CVV
            };


            //check if guarantee tyoe is Deopsit or not
            if (hotelGurrenteeInfo.GuaranteeType.ToString().CompareTo((HotelService.GuaranteeType.Deposit.ToString())) == 0)
            {
                hotelCreateReservationReq.Guarantee = new UniversalService.Guarantee()
                {
                    Type = UniversalService.GuaranteeType.Deposit,
                    Item = ccInfo
                };
            }
            else if (hotelGurrenteeInfo.GuaranteeType.ToString().CompareTo((HotelService.GuaranteeType.Guarantee.ToString())) == 0)
            {
                hotelCreateReservationReq.Guarantee = new UniversalService.Guarantee()
                {
                    Type = UniversalService.GuaranteeType.Guarantee,
                    Item = ccInfo
                };
            }

            hotelCreateReservationReq.GuestInformation = new UniversalService.GuestInformation()
            {
                NumberOfRooms = numberOfRooms,
                NumberOfAdults = new UniversalService.NumberOfAdults()
                {
                    Value = numberOfAdults.ToString()
                }
            };

            if (hostToken != null && hostToken.Value != null)
            {
                hotelCreateReservationReq.HostToken = new UniversalService.HostToken()
                {
                    Host = hostToken.Host,
                    Key = hostToken.Key ?? null,
                    Value = hostToken.Value
                };
            }



            HotelReservationServicePortTypeClient hotelBookclient = new HotelReservationServicePortTypeClient("HotelReservationServicePort", WsdlService.HOTEL_ENDPOINT);
            hotelBookclient.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            hotelBookclient.ClientCredentials.UserName.Password = Helper.ReturnPassword();

            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                hotelBookclient.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                hotelCreateReservationRsp = hotelBookclient.service(null, hotelCreateReservationReq);                
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                hotelBookclient.Abort();
            }

            return hotelCreateReservationRsp;
        }

        //create a test credit card
        private UniversalService.CreditCard GetCreditCardDetails()
        {
            UniversalService.CreditCard creditCard = new UniversalService.CreditCard();
            creditCard.BankCountryCode = "US";
            creditCard.BankName = "USB";
            creditCard.ExpDate = "2018-06";
            creditCard.Type = "VI";
            creditCard.Number = "4111111111111111";
            creditCard.CVV = "123";

            return creditCard;
        }
    }
}

