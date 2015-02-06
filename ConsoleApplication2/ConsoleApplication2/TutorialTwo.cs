using ConsoleApplication2.HotelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class TutorialTwo
    {
        static void Main(string[] args)
        {
            HotelTutorial hotel = new HotelTutorial();
            BaseHotelSearchRsp hotelResponse = hotel.HotelAvailabilty();// Get Hotel Availability
            HostToken hostToken = new HostToken();

            if (hotelResponse.HotelSearchResult.Count() > 0)
            {
                HotelDetailsRsp hotelDetailsResponse = hotel.HotelDetails(hotelResponse);// Get HotelDetails for the cheapest available hotel
                if (hotelResponse.HostToken != null)
                {
                    hostToken = hotelResponse.HostToken;
                }

                if (hotelDetailsResponse.Item != null)
                {
                    hotel.HotelBook(hotelDetailsResponse, hostToken); //send the selected hotelDetails and rates to book the hotel
                }
            }

        }
    }
}
