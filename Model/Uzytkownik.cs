using System;
using System.Collections.Generic;

namespace DataGenerator.Model
{
    public class Uzytkownik : IEquatable<Uzytkownik>
    {
        public string Pesel { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public bool IsMale { get; set; }
        public DateTime DataUrodzenia { get; set; }
        public DateTime DataRejestracji { get; set; }

        public IList<Wynajem> Rentals { get; set; } = new List<Wynajem>();

        public bool Equals(Uzytkownik other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Pesel, other.Pesel);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Uzytkownik) obj);
        }

        public override int GetHashCode()
        {
            return (Pesel != null ? Pesel.GetHashCode() : 0);
        }
    }
}