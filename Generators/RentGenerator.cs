using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataGenerator.Model;

namespace DataGenerator.Generators
{
    public static class RentGenerator
    {
        public static class RentSettings
        {
            public static TimeSpan MaximalRentDuration = TimeSpan.FromDays(7);
            public const double MinAverageSpeed = 20;
            public const double MaxAverageSpeed = 100;
            public const double MaxStandbyDurationFraction = 0.2;
        }

        public static IList<Wynajem> GenerateForCarsAndUsers(IEnumerable<Samochod> cars,
            IEnumerable<Uzytkownik> users,
            DateTime minimalRentDate,
            DateTime maximalRentDate,
            int count)
        {
            var rentals = new List<Wynajem>();
            while (rentals.Count != count)
            {
                var rentDate = Settings.RandomDateTimeBetween(minimalRentDate, maximalRentDate);
                var rentEndDate = Settings.RandomDateTimeBetween(rentDate, rentDate + RentSettings.MaximalRentDuration);

                var availableUsers = GetAvailableUsers(users, rentals, rentDate, rentEndDate);
                if (!availableUsers.Any())
                    continue;
                var user = availableUsers.ElementAt(Settings.Random.Next(availableUsers.Count()));

                var availableCars = GetAvailableCars(cars, rentals, rentDate, rentEndDate);
                if(!availableCars.Any())
                    continue;
                var car = availableCars.ElementAt(Settings.Random.Next(availableCars.Count()));

                minimalRentDate = new[] { minimalRentDate, user.DataRejestracji }.Min();

                var rent = new Wynajem();

                rent.CzasRozpoczecia = rentDate;
                rent.CzasZakonczenia = rentEndDate;

                rent.OdlegloscKm = (Settings.Random.NextDouble() *
                                   (RentSettings.MaxAverageSpeed - RentSettings.MinAverageSpeed)
                                   + RentSettings.MinAverageSpeed);

                rent.IloscZuzytegoPaliwa = rent.OdlegloscKm * car.Model.SrednieSpalanie;

                var rentTime = rent.CzasZakonczenia.Subtract(rent.CzasRozpoczecia);
                rent.CzasPostoju = new TimeSpan(
                    (long)(rentTime.Ticks * Settings.Random.NextDouble() * RentSettings.MaxStandbyDurationFraction));

                rent.Vin = car.Vin;
                rent.Pesel = user.Pesel;
                rent.Id = Guid.NewGuid();

                rentals.Add(rent);
                //Console.WriteLine($"Created rent: {rent.CzasRozpoczecia}, {rent.CzasZakonczenia}, {rent.Pesel}, {rent.Vin}");
                if(rentals.Count % 100 == 0)
                    Console.WriteLine($"Just generated {rentals.Count} rentals");
            }

            return rentals;
        }

        private static IEnumerable<Samochod> GetAvailableCars(
            IEnumerable<Samochod> cars,
            IEnumerable<Wynajem> rentals,
            DateTime rentStart, 
            DateTime rentEnd)
        {
            return cars.Where(c => c.DataZakupu < rentStart).Where(c =>
                {
                    var carRentals = rentals.Where(r => r.Vin == c.Vin);
                    return !carRentals.Any(cr =>
                        (cr.CzasRozpoczecia, cr.CzasZakonczenia).OverlapsWith((rentStart, rentEnd)));
                })
                .Where(c => !c.WasServicedDuringTimePeriod(rentStart, rentEnd));
        }

        private static IEnumerable<Uzytkownik> GetAvailableUsers(
            IEnumerable<Uzytkownik> users,
            IEnumerable<Wynajem> rentals,
            DateTime rentStart,
            DateTime rentEnd)
        {
            return users.Where(u => u.DataRejestracji > rentStart)
                .Where(u =>
                {
                    var userRents = rentals.Where(r => r.Pesel == u.Pesel);
                    return !userRents.Any(ur =>
                        (ur.CzasRozpoczecia, ur.CzasZakonczenia).OverlapsWith((rentStart, rentEnd)));
                });
        }
    }

    public static class DatePeriodsExtensions
    {
        public static bool OverlapsWith(this (DateTime start, DateTime end) period, (DateTime start, DateTime end) other)
        {
            return !(period.end < other.start && other.end < period.start);
        }
    }
}