using ConsoleApplication1.UniversalService;
using ConsoleApplication1.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class AirBookTest
    {
        internal AirCreateReservationRsp AirBook(AirService.AirPricingSolution lowestPrice, AirService.AirItinerary airItinerary)
        {
            AirCreateReservationReq reservationReq = new AirCreateReservationReq();
            AirCreateReservationRsp reservationRsp;

            BillingPointOfSaleInfo billSaleInfo = new BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = "UAPI";

            reservationReq.BillingPointOfSaleInfo = billSaleInfo;

            reservationReq.ContinuityCheckOverride = new ContinuityCheckOverride()
            {
                Key = "1T",
                Value = "true"
            };

            reservationReq.BookingTraveler = AddBookingTraveler(airItinerary);

            reservationReq.FormOfPayment = AddFormOfPayment();

            reservationReq.AirPricingSolution = AddAirPriceSolution(lowestPrice, airItinerary);

            List<ActionStatus> statusList = new List<ActionStatus>();

            ActionStatus actionStatus = new ActionStatus()
            {
                ProviderCode = "1G",
                Type = ActionStatusType.TAW,
                TicketDate = reservationReq.AirPricingSolution.AirPricingInfo[0].LatestTicketingTime
            };

            statusList.Add(actionStatus);

            reservationReq.ActionStatus = statusList.ToArray();



            reservationReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);
            reservationReq.RetainReservation = typeRetainReservation.Both;

            AirCreateReservationPortTypeClient client = new AirCreateReservationPortTypeClient("AirCreateReservationPort", WsdlService.AIR_ENDPOINT);
            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                /*SupportedVersions versions = new SupportedVersions();

                versions.airVersion = "air_v29_0";
                versions.urVersion = "universal_v29_0";*/                

                reservationRsp = client.service(null, reservationReq);
                //Console.WriteLine(lowFareSearchRsp.AirSegmentList.Count());

                return reservationRsp;
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                client.Abort();
                return null;
            }

        }

        private AirPricingSolution AddAirPriceSolution(AirService.AirPricingSolution lowestPrice, AirService.AirItinerary airItinerary)
        {
            AirPricingSolution finalPrice = new AirPricingSolution()
            {
                Key = lowestPrice.Key,
                TotalPrice = lowestPrice.TotalPrice,
                BasePrice = lowestPrice.BasePrice,
                ApproximateTotalPrice = lowestPrice.ApproximateTotalPrice,
                ApproximateBasePrice = lowestPrice.ApproximateBasePrice,
                Taxes = lowestPrice.Taxes,
                ApproximateTaxes = lowestPrice.ApproximateTaxes,
                QuoteDate = lowestPrice.QuoteDate
            };
            List<typeBaseAirSegment> finalSegments = new List<typeBaseAirSegment>();
            List<AirPricingInfo> finalPriceInfo =new List<AirPricingInfo>();

            
            
            foreach (var segmentRef in lowestPrice.AirSegmentRef)
            {
                foreach (var segment in airItinerary.AirSegment)
                {
                    if (segmentRef.Key.CompareTo(segment.Key) == 0)
                    {
                        typeBaseAirSegment univSeg = new typeBaseAirSegment()
                        {
                            ArrivalTime = segment.ArrivalTime,
                            AvailabilityDisplayType = segment.AvailabilityDisplayType,
                            AvailabilitySource = segment.AvailabilitySource,
                            Carrier = segment.Carrier,
                            ChangeOfPlane = segment.ChangeOfPlane,
                            ClassOfService = segment.ClassOfService,
                            DepartureTime = segment.DepartureTime,
                            Destination = segment.Destination,
                            Distance = segment.Distance,
                            Equipment = segment.Equipment,
                            FlightNumber = segment.FlightNumber,
                            FlightTime = segment.FlightTime,
                            Group = segment.Group,
                            Key = segment.Key,
                            LinkAvailability = segment.LinkAvailability,
                            OptionalServicesIndicator = segment.OptionalServicesIndicator,
                            Origin = segment.Origin,
                            ParticipantLevel = segment.ParticipantLevel,
                            PolledAvailabilityOption = segment.PolledAvailabilityOption,
                            ProviderCode = segment.ProviderCode,
                            TravelTime = segment.TravelTime,
                        };

                        finalSegments.Add(univSeg);
                        break;
                    }
                }
            }

            foreach (var priceInfo in lowestPrice.AirPricingInfo)
            {
                AirPricingInfo info = new AirPricingInfo()
                {
                    ApproximateBasePrice = priceInfo.ApproximateBasePrice,
                    ApproximateTotalPrice = priceInfo.ApproximateTotalPrice,
                    BasePrice = priceInfo.BasePrice,
                    ETicketability = (typeEticketability)priceInfo.ETicketability,
                    IncludesVAT = priceInfo.IncludesVAT,
                    Key = priceInfo.Key,
                    LatestTicketingTime = priceInfo.LatestTicketingTime,
                    //PlatingCarrier = priceInfo.PlatingCarrier, Optional but might be required for some carriers
                    PricingMethod = (typePricingMethod)priceInfo.PricingMethod,
                    ProviderCode = priceInfo.ProviderCode,
                    Taxes = priceInfo.Taxes,
                    TotalPrice = priceInfo.TotalPrice,
                };

                List<FareInfo> fareInfoList = new List<FareInfo>();

                List<ManualFareAdjustment> fareAdjustmentList = new List<ManualFareAdjustment>();

                ManualFareAdjustment adjustment = new ManualFareAdjustment()
                {
                    AdjustmentType = typeAdjustmentType.Amount,
                    AppliedOn = typeAdjustmentTarget.Base,
                    Value = +40,
                    PassengerRef = "gr8AVWGCR064r57Jt0+8bA=="
                };

                fareAdjustmentList.Add(adjustment);

                info.AirPricingModifiers = new AirPricingModifiers()
                {
                    ManualFareAdjustment = fareAdjustmentList.ToArray()
                };

                foreach (var fareInfo in priceInfo.FareInfo)
                {
                    FareInfo createInfo = new FareInfo()
                    {
                        Amount = fareInfo.Amount,
                        DepartureDate = fareInfo.DepartureDate,
                        Destination = fareInfo.Destination,
                        EffectiveDate = fareInfo.EffectiveDate,
                        FareBasis = fareInfo.FareBasis,
                        Key = fareInfo.Key,
                        NotValidAfter = fareInfo.NotValidAfter,
                        NotValidBefore = fareInfo.NotValidBefore,
                        Origin = fareInfo.Origin,
                        PassengerTypeCode = fareInfo.PassengerTypeCode,
                        PrivateFare = (typePrivateFare)fareInfo.PrivateFare,
                        PseudoCityCode = fareInfo.PseudoCityCode,
                        FareRuleKey = new FareRuleKey()
                        {
                            FareInfoRef = fareInfo.FareRuleKey.FareInfoRef,
                            ProviderCode = fareInfo.FareRuleKey.ProviderCode,
                            Value = fareInfo.FareRuleKey.Value
                        }

                    };

                    List<Endorsement> endorsementList = new List<Endorsement>();

                    if (fareInfo.Endorsement != null)
                    {
                        foreach (var endorse in fareInfo.Endorsement)
                        {
                            Endorsement createEndorse = new Endorsement()
                            {
                                Value = endorse.Value
                            };

                            endorsementList.Add(createEndorse);
                        }

                        createInfo.Endorsement = endorsementList.ToArray();
                    }

                    fareInfoList.Add(createInfo);                    
                }

                info.FareInfo = fareInfoList.ToArray();

                List<BookingInfo> bInfo = new List<BookingInfo>();

                foreach (var bookingInfo in priceInfo.BookingInfo)
                {
                    BookingInfo createBookingInfo = new BookingInfo()
                    {
                        BookingCode = bookingInfo.BookingCode,
                        CabinClass = bookingInfo.CabinClass,
                        FareInfoRef = bookingInfo.FareInfoRef,
                        SegmentRef = bookingInfo.SegmentRef
                    };

                    bInfo.Add(createBookingInfo);
                }

                info.BookingInfo = bInfo.ToArray();

                List<typeTaxInfo> taxes = new List<typeTaxInfo>();

                foreach (var tax in priceInfo.TaxInfo)
                {
                    typeTaxInfo createTaxInfo = new typeTaxInfo()
                    {
                        Amount = tax.Amount,
                        Category = tax.Category,
                        Key = tax.Key
                    };

                    taxes.Add(createTaxInfo);
                }

                info.TaxInfo = taxes.ToArray();

                info.FareCalc = priceInfo.FareCalc;

                List<PassengerType> passengers = new List<PassengerType>();

                /*foreach (var pass in priceInfo.PassengerType)
                {
                    PassengerType passType = new PassengerType() 
                    { 
                        BookingTravelerRef = pass.BookingTravelerRef,
                        Code = pass.BookingTravelerRef
                    };

                    passengers.Add(passType);
                }*/

                passengers.Add(new PassengerType()
                {
                    Code = "ADT",
                    BookingTravelerRef = "gr8AVWGCR064r57Jt0+8bA=="
                });

                info.PassengerType = passengers.ToArray();

                if (priceInfo.ChangePenalty != null)
                {
                    info.ChangePenalty = new typeFarePenalty()
                    {
                        Amount = priceInfo.ChangePenalty.Amount
                    };
                }

                List<BaggageAllowanceInfo> baggageInfoList = new List<BaggageAllowanceInfo>();

                foreach (var allowanceInfo in priceInfo.BaggageAllowances.BaggageAllowanceInfo)
                {
                    BaggageAllowanceInfo createBaggageInfo = new BaggageAllowanceInfo()
                    {
                        Carrier = allowanceInfo.Carrier,
                        Destination = allowanceInfo.Destination,
                        Origin = allowanceInfo.Origin,
                        TravelerType = allowanceInfo.TravelerType
                    };

                    List<URLInfo> urlInfoList = new List<URLInfo>();

                    foreach (var url in allowanceInfo.URLInfo)
                    {
                        URLInfo urlInfo = new URLInfo()
                        {
                            URL = url.URL
                        };

                        urlInfoList.Add(urlInfo);
                    }


                    createBaggageInfo.URLInfo = urlInfoList.ToArray();

                    List<ConsoleApplication1.UniversalService.TextInfo> textInfoList = new List<UniversalService.TextInfo>();

                    foreach (var textData in allowanceInfo.TextInfo)
                    {
                        ConsoleApplication1.UniversalService.TextInfo textInfo = new UniversalService.TextInfo()
                        {
                            Text = textData.Text
                        };

                        textInfoList.Add(textInfo);
                    }

                    createBaggageInfo.TextInfo = textInfoList.ToArray();

                    List<BagDetails> bagDetailsList = new List<BagDetails>();

                    foreach (var bagDetails in allowanceInfo.BagDetails)
                    {
                        BagDetails bag = new BagDetails()
                        {
                            ApplicableBags = bagDetails.ApplicableBags,
                            ApproximateBasePrice = bagDetails.ApproximateBasePrice,
                            ApproximateTotalPrice = bagDetails.ApproximateTotalPrice,
                            BasePrice = bagDetails.BasePrice,
                            TotalPrice = bagDetails.TotalPrice,                        
                        };

                        List<BaggageRestriction> bagRestictionList = new List<BaggageRestriction>();
                        foreach (var restriction in bagDetails.BaggageRestriction)
                        {
                            List<ConsoleApplication1.UniversalService.TextInfo> restrictionTextList = new List<UniversalService.TextInfo>();
                            foreach (var bagResTextInfo in restriction.TextInfo)
                            {
                                ConsoleApplication1.UniversalService.TextInfo resText = new UniversalService.TextInfo()
                                {
                                    Text = bagResTextInfo.Text
                                };

                                restrictionTextList.Add(resText);
                            }

                            BaggageRestriction bagRes = new BaggageRestriction()
                            {
                                TextInfo = restrictionTextList.ToArray()
                            };
                            
                            bagRestictionList.Add(bagRes);
                        }

                        bag.BaggageRestriction = bagRestictionList.ToArray();
                        bagDetailsList.Add(bag);
                    }

                    createBaggageInfo.BagDetails = bagDetailsList.ToArray();

                    baggageInfoList.Add(createBaggageInfo);
                    
                }


                List<CarryOnAllowanceInfo> carryOnAllowanceList = new List<CarryOnAllowanceInfo>();

                foreach (var carryOnBag in priceInfo.BaggageAllowances.CarryOnAllowanceInfo)
                {
                    CarryOnAllowanceInfo carryOn = new CarryOnAllowanceInfo()
                    {
                        Carrier = carryOnBag.Carrier,
                        Destination = carryOnBag.Destination,
                        Origin = carryOnBag.Origin
                    };

                    carryOnAllowanceList.Add(carryOn);
                }

                List<BaseBaggageAllowanceInfo> embargoInfoList = new List<BaseBaggageAllowanceInfo>();

                if(priceInfo.BaggageAllowances.EmbargoInfo != null)
                {
                    foreach(AirService.BaseBaggageAllowanceInfo embargoInfo in priceInfo.BaggageAllowances.EmbargoInfo)
                    {
                        BaseBaggageAllowanceInfo embargo = new BaseBaggageAllowanceInfo()
                        {
                            Carrier = embargoInfo.Carrier,
                            Destination = embargoInfo.Destination,
                            Origin = embargoInfo.Origin
                        };

                        List<URLInfo> embargoURLList = new List<URLInfo>();
                        foreach(var embargoUrl in embargoInfo.URLInfo){
                            URLInfo url = new URLInfo()
                            {
                                URL = embargoUrl.URL,
                                Text = embargoUrl.Text
                            };

                            embargoURLList.Add(url);
                        }

                        embargo.URLInfo = embargoURLList.ToArray();

                        List<ConsoleApplication1.UniversalService.TextInfo> embargoTextList = new List<UniversalService.TextInfo>();
                        foreach(var embargoText in embargoInfo.TextInfo){
                            ConsoleApplication1.UniversalService.TextInfo text = new UniversalService.TextInfo()
                            {
                                Text = embargoText.Text
                            };

                            embargoTextList.Add(text);
                        }

                        embargo.TextInfo = embargoTextList.ToArray();

                        embargoInfoList.Add(embargo);
                    }
                }


                info.BaggageAllowances = new BaggageAllowances()
                {
                    BaggageAllowanceInfo = baggageInfoList.ToArray(),
                    CarryOnAllowanceInfo = carryOnAllowanceList.ToArray(),
                    EmbargoInfo = embargoInfoList.ToArray()
                };


                finalPriceInfo.Add(info);
                break;

            }

            finalPrice.AirPricingInfo = finalPriceInfo.ToArray();
            finalPrice.AirSegment = finalSegments.ToArray();


            return finalPrice;


        }


        private FormOfPayment[] AddFormOfPayment()
        {
            List<FormOfPayment> payments = new List<FormOfPayment>();

            FormOfPayment fop = new FormOfPayment();
            fop.Key = "jwt2mcK1Qp27I2xfpcCtAw==";//Key can be different
            fop.Type = "Credit";

            CreditCard cc = new CreditCard()
            {
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
                },
                ExpDate = DateTime.Now.AddYears(2).ToString("yyyy-MM"),
                Key = "GAJOYrVu4hGShsrlYIhwmw==",
                Number = "4111111111111111",
                BankCountryCode = "US",
                CVV = "123",
                Type = "VI"
            };

            fop.Item = cc;

            payments.Add(fop);

            return payments.ToArray();

        }

        private BookingTraveler[] AddBookingTraveler(AirService.AirItinerary airItinerary)
        {

            List<BookingTraveler> travelers = new List<BookingTraveler>();


            BookingTraveler traveler = new BookingTraveler();
            traveler.DOB = DateTime.Now.AddYears(-28);
            traveler.Gender = "M";
            traveler.TravelerType = "ADT";
            traveler.Key = "gr8AVWGCR064r57Jt0+8bA==";
            traveler.Nationality = "US";

            traveler.BookingTravelerName = new BookingTravelerName()
            {
                First = "Jack",
                Last = "Smith",
                Prefix = "Mr"
            };

            DeliveryInfoShippingAddress shipping = new DeliveryInfoShippingAddress()
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
            };

            List<DeliveryInfo> deliveryInfoList = new List<DeliveryInfo>();
            DeliveryInfo deliveryInfo = new DeliveryInfo()
            {
                ShippingAddress = shipping
            };

            deliveryInfoList.Add(deliveryInfo);

            traveler.DeliveryInfo = deliveryInfoList.ToArray();

            List<PhoneNumber> phoneList = new List<PhoneNumber>();

            PhoneNumber phoneNum = new PhoneNumber()
            {
                AreaCode = "303",
                CountryCode = "1",
                Number = "3333333",
                Location = "DEN"
            };

            phoneList.Add(phoneNum);

            traveler.PhoneNumber = phoneList.ToArray();

            List<Email> emailList = new List<Email>();

            Email email = new Email()
            {
                EmailID = "test@travelport.com",
                Type = "Home"
            };

            emailList.Add(email);

            traveler.Email = emailList.ToArray();


            List<SSR> ssrList = new List<SSR>();
            //This part is optional but required for some airlines like UA etc.
            if (airItinerary.AirSegment != null)
            {
                IEnumerator segments = airItinerary.AirSegment.GetEnumerator();
                while (segments.MoveNext())
                {
                    AirService.typeBaseAirSegment seg = (AirService.typeBaseAirSegment)segments.Current;
                    SSR ssr = new SSR()
                    {
                        Carrier = seg.Carrier,
                        SegmentRef = seg.Key,
                        Status = "HK",
                        Type = "DOCS",
                        FreeText = "P/" + traveler.Nationality + "/F1234567/" + traveler.Nationality + "/"
                                    + traveler.DOB.ToString("ddMMMyy") + "/"
                                    + traveler.Gender + "/" + DateTime.Now.AddYears(2).ToString("ddMMMyy") + "/" + traveler.BookingTravelerName.Last
                                    + "/" + traveler.BookingTravelerName.First

                    };

                    ssrList.Add(ssr);

                }
            }

            traveler.SSR = ssrList.ToArray();

            List<typeStructuredAddress> addressList = new List<typeStructuredAddress>();

            typeStructuredAddress address = new typeStructuredAddress()
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
            };

            addressList.Add(address);

            traveler.Address = addressList.ToArray();

            travelers.Add(traveler);

            return travelers.ToArray();
        }

                    

    }
}
