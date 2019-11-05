using DataGenerator.Generators;
using DataGenerator.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


// {
// Numer_rachunku:
// VIN: 
// Opis_czynnosci_naprawczych: 
// [{Opis: 
//     Cena: 
//     GwarancjaWDniach: }],
// Laczny_koszt: ,
// Data_przyjecia: 
// Data_realizacji:  
// Nazwa_serwisu:  
// }


namespace DataGenerator
{
    public static class WorldSettings
    {
        public const int ModelCount = 20;

        public static (int min, int max) CarBatchSizeRange = (50, 200);

        public const int MaxServicesInAPeriod = 4;
    }
    class Program
    {
        static async Task Main(string[] args)
        {

            //moment t0
            var models = CarModelGenerator.Generate(WorldSettings.ModelCount)
                .ToList();

            var initialCarsBatches = models
                .Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));

            var carsT0 = initialCarsBatches
                .SelectMany(modelGroup => modelGroup.Select(car => car));


            //transmission from t0 to t1
            var users = UserGenerator.Generate(50000).Distinct().ToList();

            var additionalCarBatches = models.Select(m => m.CreateBatch(Settings.Random.Next(5, 20)));
            var additionalCars = additionalCarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var carsT1 = carsT0.Concat(additionalCars).ToList();

            Console.WriteLine("Creating rents");
            //var rents = RentGenerator.GenerateForCarsAndUsers(carsT1, users, Settings.SystemStartDate,
            //Settings.FirstDataCollection, 10000).ToList();

            var sw = Stopwatch.StartNew();
            var rentals = await RentGenerator.Generate(carsT1, users, Settings.SystemStartDate,
                Settings.FirstDataCollection, 50000);

            Console.WriteLine($"Rent generation took {sw.Elapsed.Seconds} seconds");

            sw.Restart();
            carsT1.ForEach(c =>
            {
                c.GenerateServiceData(rentals, 
                    c.DataZakupu, 
                    Settings.FirstDataCollection,
                    Settings.FirstDataCollection);
            });
            Console.WriteLine($"Service data generation took {sw.Elapsed.Seconds} seconds");



            Console.WriteLine("finished");
        }
    }

    public static class CarExtensions
    {
        public static void GenerateServiceData(this Samochod car,
            IEnumerable<Wynajem> rentals,
            DateTime minAdmittanceDate,
            DateTime maxAdmittanceDate,
            DateTime stopGeneratingAfter)
        {
            var carRentals = rentals.Where(r => r.Vin == car.Vin);

            var serviceActions = ServiceDataModelGenerator.GenerateForCar(
                car, carRentals,
                minAdmittanceDate, maxAdmittanceDate,
                stopGeneratingAfter, Settings.Random.Next(WorldSettings.MaxServicesInAPeriod))
                .ToList();

            serviceActions.ForEach(a => car.Services.Add(a));
        }

        public static bool WasServicedDuringTimePeriod(this Samochod car, DateTime start, DateTime end)
        {
            return car.Services.Any(serv => end > serv.DataPrzyjecia && start < serv.DataRealizacji);
        }
    }
}
