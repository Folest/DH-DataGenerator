using DataGenerator.Extensions;
using DataGenerator.Generators;
using DataGenerator.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataGenerator.Model.Json;
using DataGenerator.Model.Sql;

namespace DataGenerator
{
    public static class WorldSettings
    {
        public const string FileName = "Generator.sql";
        public const int ModelCount = 10;
        public const int RentRateCount = 10;

        public static (int min, int max) CarBatchSizeRange = (25, 100);

        public const int MaxServicesInAPeriod = 4;
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            if (File.Exists(WorldSettings.FileName))
            {
                File.Delete(WorldSettings.FileName);
            }

            //moment t0
            var t0Models = CarModelGenerator.Generate(WorldSettings.ModelCount)
                .ToList();

            var t0CarBatches = t0Models
                .Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));

            var t0Cars = t0CarBatches
                .SelectMany(modelGroup => modelGroup.Select(car => car))
                .ToArray();

            var t0RentRates = RentRateGenerator.Generate(WorldSettings.RentRateCount).ToList();

            // save to files
            SaveAsScripts(t0Cars, null, t0Models, null, t0RentRates);


            //transmission from t0 to t1
            var t1CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(2, 10)));
            var t1AdditionalCars = t1CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var t1Cars = t0Cars.Concat(t1AdditionalCars).ToList();
            var t1Users = UserGenerator.Generate(50).Distinct().ToList();

            Console.WriteLine("Creating rents");
            var sw = Stopwatch.StartNew();

            var t1Rentals = await RentGenerator.Generate(t1Cars, t1Users, t0RentRates, Settings.SystemStartDate,
                Settings.FirstDataCollection, 1000);


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
            SaveAsJson(t1Cars.Except(t0Cars).Select(c => c.Services).SelectMany(s => s).ToArray(), "t1");
            SaveAsScripts(t1Cars.Except(t0Cars), t1Users, null, t1Rentals, null,"t1");


            // transmission from t0 to t1
            //var t2CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(2, 10)));

            //var t2AdditionalCars = t2CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));
            //var t2AdditionalUsers = UserGenerator.Generate(50).Distinct().ToArray();

            //var t2Cars = t1Cars.Concat(t2AdditionalCars).ToArray();
            //var t2Users = t1Users.Concat(t2AdditionalUsers).Distinct().ToArray();

            //sw.Restart();
            //var t2Rentals = await RentGenerator.Generate(t2Cars, t2Users, t0RentRates, Settings.FirstDataCollection,
            //    Settings.SecondDataCollection, 100);

            //Console.WriteLine($"Rent T2 generation took {sw.Elapsed.Seconds} seconds");

            //sw.Restart();
            //t2Cars.Except(t1Cars).ToList().ForEach(c =>
            //{
            //    c.GenerateServiceData(t2Rentals,
            //        Settings.FirstDataCollection,
            //        Settings.SecondDataCollection,
            //        Settings.SecondDataCollection);
            //});
            //Console.WriteLine($"Service T2 data generation took {sw.Elapsed.Seconds} seconds");


            //SaveAsJson(t2Cars.Except(t1Cars).Select(c => c.Services).SelectMany(s => s).ToArray(), "t2");
            //SaveAsScripts(t2Cars.Except(t1Cars), t2Users.Except(t1Users), null, t2Rentals, null, "t2");


            Console.WriteLine("finished");
        }

        public static void SaveAsScripts(
            IEnumerable<Samochod> cars = null,
            IEnumerable<Uzytkownik> users = null,
            IEnumerable<ModelSamochodu> models = null,
            IEnumerable<Wynajem> rentals = null,
            IEnumerable<Cennik> rates = null,
            string periodName = "t0")
        {
            var filename = WorldSettings.FileName;
                
            File.AppendAllText(filename, $"\n\n--------------{periodName} INSERTS--------------\n\n");

            if (rates != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for RATES-------\n");
                var script = rates.ToInsert();
                File.AppendAllText(filename, script);
            }

            if (models != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for MODELS-------\n");
                var script = models.ToInsert();
                File.AppendAllText(filename, script);
            }

            if (users != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for USERS-------\n");
                var script = users.ToInsert();
                File.AppendAllText(filename, script);
            }

            if (cars != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for CARS-------\n");
                var script = cars.ToInsert();
                File.AppendAllText(filename, script);
            }

            if (rentals != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for RENTALS-------\n");
                var script = rentals.ToInsert();
                File.AppendAllText(filename , script);
            }
        }

        public static void SaveAsJson(IEnumerable<ServiceDataModel> services, string periodName = "t0")
        {
            var jsons = services.Select(s => JsonConvert.SerializeObject(s) + ",").ToArray();

            File.WriteAllLines(periodName + "Services.json", jsons);
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
