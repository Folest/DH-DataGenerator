using DataGenerator.Model;
using System;
using DataGenerator.Model.Sql;

namespace DataGenerator.Generators
{
    public static class DrivingLicenseGenerator
    {
        public static class DrivingLicenseSettings
        {
            public const int MinimalDrivingAge = 18;
        }

        private static long Id { get; set; }

        public static PrawoJazdy GenerateForUser(this Uzytkownik user)
        {
            var minimalIssuingDate = user.DataUrodzenia.AddYears(DrivingLicenseSettings.MinimalDrivingAge);

            return new PrawoJazdy
            {
                DataWydania = Settings.RandomDateBetween(minimalIssuingDate, user.DataRejestracji),
                Id = Id++,
                Pesel = user.Pesel
            };
        }
    }
}