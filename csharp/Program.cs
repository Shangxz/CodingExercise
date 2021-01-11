using ParagonCodingExercise.Airports;
using ParagonCodingExercise.Events;
using System.Collections.Generic;
using System;

namespace ParagonCodingExercise
{
    class Program
    {
        private static string AirportsFilePath = @".\Resources\airports.json";

        // Location of ADS-B events
        private static string AdsbEventsFilePath = @".\Resources\events.txt";

        // Write generated flights here
        private static string OutputFilePath = @".\Resources\flights.txt";
        static void Main(string[] args)
        {
            Execute();
            
            Console.ReadKey();
        }

        private static void Execute()
        {
            // Load the airports
            AirportCollection airports = AirportCollection.LoadFromFile(AirportsFilePath);

            var fileLines = System.IO.File.ReadAllLines(AdsbEventsFilePath);

            Dictionary<string, List<AdsbEvent>> airplanes = new Dictionary<string, List<AdsbEvent>>();

            foreach (var line in fileLines){
                
                string airplaneId = Events.AdsbEvent.FromJson(line).Identifier;
                if (airplanes.ContainsKey(airplaneId)){
                    airplanes[airplaneId].Add(Events.AdsbEvent.FromJson(line));
                }
                else{
                    List<AdsbEvent> tmpEventList = new List<AdsbEvent>() {Events.AdsbEvent.FromJson(line)};
                    airplanes.Add(Events.AdsbEvent.FromJson(line).Identifier, tmpEventList);
                }
            }

            System.IO.StreamWriter outFile = new System.IO.StreamWriter(OutputFilePath);

            foreach (var key in airplanes.Keys){
                System.Console.WriteLine(key);
                DateTime prevTime = DateTime.Parse("1/01/2020 12:00:00 AM");
                GeoCoordinate prevAirportLocation = null;
                Airport prevAirport = null;
                Flight flight = new Flight();
                flight.AircraftIdentifier = key;

                foreach (var adsbEventEntry in airplanes[key]){
                    // create a geolocation based on airplane adsb event coords
                    string latLong = adsbEventEntry.Latitude.ToString() + "," + adsbEventEntry.Longitude.ToString();
                    GeoCoordinate airplaneLocation = GeoCoordinate.FromLatitudeAndLongitudeString(latLong);
                    // get closest airport based on current airplane location
                    Airport tempAirport = airports.GetClosestAirport(airplaneLocation);
                    // create airport geolocation
                    string airportLatLong = tempAirport.Latitude.ToString() + "," + tempAirport.Longitude.ToString();
                    GeoCoordinate airportLocation = GeoCoordinate.FromLatitudeAndLongitudeString(airportLatLong);

                    // In order for an event to be a landing/takeoff:
                    // 1. geolocation within 5 miles of an airport (Flight safety, no unregistered aircraft within 5 miles of an airport)
                    // 2. altitude diff within 500 feet (safety altitude)
                    // 3. take off and landing speed < 200 mph

                    // In order for an airport to have a takeoff for this airplane:
                    // 1. The timestamp reads must have a difference > 30 mins

                    // Airplane heading and speed is not always present in adsbevent data, which means we can't rely on it to determine landing vs takeoff
                    double tempDist = airplaneLocation.GetDistanceTo(airportLocation);
                    if (tempDist < 5){
                        if (adsbEventEntry.Altitude != null && (adsbEventEntry.Altitude - tempAirport.Elevation <= 500) && adsbEventEntry.Speed != null && adsbEventEntry.Speed <= 200){
                            //doing some reasearch, it looks like it takes the crew around 30-60 mins to get the plane checked again for next flight
                            if (adsbEventEntry.Timestamp - prevTime > TimeSpan.Parse("00:30:00")){ 
                                // logic for arrivals and departures
                                if (prevAirportLocation != null && prevAirportLocation != airportLocation){
                                    //only departure is added w/ previous timestamp
                                    flight.DepartureTime = prevTime;
                                    flight.DepartureAirport = prevAirport.Identifier;
                                }
                                else if (prevAirportLocation == airportLocation) {
                                    //arrival is added w/ previous timestamp
                                    flight.ArrivalTime = prevTime;
                                    flight.ArrivalAirport = prevAirport.Identifier;
                                }
                            }
                            prevAirportLocation = airportLocation;
                            prevTime = adsbEventEntry.Timestamp;
                            prevAirport = tempAirport;
                        }
                    }
                    if (flight.DepartureAirport != null && flight.ArrivalAirport != null){
                        outFile.WriteLine(flight.ToString());
                        flight = new Flight();
                        flight.AircraftIdentifier = key;
                    }
                }
            }
            
            outFile.Close();

        }
    }
}
