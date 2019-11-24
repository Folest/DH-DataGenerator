using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model.Sql;

namespace DataGenerator.Generators
{
    public class RentRateGenerator
    {
        // All those are in grosze
        private static (int min, int max) PricePerKmRange = (40, 150);
        private static (int min, int max) PricePerMinDriveRange = (0, 100);
        private static (int min, int max) PricePerMinParking = (0, 20);
        private static int CurrentId = 1;

        public static Cennik Generate()
        {
            return new Cennik
            {
                Id = CurrentId++,
                CenaPostojuMinuta = Settings.Random.Next(PricePerMinParking.min, PricePerMinParking.max),
                CenaPrzejazduKm = Settings.Random.Next(
                    PricePerKmRange.min, PricePerKmRange.max) % 10,
                CenaPrzejazduMinuta = Settings.Random.Next(
                    PricePerMinDriveRange.min, PricePerMinDriveRange.max)
            };
        }

        public static IEnumerable<Cennik> Generate(int count)
        {
            return Enumerable.Range(1, count)
                .Select(_ => Generate())
                .Append(Cennik.DefaultPriceList);
        }
    }
}