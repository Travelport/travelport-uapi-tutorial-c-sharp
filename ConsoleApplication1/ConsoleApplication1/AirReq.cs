using ConsoleApplication1.AirService;
using ConsoleApplication1.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    class AirReq
    {
        public static void AddPointOfSale(BaseSearchReq req, string appName)
        {
            BillingPointOfSaleInfo billSaleInfo = new BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = appName;
            req.BillingPointOfSaleInfo = billSaleInfo;
        }

        public static void AddPointOfSale(BaseCoreReq req, string appName)
        {
            BillingPointOfSaleInfo billSaleInfo = new BillingPointOfSaleInfo();
            billSaleInfo.OriginApplication = appName;
            req.BillingPointOfSaleInfo = billSaleInfo;
        }


        /**
         * Add the search passengers to the request.  We only add ADT (adult)
         * passengers and this only works for LowFareSearchReq objects.
         * @param request the req to add the passenger parameter to
         * @param n number of adults to put in the requset
         */
        public static void AddAdultPassengers(BaseLowFareSearchReq request, int n)
        {
            List<SearchPassenger> passList = new List<SearchPassenger>();
            for (int i = 0; i < n; ++i)
            {
                SearchPassenger adult = new SearchPassenger();
                adult.Code = "ADT";
                passList.Add(adult);
            }
            request.SearchPassenger = passList.ToArray();
        }


        public SearchAirLeg CreateAirLeg(string originAirportCode, string destAirportCode)
        {
            typeSearchLocation originLoc = new typeSearchLocation();
            typeSearchLocation destLoc = new typeSearchLocation();

            // airport objects are just wrappers for their codes
            Airport origin = new Airport(), dest = new Airport();
            origin.Code = originAirportCode;
            dest.Code = destAirportCode;

            // search locations can be things other than airports but we are using
            // the airport version...
            originLoc.Item = origin;
            destLoc.Item = dest;

            return CreateLeg(originLoc, destLoc);
        }

        private SearchAirLeg CreateLeg(typeSearchLocation originLoc, typeSearchLocation destLoc)
        {
            SearchAirLeg leg = new SearchAirLeg();
            leg.SearchOrigin.SetValue(originLoc.Item, 0);
            leg.SearchDestination.SetValue(destLoc.Item, 0);

            return leg;
        }


        /**
         * Make a search location based on a city or airport code (city is 
         * preferred to airport in a conflict) and set the search radius to
         * 50mi.
        */
        public static typeSearchLocation CreateLocationNear(String cityOrAirportCode)
        {
            typeSearchLocation result = new typeSearchLocation();

            //city
            CityOrAirport place = new CityOrAirport();
            place.Code = cityOrAirportCode;
            place.PreferCity = true;
            result.Item = place;

            //distance
            Distance dist = new Distance();
            dist.Units = DistanceUnits.MI;
            dist.Value = string.Format("50");
            result.Distance = dist;

            return result;
        }


        /**
         * Mmodify a search leg to use economy class of service as preferred.
         * 
         * @param outbound the leg to modify
         */
        public static void AddEconomyPreferred(SearchAirLeg outbound)
        {
            AirLegModifiers modifiers = new AirLegModifiers();            
            PreferredCabins cabins = new PreferredCabins();
            CabinClass cabinClass = new CabinClass();
            cabinClass.Type = "Economy";
            cabins.CabinClass = cabinClass;

            modifiers.PreferredCabins = cabins;
            outbound.AirLegModifiers = modifiers;
        }


        /**
         * Modify a search leg based on a departure date
         * 
         * @param outbound the leg to modify
         * @param departureDate the departure date in YYYY-MM-dd
         */
        public static void AddDepartureDate(SearchAirLeg outbound, String departureDate)
        {
            // flexible time spec is flexible in that it allows you to say
            // days before or days after
            typeFlexibleTimeSpec noFlex = new typeFlexibleTimeSpec();
            noFlex.PreferredTime = departureDate;

            List<typeFlexibleTimeSpec> flexList = new List<typeFlexibleTimeSpec>();
            flexList.Add(noFlex);
            outbound.Items = flexList.ToArray();
            
        }


         /**
	     * Search modifiers to create, usually a GDS code plus optionally 
	     * RCH (Helper.RAIL_PROVIDER) or ACH (Helper.LOW_COST_PROVIDER).
	     * 
	     * @param providerCode  one or more provider codes (zero will not work!)
	     * @return the modifiers object
	     */
	    public static AirSearchModifiers CreateModifiersWithProviders(String[] providerCode) {
		    AirSearchModifiers modifiers = new AirSearchModifiers();
            List<Provider> providers = new List<Provider>();

		    for (int i=0; i<providerCode.Length;++i) {
			    Provider p = new Provider();
			    // set the code for the provider
			    p.Code  = providerCode[i];
			    // can be many providers, but we just use one
			    providers.Add(p);
		    }
		    modifiers.PreferredProviders  = providers.ToArray();
		    return modifiers;
	    }

        public static SearchAirLeg CreateSearchLeg(String originAirportCode, String destAirportCode)
        {
            // TODO Auto-generated method stub
            typeSearchLocation originLoc = new typeSearchLocation();
            typeSearchLocation destLoc = new typeSearchLocation();

            // airport objects are just wrappers for their codes
            Airport origin = new Airport(), dest = new Airport();
            origin.Code = originAirportCode;
            dest.Code = destAirportCode;

            // search locations can be things other than airports but we are using
            // the airport version...
            originLoc.Item = origin;
            destLoc.Item = dest;

            return CreateSearchLeg(originLoc, destLoc);
        }

        private static SearchAirLeg CreateSearchLeg(typeSearchLocation originLoc,
                typeSearchLocation destLoc)
        {
            SearchAirLeg leg = new SearchAirLeg();

            leg.SearchOrigin = new typeSearchLocation[1];
            leg.SearchDestination = new typeSearchLocation[1];
            leg.SearchOrigin.SetValue(originLoc, 0);
            leg.SearchDestination.SetValue(destLoc, 0);

            return leg;
        }

        public static void AddSearchDepartureDate(SearchAirLeg outbound,
                String departureDate)
        {
            // flexible time spec is flexible in that it allows you to say
            // days before or days after
            typeFlexibleTimeSpec noFlex = new typeFlexibleTimeSpec();
            noFlex.PreferredTime = departureDate;

            List<typeFlexibleTimeSpec> flexList = new List<typeFlexibleTimeSpec>();
            flexList.Add(noFlex);
            outbound.Items = flexList.ToArray();
        }

        public static void AddSearchEconomyPreferred(SearchAirLeg outbound)
        {
            AirLegModifiers modifiers = new AirLegModifiers();
            PreferredCabins cabins = new PreferredCabins();
            CabinClass cabinClass = new CabinClass();
            cabinClass.Type = "Economy";
            cabins.CabinClass = cabinClass;

            modifiers.PreferredCabins = cabins;
            outbound.AirLegModifiers = modifiers;
        }


        internal static AirPricingModifiers AddAirPriceModifiers(typeAdjustmentType adjustmentType, int amount)
        {
            AirPricingModifiers priceModifiers = new AirPricingModifiers();
            List<ManualFareAdjustment> fareList = new List<ManualFareAdjustment>();

            ManualFareAdjustment fareAdjustment = new ManualFareAdjustment();

            if (typeAdjustmentType.Amount.CompareTo(adjustmentType) == 0)
            {
                fareAdjustment.AdjustmentType = typeAdjustmentType.Amount;
            }
            else if (typeAdjustmentType.Percentage.CompareTo(adjustmentType) == 0)
            {
                fareAdjustment.AdjustmentType = typeAdjustmentType.Percentage;
            }

            fareAdjustment.PassengerRef = "1";
            fareAdjustment.AppliedOn = typeAdjustmentTarget.Base;
            fareAdjustment.Value = amount;

            fareList.Add(fareAdjustment);

            priceModifiers.ManualFareAdjustment = fareList.ToArray();

            return priceModifiers;
        }

        internal static SearchPassenger[] AddSearchPassenger()
        {
            List<SearchPassenger> passengers = new List<SearchPassenger>();

            SearchPassenger passenger = new SearchPassenger();
            passenger.Code = "ADT";
            passenger.BookingTravelerRef = "gr8AVWGCR064r57Jt0+8bA==";

            passengers.Add(passenger);

            return passengers.ToArray();
        }

        internal static AirPriceRsp AirPrice(List<typeBaseAirSegment> pricingSegments)
        {
            AirPriceReq priceReq = new AirPriceReq();
            AirPriceRsp priceRsp;

            AddPointOfSale(priceReq, "UAPI");

            AirItinerary itinerary = new AirItinerary();

            List<typeBaseAirSegment> itinerarySegments = new List<typeBaseAirSegment>();

            IEnumerator airSegments = pricingSegments.GetEnumerator();
            while (airSegments.MoveNext())
            {
                typeBaseAirSegment seg = (typeBaseAirSegment)airSegments.Current;
                seg.ProviderCode = "1G";
                seg.FlightDetailsRef = null;

                itinerarySegments.Add(seg);
            }

            itinerary.AirSegment = itinerarySegments.ToArray();

            priceReq.AirItinerary = itinerary;

            priceReq.SearchPassenger = AddSearchPassenger();

            priceReq.AirPricingModifiers = new AirPricingModifiers()
            {
                PlatingCarrier = priceReq.AirItinerary.AirSegment[0].Carrier
            };

            List<AirPricingCommand> pricingCommands = new List<AirPricingCommand>();

            AirPricingCommand command = new AirPricingCommand()
            {
                CabinClass = "Economy"//You can use Economy,PremiumEconomy,Business etc.
            };

            pricingCommands.Add(command);

            priceReq.AirPricingCommand = pricingCommands.ToArray();
            

            priceReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

            AirPricePortTypeClient client = new AirPricePortTypeClient("AirPricePort", WsdlService.AIR_ENDPOINT);

            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                priceRsp = client.service(priceReq);                

                return priceRsp;
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
