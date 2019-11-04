using System.Collections.Generic;
using System.Linq;
using DataGenerator.Builder;
using DataGenerator.Model;

namespace DataGenerator.Generators
{
    public static class ServiceActionModelGenerator
    {
        internal static class ServiceActionModelSettings
        {
            public static Dictionary<string, (int, int)> DescriptionPriceRange { get; } =
                new Dictionary<string, (int, int)> //todo: change ints to decimals
                {
                    {"Wymiana uszczelki", (100, 600)},
                    {"Wymiana oleju", (200, 500)},
                    {"Naprawa amortyzatora", (300, 1000)},
                    {"Wymiana klocków hamulcowych", (200, 500)},
                    {"Wymiana tarcz hamulcowych", (300, 800)},
                    {"Wymiana zderzaka", (500, 2000)},
                    {"Wymiana szyby", (500, 2000)},
                    {"Regulacja hamulca ręcznego", (50, 200)},
                    {"Wymiana płynu hamulcowego", (100, 300)},
                    {"Wymiana wycieraczek", (50, 200)}
                };

            public static int ShortestWarrantyDays { get; } = 30;
            public static int LongestWarrantyDays { get; } = 730;
        }

        public static ServiceActionModel Generate()
        {
            var mb = new ServiceActionModelBuilder();

            var minWarrantyDays = ServiceActionModelSettings.ShortestWarrantyDays;
            var maxWarrantyDays = ServiceActionModelSettings.LongestWarrantyDays;

            var descriptions = ServiceActionModelSettings.DescriptionPriceRange.Keys;

            var description = descriptions.ElementAt(Settings.Random.Next(descriptions.Count));
            var priceRange = ServiceActionModelSettings.DescriptionPriceRange[description];

            mb.WithWarranty(Settings.Random.Next(minWarrantyDays, maxWarrantyDays));
            mb.WithDescription(description);
            mb.WithCost(priceRange.Item1 + Settings.Random.Next(priceRange.Item1, priceRange.Item2));

            return mb.BuildModel();
        }

        public static IEnumerable<ServiceActionModel> Generate(int count) =>
            Enumerable.Range(0, count).Select(_ => Generate()).ToList();
    }
}