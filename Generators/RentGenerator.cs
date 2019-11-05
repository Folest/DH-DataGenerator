using DataGenerator.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uzytkownik = DataGenerator.Model.Uzytkownik;

namespace DataGenerator.Generators
{
    public static class RentGenerator
    {
        public static class RentSettings
        {
            public static TimeSpan MaximalRentDuration = TimeSpan.FromDays(7);
            public static TimeSpan MinimalRentDuration = TimeSpan.FromMinutes(5);
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
                //var randomUserIndex = Settings.Random.Next(availableUsers.Count());
                var user = availableUsers.First();


                var availableCars = GetAvailableCars(cars, rentals, rentDate, rentEndDate);
                //var randomCarIndex = Settings.Random.Next(availableCars.Count());
                if (!availableCars.Any())
                    continue;


                var car = availableCars.First();


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
                if (rentals.Count % 100 == 0)
                    Console.WriteLine($"Just generated {rentals.Count} rentals");
            }

            return rentals;
        }



        public static async Task<IEnumerable<Wynajem>> Generate(
            IEnumerable<Samochod> cars,
            IEnumerable<Uzytkownik> users,
            DateTime minimalRentDate,
            DateTime maximalRentDate,
            int approximateCount)
        {
            var averagePerCar = (int)(1/((double)cars.Count() / approximateCount));
            var rentals = new LinkedList<Wynajem>();

            // pairing of cars and time frames of rentals
            var carsRentalPeriods = new Dictionary<Samochod, IEnumerable<(DateTime, DateTime)>>();
            foreach (var car in cars)
            {
                var generator = new TimePeriodGenerator(averagePerCar);
                var rentalSpans = await generator.Generate(minimalRentDate, maximalRentDate,
                    RentSettings.MinimalRentDuration, RentSettings.MaximalRentDuration);

                carsRentalPeriods.TryAdd(car, rentalSpans);
            }
            // user should not be checked and have their rentals added at the same time
            // this prevents it from happening
            var usersMutexes = users.ToDictionary(user => user, user => new Mutex());

            foreach (var car in carsRentalPeriods.Keys.AsParallel())
            {
                var rentalPeriods = carsRentalPeriods[car];

                // parallel in respect to rental periods per car
                foreach (var (start, end) in rentalPeriods.AsParallel())
                {
                    var user = usersMutexes
                        .Where(pair => !pair.Value.SafeWaitHandle.IsClosed) // users not currently under modification
                        .FirstOrDefault(kv =>
                    {
                        var rentals = kv.Key.Rentals.Select(r => (r.CzasRozpoczecia, r.CzasZakonczenia));
                        return !rentals.Any(r => r.OverlapsWith((start, end)));
                    }).Key; // which do not rent any cars in time of this 

                    if (user == default)
                        continue;

                    // if we reeally found this user
                    if(!usersMutexes[user].WaitOne(100))
                        continue;

                    var rental = new Wynajem
                    {
                        CzasRozpoczecia = start, CzasZakonczenia = end, Vin = car.Vin, Pesel = user.Pesel
                    };
                    rentals.AddLast(rental);
                    user.Rentals.Add(rental);
                    usersMutexes[user].ReleaseMutex();
                }

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