using System;
using DataGenerator.Generators;
using System.Linq;
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
    class Program
    {
        static void Main(string[] args)
        {

            // moment t0
            var models = CarModelGenerator.Generate(50).ToList();

            var initialCarsBatches = models.Select(m => m.CreateBatch(Settings.Random.Next(50, 100)));
            var carsT0 = initialCarsBatches.SelectMany(modelGroup => modelGroup.Select(car => car)).ToList();


            // transmission from t0 to t1

            var users = UserGenerator.Generate(10000);
            //var additionalCarBatches = models.Select(m => m.CreateBatch(Settings.Random.Next(5, 20)));
            //var additionalCars = additionalCarBatches.SelectMany(modelGroup => modelGroup.Select(c => c));

            var carsT1 = carsT0.ToList();

            carsT1.ForEach(c =>
            {
                c.GenerateServiceData(c.DataZakupu,
                    Settings.FirstDataCollection, 
                    Settings.FirstDataCollection,
                    Settings.Random.Next(4));
            });


            Console.WriteLine("Creating rents");
            var rents = RentGenerator.GenerateForCarsAndUsers(carsT1, users, Settings.SystemStartDate,
                Settings.FirstDataCollection, 10000).ToList();


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
