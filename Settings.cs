using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace DataGenerator
{
    public static class Settings
    {
        public static Random Random = new Random();
        public const int OutputCount = 10;
        public const int MinCost = 200;
        public const int MaxCost = 50000;

        //important: THIS IS THE T0, T1 and T2
        public static DateTime SystemStartDate = new DateTime(2015, 1, 1);
        public static DateTime FirstDataCollection = new DateTime(2017, 1, 1);
        public static DateTime SecondDataCollection = new DateTime(2019, 1, 1);

        public const int MinDayCount = 1;
        public const int MaxDayCount = 2;

        public static IEnumerable<char> LowerCases = Enumerable.Range('a', 'z' - 'a' + 1)
            .Select(code => (char) code)
            .ToList();

        public static IEnumerable<char> UpperCases = Enumerable.Range('A', 'Z' - 'A' + 1)
            .Select(code => (char) code)
            .ToList();

        public static IEnumerable<char> Numbers = Enumerable.Range('0', '9' - '0' + 1)
            .Select(code => (char) code)
            .ToList();

        public static IEnumerable<char> VinCharPool = UpperCases.Where(c => c != 'O' && c != 'Q' && c != 'I')
            .Concat(Numbers)
            .ToList();

        //todo: this could be an extension on enumerable of char
        public static string RandomString(int length, IEnumerable<char> charPool) =>
            Enumerable.Range(0, length)
                .Select(_ => charPool.ElementAtOrDefault(Settings.Random.Next() % charPool.Count()))
                .Aggregate(new StringBuilder(), (sb, ch) => sb.Append(ch))
                .ToString();

        public static string RandomVin() => RandomString(17, VinCharPool);

        public static DateTime RandomDateBetween(DateTime oldest, DateTime newest)
        {
            if (oldest > newest)
            {
                throw new Exception("Wrong argument order");
            }

            var maxDays = newest.Subtract(oldest).Days;

            return oldest.AddDays(Random.Next() % maxDays);
        }

        public static DateTime RandomDateTimeBetween(DateTime oldest, DateTime newest)
        {
            if (oldest > newest)
            {
                throw new Exception("Wrong argument order");
            }
            var maxDuration = newest.Subtract(oldest).Ticks;
            var duration = (long)(Random.NextDouble() * long.MaxValue) % maxDuration;

            return oldest.AddTicks(duration);
        }
    }
}