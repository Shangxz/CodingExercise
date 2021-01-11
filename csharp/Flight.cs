using System;

namespace ParagonCodingExercise
{
    public class Flight
    {
        public string AircraftIdentifier { get; set; }

        public DateTime DepartureTime { get; set; }

        public string DepartureAirport { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string ArrivalAirport { get; set; }

        public override string ToString()
        {
            return "{" + $"\"AircraftIdentifier\":{AircraftIdentifier},\"DepartureTime\":{DepartureTime},\"DepartureAirport\":{DepartureAirport},\"ArrivalTime\":{ArrivalTime},\"ArrivalAirport\":{ArrivalAirport}" + "}";
        }
    }
}
