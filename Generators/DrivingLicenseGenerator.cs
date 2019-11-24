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

        private static int Id { get; set; }

        public static PrawoJazdy GenerateForUser(Uzytkownik user, DateTime maximalIssuingDate)
        {
            var minimalIssuingDate = user.DataUrodzenia.AddYears(DrivingLicenseSettings.MinimalDrivingAge);

            return new PrawoJazdy
            {
                DataWydania = Settings.RandomDateBetween(minimalIssuingDate, maximalIssuingDate),
                Id = Id++,
                Pesel = user.Pesel
            };
        }
    }
}