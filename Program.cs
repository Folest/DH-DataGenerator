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
        public const int ModelCount = 30;
        public const int RentRateCount = 20;

        public static (int min, int max) CarBatchSizeRange = (25, 50);

        public const int MaxServicesInAPeriod = 2;
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
            var t0Models = CarModelGenerator.Generate(WorldSettings.ModelCount, Settings.FirstDataCollection.Year - 3)
                .ToList();

            var t0CarBatches = t0Models
                .Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));

            var t0Cars = t0CarBatches
                .SelectMany(modelGroup => modelGroup.Select(car => car))
                .ToArray();

            var t0RentRates = RentRateGenerator.Generate(WorldSettings.RentRateCount).ToList();

            // save to files
            SaveAsScripts(t0Cars, null, t0Models, null, t0RentRates, OperationAreaStub.AreasOfOperation);


            //transmission from t0 to t1
            var t1CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));
            var t1AdditionalCars = t1CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var t1Cars = t0Cars.Concat(t1AdditionalCars).ToList();

            var t1Users = UserGenerator.Generate(2000).Distinct().ToList();

            var t1Licenses = t1Users.Select(u => u.GenerateForUser());

            Console.WriteLine("Creating rents");
            var sw = Stopwatch.StartNew();

            var t1Rentals = await RentGenerator.Generate(t1Cars, t1Users, t0RentRates, Settings.SystemStartDate, Settings.FirstDataCollection, 20000);

            t1Cars.ForEach(c =>
            {
                c.GenerateServiceData(t1Rentals,
                    c.DataZakupu,
                    Settings.FirstDataCollection,
                    Settings.FirstDataCollection);
            });

            // saving to files
            SaveAsJson(t1Cars.Select(c => c.Services).SelectMany(s => s).ToArray(), "t1");
            SaveAsScripts(t1AdditionalCars, t1Users, null, t1Rentals, null, null, t1Licenses, "t1");

            // delete all the service action infromation about t1 cars
            t1Cars.ForEach(c => c.Services = new List<ServiceDataModel>());

            // transmission from t0 to t1
            var t2CarBatches = t0Models.Select(m => m.CreateBatch(Settings.Random.Next(WorldSettings.CarBatchSizeRange.min, WorldSettings.CarBatchSizeRange.max)));

            var t2AdditionalCars = t2CarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));
            var t2AdditionalUsers = UserGenerator.Generate(2000).Distinct().ToArray();

            var t2Cars = t1Cars.Concat(t2AdditionalCars).ToArray();
            var t2Users = t1Users.Concat(t2AdditionalUsers).Distinct().ToArray();

            sw.Restart();

            var t2Rentals = await RentGenerator.Generate(t2Cars, t2Users, t0RentRates, Settings.FirstDataCollection,
                Settings.SecondDataCollection, 30000);

            Console.WriteLine($"Rent T2 generation took {sw.Elapsed.Seconds} seconds");

            sw.Restart();
            t2Cars.ToList().ForEach(c =>
            {
                c.GenerateServiceData(t2Rentals,
                    Settings.FirstDataCollection,
                    Settings.SecondDataCollection,
                    Settings.SecondDataCollection);
            });
            Console.WriteLine($"Service T2 data generation took {sw.Elapsed.Seconds} seconds");


            SaveAsJson(t2Cars.Select(c => c.Services).SelectMany(s => s).ToArray(), "t2");
            SaveAsScripts(t2AdditionalCars, t2AdditionalUsers, null, t2Rentals, null, null, null, "t2");


            Console.WriteLine("finished");
        }

        public static void SaveAsScripts(
            IEnumerable<Samochod> cars = null,
            IEnumerable<Uzytkownik> users = null,
            IEnumerable<ModelSamochodu> models = null,
            IEnumerable<Wynajem> rentals = null,
            IEnumerable<Cennik> rates = null,
            IEnumerable<ObszarDzialalnosci> areas = null,
            IEnumerable<PrawoJazdy> licenses = null,
            string periodName = "t0")
        {
            var filename = WorldSettings.FileName;

            File.AppendAllText(filename, $"--------------{periodName} INSERTS--------------\n\n");

            if (areas != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for AREAS-------\n");
                var script = areas.ToInsert();
                File.AppendAllText(filename, script);
            }

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

            if (licenses != null)
            {
                File.AppendAllText(filename, $"\n\n-------{periodName} INSERTS for LICENSES-------\n");
                var script = licenses.ToInsert();
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
                File.AppendAllText(filename, script);
            }
        }

        public static void SaveAsJson(IEnumerable<ServiceDataModel> services, string periodName = "t0") 
        {
            List<string> jsons = services.Select(s => JsonConvert.SerializeObject(s) + ",").ToList();
            
            jsons[jsons.Count - 1] = jsons.Last().Remove(jsons.Last().Length - 1);
            File.WriteAllLines(periodName + "Services.json", jsons.Prepend("[").Append("]"));
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
