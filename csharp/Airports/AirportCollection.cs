using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ParagonCodingExercise.Airports
{
    public class AirportCollection
    {
        public List<Airport> AirportList {get; set;}
        
        public AirportCollection(List<Airport> airports)
        {
            AirportList = airports;
        }

        public static AirportCollection LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using TextReader reader = new StreamReader(filePath);
            var json = reader.ReadToEnd();

            var airports = JsonSerializer.Deserialize<List<Airport>>(json);
            return new AirportCollection(airports);
        }

        public Airport GetClosestAirport(GeoCoordinate coordinate, double? heading)
        {
            // Assign it to first airport to avoid linting errors
            Airport closestAirport = AirportList[0];
            double minDist = Double.MaxValue;
            foreach (var airport in AirportList){
                string latlong = airport.Latitude.ToString() + "," + airport.Longitude.ToString();
                GeoCoordinate airportLocation = GeoCoordinate.FromLatitudeAndLongitudeString(latlong);
                double tempDist = coordinate.GetDistanceTo(airportLocation);
                if (heading.HasValue){
                    double directToAirport = coordinate.GetBearingTo(airportLocation);
                    double tempHeading = heading.Value;
                    if (Math.Abs(directToAirport - tempHeading) <= 180.0){ //similar direction
                        if (tempDist < minDist){
                            minDist = tempDist;
                            closestAirport = airport;
                        }
                    }
                }
                else {
                    if (tempDist < minDist){
                        minDist = tempDist;
                        closestAirport = airport;
                    }
                }
            }

            return closestAirport;
        }
    }
}
