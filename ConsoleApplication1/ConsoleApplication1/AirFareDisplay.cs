using ConsoleApplication1.AirService;
using ConsoleApplication1.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class AirFareDisplay
    {
        internal AirFareDisplayRsp GetAirFareDisplayDetails(List<typeBaseAirSegment> pricingSegments)
        {
            AirFareDisplayReq fareDisplayReq = new AirFareDisplayReq();
            AirFareDisplayRsp fareDisplayRsp;

            fareDisplayReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

            string depart = "";

            IEnumerator<typeBaseAirSegment> segments = pricingSegments.GetEnumerator();
            if (segments.MoveNext()) 
            {
                typeBaseAirSegment seg = segments.Current;
                if (seg.Origin != null)
                {
                    fareDisplayReq.Origin = seg.Origin;
                }
                if (seg.Destination != null)
                {
                    fareDisplayReq.Destination = seg.Destination;
                }

                if (seg.DepartureTime != null)
                {
                    depart = seg.DepartureTime;
                }

                fareDisplayReq.ProviderCode = "1G";
            }

            fareDisplayReq.BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
            {
                OriginApplication = "UAPI"
            };

            fareDisplayReq.PassengerType = AddPassengerType();

            fareDisplayReq.AirFareDisplayModifiers = AddAirFareDisplayModifiers(depart);


            AirFareDisplayPortTypeClient client = new AirFareDisplayPortTypeClient("AirFareDisplayPort", WsdlService.AIR_ENDPOINT);

            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                fareDisplayRsp = client.service(fareDisplayReq);

                return fareDisplayRsp;
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                client.Abort();
                return null;
            }

        }

        private AirFareDisplayModifiers AddAirFareDisplayModifiers(string departDt)
        {
            AirFareDisplayModifiers displayMod = new AirFareDisplayModifiers();
            displayMod.MaxResponses = "200";
            displayMod.DepartureDate = DateTime.ParseExact(departDt.Substring(0,10),"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
            displayMod.BaseFareOnly = false;
            displayMod.UnrestrictedFaresOnly = false;
            displayMod.CurrencyType = "USD";

            return displayMod;
        }

        private typePassengerType[] AddPassengerType()
        {
            typePassengerType pass = new typePassengerType() 
            { 
                Code = "ADT",
                PricePTCOnly = true
            };

            List<typePassengerType> passList = new List<typePassengerType>();
            passList.Add(pass);

            return passList.ToArray();
        }

        internal AirFareRulesRsp GetAirFareRules(AirFareDisplayRsp fareDisplayRsp, AirReservationLocatorCode reservationLocatorCode)
        {
            AirFareRulesReq rulesReq = new AirFareRulesReq();
            AirFareRulesRsp rulesRsp = null;

            if (fareDisplayRsp != null)
            {
                rulesReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

                rulesReq.FareRuleType = typeFareRuleType.@long;//We cna use @short to get short fareRules

                rulesReq.BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
                {
                    OriginApplication = "UAPI"
                };

                IEnumerator fareDisplay = fareDisplayRsp.FareDisplay.GetEnumerator();
                if (fareDisplay.MoveNext())//There can be multiple fareDiplay in FareDisplayRsp, we are taking the first one here
                {
                    FareDisplay fare = (FareDisplay)fareDisplay.Current;
                    if (fare.AirFareDisplayRuleKey != null)
                    {
                        List<Object> items = new List<object>();
                        items.Add(fare.AirFareDisplayRuleKey);

                        rulesReq.Items = items.ToArray();
                    }
                }

            }
            else if (reservationLocatorCode != null)
            {
                rulesReq.TargetBranch = CommonUtility.GetConfigValue(ProjectConstants.G_TARGET_BRANCH);

                rulesReq.FareRuleType = typeFareRuleType.@long;

                rulesReq.BillingPointOfSaleInfo = new BillingPointOfSaleInfo()
                {
                    OriginApplication = "UAPI"
                };

                AirReservationLocatorCode locatorCode = reservationLocatorCode;
                List<Object> codes = new List<object>();
                codes.Add(locatorCode);

                rulesReq.Items = codes.ToArray();
            }

            AirFareRulesPortTypeClient client = new AirFareRulesPortTypeClient("AirFareRulesPort", WsdlService.AIR_ENDPOINT);

            client.ClientCredentials.UserName.UserName = Helper.RetrunUsername();
            client.ClientCredentials.UserName.Password = Helper.ReturnPassword();
            try
            {
                var httpHeaders = Helper.ReturnHttpHeader();
                client.Endpoint.EndpointBehaviors.Add(new HttpHeadersEndpointBehavior(httpHeaders));

                rulesRsp = client.service(rulesReq);

                //return rulesRsp;
            }
            catch (Exception se)
            {
                Console.WriteLine("Error : " + se.Message);
                client.Abort();
                return null;
            }

            return rulesRsp;
        }
    }
}
