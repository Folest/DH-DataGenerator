using DataGenerator.Model;
using DataGenerator.Model.Sql;

namespace DataGenerator
{
    public static class UserExtensions
    {
        public static PrawoJazdy ForUser(this Uzytkownik user)
        {
            var minIssueDate = user.DataUrodzenia.AddYears(18);
            var maxIssueDate = user.DataRejestracji;

            var license = new PrawoJazdy();

            license.DataWydania = Settings.RandomDateBetween(minIssueDate, maxIssueDate);
            license.Pesel = user.Pesel;

            return license;
        }
    }
}