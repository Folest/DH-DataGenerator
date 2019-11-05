using DataGenerator.Generators;
using DataGenerator.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataGenerator.Extensions;
using Newtonsoft.Json;


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
            var t0Models = CarModelGenerator.Generate(WorldSettings.ModelCount)
                .ToList();

            var t0CarBatches = t0Models
                .Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));

            var t0Cars = t0CarBatches
                .SelectMany(modelGroup => modelGroup.Select(car => car))
                .ToArray();

            // save to files
            SaveAsScripts(t0Cars, null, t0Models);


            //transmission from t0 to t1
            var t1CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(20, 50)));
            var t1AdditionalCars = t1CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var t1Cars = t0Cars.Concat(t1AdditionalCars).ToList();
            var t1Users = UserGenerator.Generate(50000).Distinct().ToList();

            Console.WriteLine("Creating rents");
            var sw = Stopwatch.StartNew();


            //var t1Rentals = RentGenerator.GenerateForCarsAndUsers(t1Cars, t1Users, Settings.SystemStartDate,
            //Settings.FirstDataCollection, 10000).ToList();

            var t1Rentals = await RentGenerator.Generate(t1Cars, t1Users, Settings.SystemStartDate,
                Settings.FirstDataCollection, 20000);


            Console.WriteLine($"Rent T1 generation took {sw.Elapsed.Seconds} seconds");

            sw.Restart();
            t1Cars.ForEach(c =>
            {
                c.GenerateServiceData(t1Rentals,
                    c.DataZakupu,
                    Settings.FirstDataCollection,
                    Settings.FirstDataCollection);
            });
            Console.WriteLine($"Service T1 data generation took {sw.Elapsed.Seconds} seconds");
            
            // saving to files
            SaveAsJson(t1Cars.Select(c => c.Services).SelectMany(s => s).ToArray(), "t1");
            SaveAsScripts(t1Cars, t1Users, t0Models, t1Rentals, "t1");


            // transmission from t0 to t1
            var t2CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(20, 50)));

            var t2AdditionalCars = t2CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));
            var t2AdditionalUsers = UserGenerator.Generate(20000).Distinct().ToList();

            var t2Cars = t1Cars.Concat(t2AdditionalCars).ToList();
            var t2Users = t1Users.Concat(t2AdditionalUsers).Distinct();

            sw.Restart();
            var t2Rentals = await RentGenerator.Generate(t2Cars, t2Users, Settings.FirstDataCollection,
                Settings.SecondDataCollection, 20000);

            Console.WriteLine($"Rent T2 generation took {sw.Elapsed.Seconds} seconds");

            sw.Restart();
            t1Cars.ForEach(c =>
            {
                c.GenerateServiceData(t2Rentals,
                    Settings.FirstDataCollection,
                    Settings.SecondDataCollection,
                    Settings.SecondDataCollection);
            });
            Console.WriteLine($"Service T2 data generation took {sw.Elapsed.Seconds} seconds");

            Console.WriteLine("finished");
        }

        public static void SaveAsScripts(
            IEnumerable<Samochod> cars = null,
            IEnumerable<Uzytkownik> users = null,
            IEnumerable<ModelSamochodu> models = null,
            IEnumerable<Wynajem> rentals = null,
            string periodName = "t0")
        {
            if (models != null)
            {
                var script = models.ToInsert();
                File.WriteAllText(periodName + "Modele.sql", script);
            };

            if (users != null)
            {
                var script = users.ToInsert();
                File.WriteAllText(periodName + "Uzytkownicy.sql", script);
            };

            if (cars != null)
            {
                var script = cars.ToInsert();
                File.WriteAllText(periodName + "Samochody.sql", script);
            };

            if (rentals != null)
            {
                var script = rentals.ToInsert();
                File.WriteAllText(periodName + "Wynajmy.sql", script);
            };
        }

        public static void SaveAsJson(IEnumerable<ServiceDataModel> services, string periodName = "t0")
        {
            var jsons = services.Select(JsonConvert.SerializeObject);

            File.WriteAllLines(periodName + ".json", jsons);
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
