using System;
using System.Diagnostics;
using DataGenerator.Generators;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataGenerator.Model;


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
                .SelectMany(modelGroup => modelGroup.Select(car => car))
                .ToList();


            //transmission from t0 to t1
            var users = UserGenerator.Generate(50000).Distinct().ToList();

            var additionalCarBatches = models.Select(m => m.CreateBatch(Settings.Random.Next(5, 20)));
            var additionalCars = additionalCarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var carsT1 = carsT0.ToList();

            carsT1.ForEach(c =>
            {
                c.GenerateServiceData(c.DataZakupu,
                    Settings.FirstDataCollection,
                    Settings.FirstDataCollection,
                    Settings.Random.Next(4));
            });


            Console.WriteLine("Creating rents");
            //var rents = RentGenerator.GenerateForCarsAndUsers(carsT1, users, Settings.SystemStartDate,
            //Settings.FirstDataCollection, 10000).ToList();

            var sw = Stopwatch.StartNew();
            var result = await RentGenerator.Generate(carsT1, users, Settings.SystemStartDate,
                Settings.FirstDataCollection, 20000);

            Console.WriteLine($"Generation took {sw.Elapsed.Seconds} seconds");

            Console.WriteLine("finished");
        }
    }

    public static class CarExtensions
    {
        public static void GenerateServiceData(this Samochod car,
            DateTime minAdmittanceDate,
            DateTime maxAdmittanceDate,
            DateTime stopGeneratingAfter,
            int count)
        {
            var serviceActions = ServiceDataModelGenerator.GenerateForCar(car,
                minAdmittanceDate, maxAdmittanceDate,
                stopGeneratingAfter, count).ToList();
            serviceActions.ForEach(a => car.Services.Add(a));
        }

        public static bool WasServicedDuringTimePeriod(this Samochod car, DateTime start, DateTime end)
        {
            return car.Services.Any(serv => end > serv.DataPrzyjecia && start < serv.DataRealizacji);
        }
    }
}
