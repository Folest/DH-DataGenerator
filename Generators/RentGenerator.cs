using DataGenerator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using DataGenerator.Model.Sql;
using Uzytkownik = DataGenerator.Model.Sql.Uzytkownik;

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
            public const double MaxParkingDurationFraction = 0.2;
        }

        public static async Task<IEnumerable<Wynajem>> Generate(
            IEnumerable<Samochod> cars,
            IEnumerable<Uzytkownik> users,
            IEnumerable<Cennik> rates,
            DateTime minimalRentDate,
            DateTime maximalRentDate,
            int approximateCount)
        {
            var averagePerCar = (int)(1 / ((double)cars.Count() / approximateCount));
            if (averagePerCar == 0)
            {
                averagePerCar = 1;
            }
            var rentals = new LinkedList<Wynajem>();

            // pairing of cars and time frames of rentals
            var carsRentalPeriods = new Dictionary<Samochod, IEnumerable<(DateTime, DateTime)>>();
            foreach (var car in cars)
            {
                var generator = new TimePeriodGenerator(averagePerCar);

                minimalRentDate = new[] { car.DataZakupu, minimalRentDate }.Max();
                var rentalSpans = await generator.Generate(minimalRentDate, maximalRentDate,
                    RentSettings.MinimalRentDuration, RentSettings.MaximalRentDuration);

                carsRentalPeriods.TryAdd(car, rentalSpans);
            }
            // user should not be checked and have their rentals added at the same time
            // this prevents it from happening
            var usersMutexes = users.ToDictionary(user => user, user => new Mutex());

            foreach (var car in carsRentalPeriods.Keys)
            {
                var rentalPeriods = carsRentalPeriods[car];

                // parallel in respect to rental periods per car
                foreach (var (start, end) in rentalPeriods.AsParallel())
                {
                    var user = usersMutexes
                        .AsParallel()
                        .Where(pair => !pair.Value.SafeWaitHandle.IsClosed) // users not currently under modification
                        .FirstOrDefault(kv =>
                    {
                        var rentals = kv.Key.Rentals.Select(
                            r => (r.CzasRozpoczecia, r.CzasZakonczenia));
                        return kv.Key.DataRejestracji < start 
                        && !rentals.Any(r => r.OverlapsWith((start, end)));
                    }).Key; // which do not rent any cars in time of this 

                    if (user == default)
                        continue;

                    // if we reeally found this user
                    if (!usersMutexes[user].WaitOne(200))
                        continue;

                    var rental = CreateRent(start, end, car, user, rates.ToList());
                    rentals.AddLast(rental);
                    user.Rentals.Add(rental);
                    usersMutexes[user].ReleaseMutex();
                }
            }

            return rentals;
        }

        private static Wynajem CreateRent(DateTime start, DateTime end, Samochod car, Uzytkownik user,
            IList<Cennik> cenniki)
        {
            var rent = new Wynajem
            {
                CzasRozpoczecia = start,
                CzasZakonczenia = end,
                OdlegloscKm = (Settings.Random.NextDouble() *
                               (RentSettings.MaxAverageSpeed - RentSettings.MinAverageSpeed)
                               + RentSettings.MinAverageSpeed)
            };

            rent.IloscZuzytegoPaliwa = rent.OdlegloscKm * car.Model.SrednieSpalanie;

            var rentTime = rent.CzasZakonczenia.Subtract(rent.CzasRozpoczecia);

            var parkingFraction = Settings.Random.NextDouble() * RentSettings.MaxParkingDurationFraction;
            rent.CzasPostojuMin = (int)(rentTime.TotalMinutes * parkingFraction);

            rent.Cennik = cenniki.ElementAt(Settings.Random.Next(cenniki.Count()));
            rent.Vin = car.Vin;
            rent.Pesel = user.Pesel;
            
            return rent;
        }
    }

    public static class DatePeriodsExtensions
    {
        public static bool OverlapsWith(this (DateTime start, DateTime end) period, (DateTime start, DateTime end) other)
        {
            return !(period.start > other.end || other.start > period.end);
        }
    }
}