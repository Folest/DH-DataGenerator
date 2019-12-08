using System;
using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model.Sql;

namespace DataGenerator.Generators
{
    class UserGenerator
    {
        public static class UserSettings
        {
            public static int MaxAge = 80;
            public static int MinAge = 19;

            public static IEnumerable<string> NamePool = new[]
            {
                "Adam", "Ewa", "Jan", "Helena", "Szymon", "Piotr", "Grzegorz", "Anna", "Wojciech", "Jerzy", "Zdzislaw",
                "Janusz", "Adrian", "Julia", "Ignacy", "Aleksander", "Damian", "Jasiu", "Krzysztof", "Robert"
            };

            public static IEnumerable<string> SurnamePool = new[]
            {
                "Nowak", "Kowalski", "Wisniewski", "Wojcik", "Kowalczyk", "Kaminski", "Lewandowski", "Szymanski",
                "Wozniak", "Dabrowski",
                "Kozlowski", "Jankowski", "Mazur", "Wojciechowski", "Kwiatkowski", "Krawczyk", "Kaczmarek",
                "Piotrowski", "Grabowski"
            };
        }

        public static (DateTime oldest, DateTime newest) RegistrationDate = (Settings.SystemStartDate, Settings.FirstDataCollection);

        public static Uzytkownik Generate()
        {
            var user = new Uzytkownik
            {
                DataRejestracji = Settings.RandomDateBetween(RegistrationDate.oldest, RegistrationDate.newest)
            };

            user.DataUrodzenia = Settings.RandomDateBetween(user.DataRejestracji.AddYears(-UserSettings.MaxAge),
                user.DataRejestracji.AddYears(-UserSettings.MinAge));

            var surnamePool = UserSettings.SurnamePool;
            user.Nazwisko = surnamePool.ElementAtOrDefault(Settings.Random.Next(surnamePool.Count()));

            user.IsMale = Settings.Random.Next(2) == 1;

            var namePool = UserSettings.NamePool.Where(name => (name.LastOrDefault() != 'a') == user.IsMale);
            user.Imie = namePool.ElementAtOrDefault(Settings.Random.Next(namePool.Count()));

            user.Pesel = $"{user.DataUrodzenia.Year.ToString().Substring(2, 2)}" +
                         $"{(user.DataUrodzenia.Month < 10 ? "0" + user.DataUrodzenia.Month : user.DataUrodzenia.Month.ToString())}" +
                         $"{(user.DataUrodzenia.Day < 10 ? "0" + user.DataUrodzenia.Day : user.DataUrodzenia.Day.ToString())}" +
                         $"{Settings.RandomString(5, Settings.Numbers)}";

            return user;
        }

        public static IEnumerable<Uzytkownik> Generate(int count)
        {
            return Enumerable.Range(0, count).Select(_ => Generate());
        }
    }
}
