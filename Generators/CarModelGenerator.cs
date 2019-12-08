using System;
using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model;
using DataGenerator.Model.Sql;

namespace DataGenerator.Generators
{
    public class CarModelGenerator
    {
        public static class CarModelSettings
        {
            public const int MinModelProductionStartYear = 2013;
            
            private static class CarTypes
            {
                public const string Sedan = "Sedan";
                public const string SedanPremium = "Sedan Premium";
                public const string Hatchback = "Hatchback";
                public const string HatchbackPremium = "Hatchback premium";
                public const string HatchbackMini = "Mini hatchback";
                public const string SUVMini = "Mini SUV";
                public const string SUV = "SUV";
            }

            public static Dictionary<string, IEnumerable<(string model, string typ)>> ProducersModels =
                new Dictionary<string, IEnumerable<(string, string)>>
                {
                    {"Skoda", new[] {("Octavia", CarTypes.Sedan), ("Fabia", CarTypes.HatchbackMini), ("Superb", CarTypes.SedanPremium) }},
                    {"Toyota", new[] {("CR-V", CarTypes.SUVMini), ("Corolla", CarTypes.HatchbackMini), ("Auris", CarTypes.Hatchback), ("Auris", CarTypes.Sedan) }},
                    {"Renault", new[] {("Clio", CarTypes.Hatchback), ("Zoe", CarTypes.HatchbackMini)}},
                    {"Opel", new[] {("Astra", CarTypes.HatchbackPremium), ("Astra", CarTypes.Sedan), ("Corsa",CarTypes.Hatchback), ("Ampera", CarTypes.Hatchback)}}
                };

            public static IEnumerable<string> DrivePool = new[] { "przedni", "4x4", "tylny" };

            public static Dictionary<string, IDictionary<string, ((int, int) powerRange, (double, double) consumption)>> EngineTypeSignaturesPowerRanges =
                new Dictionary<string, IDictionary<string, ((int, int), (double, double))>>
                {
                    {
                        "Benzyna", new Dictionary<string, ((int, int), (double, double))>
                        {
                            {"1.0", ((60, 80), (4.0, 7.0))},
                            {"1.2", ((70, 100), (5.0, 7.5))},
                            {"1.4", ((90, 120), (5.5, 8.0))},
                            {"1.8", ((110, 140), (6.5, 9.0)) },
                            {"2.0", ((130, 180), (7.5, 11)) },
                            {"2.5", ((150, 250), (8, 12)) }
                        }
                    },
                    {
                        "Diesel", new Dictionary<string, ((int, int), (double, double))>
                        {
                            {"1.2", ((80, 110), (5.0,7.0))},
                            {"1.4", ((100, 130),(5.5,7.5))},
                            {"1.8", ((110, 150),(6.5,8))},
                            {"2.0", ((140, 180),(7,9))}
                        }
                    },
                    {
                        "Hybryda", new Dictionary<string, ((int, int), (double, double))>
                        {
                            {"0.8", ((60, 80)  , (3.5,5))},
                            {"1.0", ((70, 100) , (3.5,6))},
                            {"1.2", ((90, 120) , (4,6.5))},
                            {"1.4", ((100, 140), (4.5,7))},
                            {"1.6", ((120, 180), (5,7))}
                        }
                    },
                };

            public const int MinimalPrice = 40000;
            public const int MaximalPrice = 150000;
            public const int PriceVariationPercent = 20;
        }

        public static (DateTime oldest, DateTime newest) PurchaseRange = (Settings.SystemStartDate,
            Settings.FirstDataCollection);

        private static int Count = 0;

        public static ModelSamochodu Generate(int maxModelProductionStartYear)
        {
            var model = new ModelSamochodu();

            model.Id = Count++;

            var producers = CarModelSettings.ProducersModels.Keys;
            model.Marka = producers.ElementAt(Settings.Random.Next(producers.Count));

            var producerModels = CarModelSettings.ProducersModels[model.Marka];

            var modelTyp = producerModels.ElementAt(Settings.Random.Next(producerModels.Count()));
            model.Nazwa = modelTyp.model;
            model.Typ = modelTyp.typ;

            model.Naped =
                CarModelSettings.DrivePool.ElementAt(Settings.Random.Next(CarModelSettings.DrivePool.Count()));

            var engines = CarModelSettings.EngineTypeSignaturesPowerRanges;

            model.TypSilnika = engines.Keys.ElementAt(Settings.Random.Next(engines.Keys.Count));

            var engineTypeSignatures = engines[model.TypSilnika];
            model.OznaczenieSilnika = engineTypeSignatures.Keys.ElementAt(Settings.Random.Next(engineTypeSignatures.Keys.Count));

            var powerRange = engines[model.TypSilnika][model.OznaczenieSilnika].powerRange;
            var power = Settings.Random.Next(powerRange.Item1, powerRange.Item2);
            var powerRoundedDown = power / 10 * 10;

            model.Moc = powerRoundedDown;

            var consumptionRange = engines[model.TypSilnika][model.OznaczenieSilnika].consumption;
            var consumption = Settings.Random.NextDouble() * (consumptionRange.Item2 - consumptionRange.Item2) +
                              consumptionRange.Item1;

            model.SrednieSpalanie = consumption;

            model.RokRozpoczeciaProdukcji = Settings.Random.Next(CarModelSettings.MinModelProductionStartYear, maxModelProductionStartYear + 1);

            var minPrice = Settings.Random.Next(CarModelSettings.MinimalPrice, CarModelSettings.MaximalPrice);
            model.PriceRange = (minPrice, minPrice + (minPrice * CarModelSettings.PriceVariationPercent / 100));

            return model;
        }

        public static IEnumerable<ModelSamochodu> Generate(int count, int maxModelProductionStartYear)
        {
            return Enumerable.Range(0, count).Select(_ => Generate(maxModelProductionStartYear));
        }
    }
}