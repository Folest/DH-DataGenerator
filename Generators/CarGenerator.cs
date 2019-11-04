using System;
using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model;

namespace DataGenerator.Generators
{
    public static class CarGenerator
    {
        internal static class CarSettings
        {
            public static IEnumerable<string> ColorPool = new[] {"Czerwony", "Niebieski", "Szary", "Czarny", "Srebrny"};

            public static IEnumerable<char> VinCharPool = Settings.UpperCases.Where(c => c != 'O' && c != 'Q' && c != 'I').ToList();

        }

        public static (DateTime oldest, DateTime newest) PurchaseDateRange =
            (Settings.SystemStartDate, Settings.FirstDataCollection); 

        public static IEnumerable<Samochod> CreateBatch(this ModelSamochodu model, int count)
        {
            var purchaseDate = Settings.RandomDateBetween(PurchaseDateRange.oldest, PurchaseDateRange.newest);
            var purchasePrice = Settings.Random.Next(model.PriceRange.Item1, model.PriceRange.Item2);
            var automaticTransmission = Settings.Random.Next() % 2 == 0;

            var cars = Enumerable.Range(0, count).Select(_ => new Samochod
            {
                Model = model,
                CenaZakupu = purchasePrice,
                DataZakupu = purchaseDate,
                SkrzyniaAutomatyczna = automaticTransmission,
                Kolor = CarSettings.ColorPool.ElementAt(Settings.Random.Next(CarSettings.ColorPool.Count())),
                Vin = Settings.RandomVin(),
                Dostepny = true,
                ObszaryDzialalnosciNazwa = "TODO"
            });

            return cars;
        }
    }
}
