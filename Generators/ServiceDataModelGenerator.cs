using DataGenerator.Builder;
using DataGenerator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model.Json;
using DataGenerator.Model.Sql;

namespace DataGenerator.Generators
{
    public static class ServiceDataModelGenerator
    {
        public static class ServiceDataModelSettings
        {
            public const int MaxDurationDays = 30;
            public const int MaxActions = 5;

            public static IEnumerable<string> ServiceNamePool = new List<string>
            {
                "Nosacz i spółka", "Mechanix", "Naprawczy", "ASO", "Kowalski sp.zoo"
            };
        }

            //todo: This could be optimized
        public static IList<ServiceDataModel> GenerateForCar(
            Samochod car, 
            IEnumerable<Wynajem> carRentals,
            DateTime minAdmittanceDate, 
            DateTime maxAdmittanceDate,
            DateTime stopGeneratingAfter, 
            int count)
        {
            minAdmittanceDate = minAdmittanceDate > car.DataZakupu ? minAdmittanceDate : car.DataZakupu;

            var lastServiceDate = car.Services.Select(action => action.DataRealizacji)
                .OrderBy(x => x)
                .LastOrDefault();

            if (lastServiceDate != default)
            {
                minAdmittanceDate = lastServiceDate > minAdmittanceDate
                    ? lastServiceDate
                    : minAdmittanceDate;
            }

            var servicingPeriods = new List<(DateTime, DateTime)>();
            for (int i = 0, retryCounter = 0; i < count; i++)
            {
                var admittanceDate = Settings.RandomDateBetween(minAdmittanceDate, maxAdmittanceDate);
                var returnDate = admittanceDate.AddDays(Settings.Random.Next(ServiceDataModelSettings.MaxDurationDays));

                var overlappingRentOrDefault = carRentals.FirstOrDefault(r =>
                    (r.CzasRozpoczecia, r.CzasZakonczenia).OverlapsWith((admittanceDate, returnDate)));

                if (overlappingRentOrDefault != default)
                {
                    if (retryCounter++ > 100)
                    {
                        Console.WriteLine("A pessimistic scenario just occured");
                    }

                    i--;
                    continue;
                }

                retryCounter = 0;
                servicingPeriods.Add((admittanceDate, returnDate));
                if (returnDate > stopGeneratingAfter)
                {
                    break;
                }
            }

            var namePool = ServiceDataModelSettings.ServiceNamePool;

            var serviceDataModels = servicingPeriods.Select(period => new ServiceDataModelBuilder()
                .WithAdmittanceDate(period.Item1)
                .WithReturnDate(period.Item2)
                .WithServiceName(namePool.ElementAt(Settings.Random.Next(namePool.Count())))
                .WithInvoiceNumber(Settings.RandomString(10, Settings.Numbers))
                .WithVin(Settings.RandomVin())
                .WithServiceActions(ServiceActionModelGenerator.Generate(Settings.Random.Next(ServiceDataModelSettings.MaxActions)+ 1))
                .CalculateCostAndBuildModel());

            return serviceDataModels.ToList();
        }
    }
}