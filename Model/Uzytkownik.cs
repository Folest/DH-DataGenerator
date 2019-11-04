using System;

namespace DataGenerator.Model
{
    public class Uzytkownik
    {
        public string Pesel { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public bool IsMale { get; set; }
        public DateTime DataUrodzenia { get; set; }
        public DateTime DataRejestracji { get; set; }
    }
}